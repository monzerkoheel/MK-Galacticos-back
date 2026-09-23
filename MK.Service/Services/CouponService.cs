using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Coupons;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;


namespace MK.Service.Services
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<ApplicationUser> _userManager;
        public CouponService(IUnitOfWork unitOfWork, 
                             IMapper mapper, 
                             ICurrentUserService currentUserService, 
                             UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUserService;
            _userManager = userManager;
        }

        public async Task<OperationResultModel> AddCouponAsync(AddCouponDto dto)
        {
            ValidateCoupon(dto);

            var coupon = _mapper.Map<Coupon>(dto);
            await CheckCouponDuplicatesAsync(coupon);

            coupon.CreatedAt = DateTime.UtcNow;
            coupon.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Coupon>().AddAsync(coupon);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.Coupon_AddedSuccessfully
            };
        }

        public async Task<OperationResultModel> EditCouponAsync(int couponId, EditCouponDto dto)
        {
            var couponSpec = new BaseSpecification<Coupon>(x => x.Id == couponId);
            couponSpec.AddInclude(x => x.Translations);
            var coupon = await _unitOfWork.Repository<Coupon>().GetFirstOrDefaultAsync(couponSpec);

            if (coupon is null)
                throw new NotFoundException(Messages.Coupon_NotFound);

            _mapper.Map(dto, coupon);
            await CheckCouponDuplicatesAsync(coupon);
            coupon.UpdatedAt = DateTime.UtcNow;

            if (dto.Translations is not null)
            {
                foreach (var translationDto in dto.Translations)
                {
                    var translation = coupon.Translations.FirstOrDefault(x => x.Language == translationDto.Language);

                    if (translation is null)
                        throw new BadRequestException(string.Format(Messages.Coupon_TranslationNotFound, translationDto.Language));

                    _mapper.Map(translationDto, translation);
                }
            }

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.Coupon_UpdatedSuccessfully
            };
        }

        public async Task<OperationResultModel> DeleteCouponAsync(int couponId)
        {
            var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(couponId);
            if (coupon is null)
                throw new NotFoundException(Messages.Coupon_NotFound);

            var userCouponSpec = new BaseSpecification<UserCoupon>(
                x => x.CouponId == couponId);

            var userCoupons = await _unitOfWork.Repository<UserCoupon>().ListAsync(userCouponSpec);

            var hasValidUserCoupons = userCoupons.Any(x => !x.IsUsed && x.ExpirationDate > DateTime.UtcNow);

            if (hasValidUserCoupons)
                throw new BadRequestException(Messages.Coupon_CannotDeleteWithValidUserCoupons);

            _unitOfWork.Repository<UserCoupon>().DeleteRange(userCoupons);

            _unitOfWork.Repository<Coupon>().Delete(coupon);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.Coupon_DeletedSuccessfully
            };
        }

        public async Task<IReadOnlyList<CouponDto>?> GetAllCouponsAsync()
        {
            var couponSpec = new BaseSpecification<Coupon>();
            couponSpec.AddInclude(x => x.Translations);
            var coupons = await _unitOfWork.Repository<Coupon>().ListAsync(couponSpec);
            return _mapper.Map<IReadOnlyList<CouponDto>>(coupons);
        }

        public async Task<IReadOnlyList<CouponDto>?> GetAllActiveCouponsAsync()
        {
            var couponSpec = new BaseSpecification<Coupon>(x => x.UserVisibility == Visibility.Active);
            couponSpec.AddInclude(x => x.Translations);
            var coupons = await _unitOfWork.Repository<Coupon>().ListAsync(couponSpec);
            return _mapper.Map<IReadOnlyList<CouponDto>>(coupons);
        }

        public async Task<CouponDetailsDto> GetCouponDetailsByIdAsync(int couponId)
        {
            var couponSpec = new BaseSpecification<Coupon>(x => x.Id == couponId);
            couponSpec.AddInclude(x => x.Translations);
            var coupon = await _unitOfWork.Repository<Coupon>().GetFirstOrDefaultAsync(couponSpec);

            if (coupon is null)
                throw new NotFoundException(Messages.Coupon_NotFound);

            var couponAuditSpec = new BaseSpecification<AuditLog>(x => x.EntityName == nameof(Coupon) && 
                                                                  x.EntityId == couponId.ToString());
            couponAuditSpec.AddOrderByDes(x => x.CreatedAt);
            var couponAuditLogs = await _unitOfWork.Repository<AuditLog>().ListAsync(couponAuditSpec);

            var result = _mapper.Map<CouponDetailsDto>(coupon);
            result.AuditLogs = _mapper.Map<ICollection<CouponAuditDto>>(couponAuditLogs);

            return result;
        }

        public async Task<OperationResultModel> AssignUserCouponAsync(int couponId)
        {
            var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(couponId);

            if (coupon is null || coupon.UserVisibility != Visibility.Active)
                throw new NotFoundException(Messages.Coupon_NotFound);

            var user = await _userManager.FindByIdAsync(_currentUser.UserId!);
            if (user is null)
                throw new NotFoundException(Messages.AccountNotFound);

            if(user.LoyaltyPointsBalance < coupon.RequiredPoints)
                throw new BadRequestException(Messages.Coupon_LoyaltyPointsNotEnough);

            var userCoupon = CreateUserCoupon(user.Id, couponId);
            userCoupon.AcquisitionType = CouponAcquisitionType.Points;

            await _unitOfWork.Repository<UserCoupon>().AddAsync(userCoupon);
            user.LoyaltyPointsBalance -= coupon.RequiredPoints.Value;
            
            await _unitOfWork.Repository<LoyaltyPointTransaction>().AddAsync(new LoyaltyPointTransaction
            {
                UserId = user.Id,
                Points = coupon.RequiredPoints.Value,
                Type = LoyaltyPointTransactionType.Redeem,
                Reason = LoyaltyTransactionReason.RewardRedemption,
                CouponId = couponId,
                CreatedAt = DateTime.UtcNow
                });

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.Coupon_AssignedSuccessfully
            };
        }

        public async Task<OperationResultModel> AssignCouponToUserAsync(string userId, int couponId)
        {
            var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(couponId);
            if (coupon is null)
                throw new NotFoundException(Messages.Coupon_NotFound);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                throw new NotFoundException(Messages.AccountNotFound);

            var userCoupon = CreateUserCoupon(user.Id, couponId);
            userCoupon.AcquisitionType = CouponAcquisitionType.Gift;

            await _unitOfWork.Repository<UserCoupon>().AddAsync(userCoupon);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.Coupon_AssignedSuccessfully
            };
        }

        public async Task AssignWelcomeCouponAsync(string userId)
        {
            var wellcomeCouponSpec = new BaseSpecification<CouponTranslation>(x => x.Language == Language.English && x.Name == "Welcome Coupon");
            var wellcomeCoupon = await _unitOfWork.Repository<CouponTranslation>().GetFirstOrDefaultAsync(wellcomeCouponSpec);
            if (wellcomeCoupon is null)
                return;

            var _userCouponRepo = _unitOfWork.Repository<UserCoupon>();
            var couponSpec = new BaseSpecification<UserCoupon>(x => x.CouponId == wellcomeCoupon.CouponId && x.UserId == userId);
            if (await _userCouponRepo.AnyAsync(couponSpec))
                return;
            var coupon = CreateUserCoupon(userId, wellcomeCoupon.CouponId);
            coupon.AcquisitionType = CouponAcquisitionType.Gift;
            coupon.ExpirationDate = DateTime.UtcNow.AddDays(3);
            await _userCouponRepo.AddAsync(coupon);
            await _unitOfWork.CompleteAsync();
        }

        #region Helper Methods
        private void ValidateCoupon(AddCouponDto dto)
        {
            switch (dto.CouponType)
            {
                case CouponType.Percentage:

                    if (!dto.DiscountPercentage.HasValue)
                        throw new BadRequestException(Messages.Coupon_DiscountPercentageRequired);

                    if (dto.DiscountPercentage <= 0 || dto.DiscountPercentage > 100)
                        throw new BadRequestException(Messages.Coupon_DiscountPercentageRange);

                    if (dto.DiscountAmount.HasValue)
                        throw new BadRequestException(Messages.Coupon_DiscountAmountNotAllowedForPercentage);

                    break;

                case CouponType.FixedAmount:

                    if (!dto.DiscountAmount.HasValue)
                        throw new BadRequestException(Messages.Coupon_DiscountAmountRequired);

                    if (dto.DiscountAmount <= 0)
                        throw new BadRequestException(Messages.Coupon_DiscountAmountGreaterThanZero);

                    if (dto.DiscountPercentage.HasValue)
                        throw new BadRequestException(Messages.Coupon_DiscountPercentageNotAllowedForFixedAmount);

                    break;

                case CouponType.FreeShipping:
                case CouponType.FreeShippingUpgrade:

                    if (dto.DiscountAmount.HasValue || dto.DiscountPercentage.HasValue)
                        throw new BadRequestException(Messages.Coupon_DiscountValuesNotAllowedForShipping);

                    break;

                default:
                    throw new BadRequestException(Messages.Coupon_InvalidType);
            }

            if (dto.MinimumOrderAmount.HasValue && dto.MinimumOrderAmount <= 0)
            {
                throw new BadRequestException(Messages.Coupon_MinimumOrderAmountGreaterThanZero);
            }

            if (dto.MaximumDiscountAmount.HasValue && dto.MaximumDiscountAmount <= 0)
            {
                throw new BadRequestException(Messages.Coupon_MaximumDiscountAmountGreaterThanZero);
            }

            if (dto.RequiredPoints.HasValue && dto.RequiredPoints <= 0)
            {
                throw new BadRequestException(Messages.Coupon_RequiredPointsGreaterThanZero);
            }

            ValidateTranslations(dto.Translations);
        }
        private void ValidateTranslations(ICollection<AddCouponTranslationDto> translations)
        {
            if (translations is null || translations.Count < 3)
                throw new BadRequestException(Messages.Coupon_AllTranslationsRequired);

            var duplicatedLanguages = translations.GroupBy(x => x.Language).Any(x => x.Count() > 1);

            if (duplicatedLanguages)
                throw new BadRequestException(Messages.Coupon_DuplicateTranslations);

            foreach (var translation in translations)
            {
                if (string.IsNullOrWhiteSpace(translation.Name))
                    throw new BadRequestException(string.Format(Messages.Coupon_TranslationNameRequired, translation.Language));

                if (string.IsNullOrWhiteSpace(translation.Description))
                    throw new BadRequestException(string.Format(Messages.Coupon_TranslationDescriptionRequired, translation.Language));
            }
        }
        private async Task CheckCouponDuplicatesAsync(Coupon newCoupon)
        {
            var existingCoupons = await _unitOfWork.Repository<Coupon>().GetAllAsync();

            foreach (var existingCoupon in existingCoupons)
            {
                if (existingCoupon.Id == newCoupon.Id)
                    continue;
                if (existingCoupon.CouponType == newCoupon.CouponType &&
                    existingCoupon.DiscountAmount == newCoupon.DiscountAmount &&
                    existingCoupon.DiscountPercentage == newCoupon.DiscountPercentage &&
                    existingCoupon.RequiredPoints == newCoupon.RequiredPoints &&
                    existingCoupon.MinimumOrderAmount == newCoupon.MinimumOrderAmount &&
                    existingCoupon.MaximumDiscountAmount == newCoupon.MaximumDiscountAmount &&
                    existingCoupon.UserVisibility == newCoupon.UserVisibility)
                        throw new BadRequestException(Messages.Coupon_DuplicateCoupon);
            }
        }
        private UserCoupon CreateUserCoupon(string userId,int couponId)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var randomPart = new string(
                Enumerable.Repeat(chars, 5)
                    .Select(s => s[Random.Shared.Next(s.Length)])
                    .ToArray());

            var code = $"MK{DateTime.UtcNow:MM}-{randomPart}";
            var userCoupon = new UserCoupon
            {
                UserId = userId,
                CouponId = couponId,
                Code = code,
                AssignedAt = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                IsUsed = false
            };
            return userCoupon;
        }

        #endregion

    }
}
