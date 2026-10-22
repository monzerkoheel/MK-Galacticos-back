using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Information;
using MK.Core.DTOs.Orders;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using MK.Repository.Data;
using MK.Repository.Specifications;
using MK.Repository.Specifications.Products_Specifications;

namespace MK.Service.Services
{
    public class UserInformationService : IUserInformationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MKGalacticosDbContext _dbContext;
        private readonly IFileService _fileService;
        public UserInformationService(
            IUnitOfWork unitOfWork,
            IMapper mapper, 
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager,
            MKGalacticosDbContext dbContext,
            IFileService fileService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUserService;
            _userManager = userManager;
            _dbContext = dbContext;
            _fileService = fileService;
        }

        public async Task<IReadOnlyList<CountryDto>> GetAvailableCountriesAsync()
        {
            var countries = await _unitOfWork.Repository<Country>().GetAllAsync();
            return _mapper.Map<IReadOnlyList<CountryDto>>(countries);
        }

        public async Task<IReadOnlyList<GovernorateDto>> GetCountryGovernoratesAsync(int countryId)
        {
            var governorates = await _unitOfWork.Repository<Governorate >()
                .ListAsync(new BaseSpecification<Governorate>(g => g.CountryId == countryId));
            return _mapper.Map<IReadOnlyList<GovernorateDto>>(governorates);
        }

        public async Task<OperationResultModel> CreateUserAddress(CreateUserAddressDto dto)
        {
            var userAddress = _mapper.Map<UserAddress>(dto);
            userAddress.UserId = _currentUser.UserId!;
            await  _unitOfWork.Repository<UserAddress>().AddAsync(userAddress);
            try 
            { 
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                throw new BadRequestException(Messages.AddressNameAlreadyExists);
            }

            
            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.AddressSavedSuccessfully
            };
        }

        public async Task<IReadOnlyList<UserAddressDto>?> GetUserAddressesAsync()
        {
            var userAddresses = await _unitOfWork.Repository<UserAddress>()
                .ListAsync(new BaseSpecification<UserAddress>(ua => ua.UserId == _currentUser.UserId));
            return _mapper.Map<IReadOnlyList<UserAddressDto>>(userAddresses);
        }

        public async Task<UserAddressDto> GetUserAddressAsync(int addressId)
        {
            var userAddress = await _unitOfWork.Repository<UserAddress>().GetByIdAsync(addressId);
            if(userAddress == null)
                throw new NotFoundException(Messages.AddressNotFound);
            if (userAddress.UserId != _currentUser.UserId)
                throw new ForbiddenException(Messages.InsufficientPermissions);
            return _mapper.Map<UserAddressDto>(userAddress);
        }

        public async Task<UserAddressDto> EditUserAddressAsync(EditUserAddressDto dto)
        {
            var _addressRepo = _unitOfWork.Repository<UserAddress>();
            var userAddress = await _addressRepo.GetByIdAsync(dto.Id);

            if (userAddress is null)
                throw new BadRequestException(Messages.AddressNotFound);

            if (userAddress.UserId != _currentUser.UserId)
                throw new ForbiddenException(Messages.InsufficientPermissions);

            _mapper.Map(dto, userAddress);
            _addressRepo.Update(userAddress);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                throw new BadRequestException(Messages.AddressNameAlreadyExists);
            }

            return _mapper.Map<UserAddressDto>(userAddress);
        }

        public async Task<OperationResultModel> DeleteUserAddressAsync(int addressId)
        {
            var _addressRepo = _unitOfWork.Repository<UserAddress>();

            var userAddress = await _addressRepo.GetByIdAsync(addressId);
            if (userAddress is null)
                throw new BadRequestException(Messages.AddressNotFound);

            if (userAddress.UserId != _currentUser.UserId)
                throw new ForbiddenException(Messages.InsufficientPermissions);

            _addressRepo.Delete(userAddress);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.AddressDeletedSuccessfully
            };
        }

        public async Task<UserProfileDto> EditUserInfoAsync(EditUserInformationsDto dto)
        {
            var user = await GetAllUserDataAsync();

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            if (dto.FullName is not null)
                user.FullName = dto.FullName;

            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            var userProfileDto = _mapper.Map<UserProfileDto>(user);
            return userProfileDto;
        }

        public async Task<UserProfileDto> UploadUserImageAsync(UploadUserImageDto dto)
        {
            var user = await GetAllUserDataAsync();
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var oldImageUrl = user.ProfileImageUrl;

            var newImageUrl = await _fileService.SaveImageAsync(dto.UserImage,"uploads/images/users");

            user.ProfileImageUrl = newImageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _fileService.DeleteImageIfExists(newImageUrl);
                throw new BadRequestException(Messages.SomethingWentWrong);
            }

            if (!string.IsNullOrWhiteSpace(oldImageUrl))
                _fileService.DeleteImageIfExists(oldImageUrl);

            var userProfileDto = _mapper.Map<UserProfileDto>(user);
            return userProfileDto;
        }

        public async Task<UserProfileDto> DeleteUserImageAsync()
        {
            var user = await GetAllUserDataAsync();
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            if (user.ProfileImageUrl == null)
                throw new BadRequestException(Messages.ImageAlreadyDeleted);

            _fileService.DeleteImageIfExists(user.ProfileImageUrl);

            user.ProfileImageUrl = null;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            var userProfileDto = _mapper.Map<UserProfileDto>(user);
            return userProfileDto;
        }

        public async Task<UserProfileDto> GetProfileAsync()
        {
            var user = await GetAllUserDataAsync();
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);
            var userProfileDto = _mapper.Map<UserProfileDto>(user);

            userProfileDto.UserLoyaltyData = await GetUserLoyaltyDataAsync(user);

            return userProfileDto;
        }

        public async Task<PaginationModel<LoyaltyTransactionDto>> GetAllUserTransaction(LoyaltyTransactionSpecPrams specPrams)
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);
             
            var transactionSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.UserId == user.Id && 
                                                                                (!specPrams.Type.HasValue || x.Type == specPrams.Type.Value));
            
            var totalTransactions = await _unitOfWork.Repository<LoyaltyPointTransaction>().CountAsync(transactionSpec);
            
            transactionSpec.ApplyPagination((specPrams.PageIndex - 1) * specPrams.PageSize, specPrams.PageSize);
            transactionSpec.AddOrderByDes(x => x.CreatedAt);
            var transactions = await _unitOfWork.Repository<LoyaltyPointTransaction>().ListAsync(transactionSpec);
            var data = transactions.Select(x => new LoyaltyTransactionDto
                        {
                            Points = x.Points,
                            Type = x.Type,
                            Description = BuildLoyaltyTransactionDescription(x.Reason),
                            CreatedAt = x.CreatedAt
                        }).ToList();
            return new PaginationModel<LoyaltyTransactionDto>
                ( 
                specPrams.PageIndex,
                specPrams.PageSize,
                totalTransactions,
                data
                );
        }

        public async Task<SimpleUserDataDto> GetSimpleUserData()
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId ?? "15");
            if (user == null)
                throw new NotFoundException(Messages.AccountNotFound);
            string? fullImagePath = null;
            if (user.ProfileImageUrl != null)
                fullImagePath = _fileService.GetFullUrl(user.ProfileImageUrl);
            return new SimpleUserDataDto 
            { 
                UserName = user.FullName,
                UserRole = _currentUser.Role ?? "Customer",
                UserImage = fullImagePath
            };
        }
      
        public async Task<PaginationModel<UserCouponDto>> GetUserCouponsAsync(UserCouponsSpecPrams specParams)
        {
            var userCouponRepo = _unitOfWork.Repository<UserCoupon>();

            var countSpec = new UserCouponSpecification(_currentUser.UserId, specParams, false);
            var totalCoupons = await userCouponRepo.CountAsync(countSpec);

            var userCouponSpec = new UserCouponSpecification(_currentUser.UserId, specParams);
            var userCoupons = await userCouponRepo.ListAsync(userCouponSpec);

            var data = _mapper.Map<IReadOnlyList<UserCouponDto>>(userCoupons);
            return new PaginationModel<UserCouponDto>(
                specParams.PageIndex,
                specParams.PageSize,
                totalCoupons,
                data);
        }

        public async Task<IReadOnlyList<ActiveProductsDto>> GetUserWishList()
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if (user == null)
                throw new NotFoundException(Messages.AccountNotFound);
            var wishListSpec = new BaseSpecification<Wishlist>(x => x.UserId == user.Id);
            wishListSpec.AddInclude(x => x.Items);
            var wishList = await _unitOfWork.Repository<Wishlist>().GetFirstOrDefaultAsync(wishListSpec);
            if (wishList == null)
            {
                var newWishList = new Wishlist
                {
                    UserId = user.Id
                };
                await _unitOfWork.Repository<Wishlist>().AddAsync(newWishList);
                await _unitOfWork.CompleteAsync();
                return new List<ActiveProductsDto>();
            }

            var productIds = wishList.Items.Select(x => x.ProductId).ToList();

            var spec = new ProductWithAllDataSpecification(x => productIds.Contains(x.Id) && x.Visibility == Visibility.Active);
            var products = await _unitOfWork.Repository<Product>().ListAsync(spec);
            var data = _mapper.Map<IReadOnlyList<ActiveProductsDto>>(products);
            return data;
        }

        public async Task<IReadOnlyList<CartItemDto>> GetUserCart()
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if (user == null)
                throw new NotFoundException(Messages.AccountNotFound);
            var cartSpec = new BaseSpecification<Cart>(x => x.UserId == user.Id);
            cartSpec.AddInclude(x => x.Items);
            cartSpec.AddInclude("Items.Product");
            cartSpec.AddInclude("Items.Product.Translations");
            cartSpec.AddInclude("Items.Product.Images");
            var cart = await _unitOfWork.Repository<Cart>().GetFirstOrDefaultAsync(cartSpec);
            if (cart == null)
            {
                var newCart = new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.Repository<Cart>().AddAsync(newCart);
                await _unitOfWork.CompleteAsync();
                return new List<CartItemDto>();
            }

            var data = _mapper.Map<IReadOnlyList<CartItemDto>>(cart.Items);
            return data;
        }

        public async Task<PaginationModel<OrderDto>> GetUserOrdersAsync(PaginationRequest dto)
        {
            var ordersSpec = new BaseSpecification<Order>(x => x.UserId == _currentUser.UserId);
            var totalCount = await _unitOfWork.Repository<Order>().CountAsync(ordersSpec);

            ordersSpec.AddOrderByDes(x => x.CreatedAt);
            ordersSpec.ApplyPagination((dto.PageIndex - 1) * dto.PageSize, dto.PageSize);
            ordersSpec.AddInclude(x => x.Items);

            var orders = await _unitOfWork.Repository<Order>().ListAsync(ordersSpec);
            var data = _mapper.Map<List<OrderDto>>(orders);

            return new PaginationModel<OrderDto>(dto.PageIndex, dto.PageSize, totalCount, data);
        }


        #region Helping functions
        private async Task<ApplicationUser?> GetAllUserDataAsync()
        {
            var user = await _userManager.Users
                     .Include(x => x.Addresses)
                     .Include(x => x.Orders)
                        .ThenInclude(x => x.Items)
                     .Include(x => x.UserCoupons)
                        .ThenInclude(x => x.Coupon)
                            .ThenInclude(x => x.Translations)
                     .Include(x => x.LoyaltyPointTransactions)
                     .Include(x => x.FanRequests)
                     .Include(x => x.Wishlist)
                     .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId);
            return user;
        }
        private async Task<LoyaltyDto> GetUserLoyaltyDataAsync(ApplicationUser user)
        {
            var lifetimePoints = user.LifetimeEarnedLoyaltyPoints;

            var currentTier = lifetimePoints switch
            {
                >= 5000 => "MK Legend",
                >= 2000 => "MK Elite",
                _ => "MK Fan"
            };

            var nextTier = lifetimePoints switch
            {
                < 2000 => "MK Elite",
                < 5000 => "MK Legend",
                _ => null
            };

            var nextTierRequiredPoints = lifetimePoints switch
            {
                < 2000 => 2000,
                < 5000 => 5000,
                _ => 0
            };

            var pointsToNextTier = nextTierRequiredPoints > 0
                ? nextTierRequiredPoints - lifetimePoints
                : 0;

            var recentTransactions = user.LoyaltyPointTransactions
                 .OrderByDescending(x => x.CreatedAt)
                 .Take(5)
                 .Select(x => new LoyaltyTransactionDto
                 {
                     Points = x.Points,
                     Type = x.Type,
                     Description = BuildLoyaltyTransactionDescription(x.Reason),
                     CreatedAt = x.CreatedAt
                 })
                 .ToList();

            return new LoyaltyDto
            {
                Balance = user.LoyaltyPointsBalance,
                LifetimeEarnedPoints = lifetimePoints,
                CurrentTier = currentTier,
                NextTier = nextTier,
                PointsToNextTier = pointsToNextTier,
                RecentTransactions = recentTransactions
            };
        }
        private string BuildLoyaltyTransactionDescription(LoyaltyTransactionReason reason)
        {
            return reason switch
            {
                LoyaltyTransactionReason.Purchase
                    => Enums.LoyaltyTransactionReason_Purchase,

                LoyaltyTransactionReason.FanRequest
                    => Enums.LoyaltyTransactionReason_FanRequest,

                LoyaltyTransactionReason.ProductReview
                    => Enums.LoyaltyTransactionReason_ProductReview,

                LoyaltyTransactionReason.DailyLogin
                    => Enums.LoyaltyTransactionReason_DailyLogin,

                LoyaltyTransactionReason.Referral
                    => Enums.LoyaltyTransactionReason_Referral,

                LoyaltyTransactionReason.RewardRedemption
                    => Enums.LoyaltyTransactionReason_RewardRedemption,

                LoyaltyTransactionReason.OrderNotCompleted
                    => Enums.LoyaltyTransactionReason_OrderNotCompleted,

                _ => string.Empty
            };
        }

        #endregion
    }
}
