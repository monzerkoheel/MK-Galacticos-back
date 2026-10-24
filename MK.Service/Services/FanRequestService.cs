using AutoMapper;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.Account.Authentication;
using MK.Core.DTOs.Account.Management;
using MK.Core.DTOs.FanRequests;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using MK.Repository.Specifications;
using MK.Repository.Specifications.FanRequests_Specifications;

namespace MK.Service.Services
{
    public class FanRequestService : IFanRequestService
    {
        private const int LikesRequiredForComingSoon = 100;
        private const int ComingSoonRewardPoints = 100;
        private const int MaxRejectedReasonLength = 500;
        private const string ImagesFolder = "uploads/images/fanRequests";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFanRequestModerationQueue _moderationQueue;

        public FanRequestService(IUnitOfWork unitOfWork,
            IMapper mapper,
            IFileService fileService,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager,
            IFanRequestModerationQueue moderationQueue)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
            _currentUser = currentUserService;
            _userManager = userManager;
            _moderationQueue = moderationQueue;
        }

        #region Customer

        public async Task<OperationResultModel> CreateFanRequestAsync(CreateFanRequestDto dto)
        {
            var userId = GetCurrentUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var name = dto.Name;
            var description = dto.Description;

            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException(Messages.NameRequired);
            if (string.IsNullOrWhiteSpace(description))
                throw new BadRequestException(Messages.DescriptionRequired);

            if (dto.Image is null || dto.Image.Length == 0)
                throw new BadRequestException(Messages.ImageRequired);

            if (dto.Image.Length > 5 * 1024 * 1024)
                throw new BadRequestException(Messages.ImageTooLarge);

            var imageUrl = await _fileService.SaveImageAsync(dto.Image, ImagesFolder);

            FanRequest fanRequest;
            try
            {
                // The AI check can take a while, so the request is saved as "Processing" and the
                // user gets an immediate response. The moderation itself runs in the background
                // (FanRequestModerationWorker -> FanRequestModerationProcessor), which later moves
                // the request to Pending (approved by AI, or sent to manual review if the AI is
                // unavailable) or to RejectedByAi (+ auto-ban rule).
                fanRequest = new FanRequest
                {
                    UserId = userId,
                    Name = name,
                    Description = description,
                    ImageUrl = imageUrl,
                    Status = FanRequestStatus.Processing,
                    RejectedReason = null,
                    LikesCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<FanRequest>().AddAsync(fanRequest);
                if (await _unitOfWork.CompleteAsync() < 1)
                    throw new Exception(Messages.SomethingWentWrong);
            }
            catch
            {
                _fileService.DeleteImageIfExists(imageUrl);
                throw;
            }

            // Hand the request over to the background worker.
            // If this ever fails, the worker's periodic sweep picks up requests stuck in Processing.
            _moderationQueue.TryEnqueue(new FanRequestModerationJob(
                fanRequest.Id,
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName));

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.FanRequestCreatedSuccessfully
            };
        }

        public async Task<PaginationModel<FanRequestDto>> GetPublicFanRequestsAsync(PublicFanRequestsSpecPrams specPrams)
        {
            var repo = _unitOfWork.Repository<FanRequest>();

            var spec = new PublicFanRequestsSpecification(specPrams);
            var totalCount = await repo.CountAsync(spec);
            spec.ApplyPagination((specPrams.PageIndex - 1) * specPrams.PageSize, specPrams.PageSize);

            var items = await repo.ListAsync(spec);
            var data = _mapper.Map<IReadOnlyList<FanRequestDto>>(items);

            await FillIsLikedAsync(data);

            return new PaginationModel<FanRequestDto>(specPrams.PageIndex, specPrams.PageSize, totalCount, data);
        }

        public async Task<PaginationModel<MyFanRequestDto>> GetMyFanRequestsAsync(PaginationRequest dto)
        {
            var userId = GetCurrentUserId();
            var (pageIndex, pageSize) = NormalizePaging(dto.PageIndex, dto.PageSize);

            var repo = _unitOfWork.Repository<FanRequest>();

            var spec = new UserFanRequestsSpecification(userId);
            var totalCount = await repo.CountAsync(spec);
            spec.ApplyPagination((pageIndex - 1) * pageSize, pageSize);

            var items = await repo.ListAsync(spec);
            var data = _mapper.Map<IReadOnlyList<MyFanRequestDto>>(items);

            return new PaginationModel<MyFanRequestDto>(pageIndex, pageSize, totalCount, data);
        }

        public async Task<OperationResultModel> LikeFanRequestAsync(int fanRequestId)
        {
            var userId = GetCurrentUserId();

            var fanRequest = await _unitOfWork.Repository<FanRequest>().GetByIdAsync(fanRequestId);
            if (fanRequest is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(FanRequest)));

            if (!IsPubliclyVisible(fanRequest.Status))
                throw new BadRequestException(Messages.NotAvailableForLikes);

            var alreadyLiked = await _unitOfWork.Repository<FanRequestLike>().AnyAsync(
                new BaseSpecification<FanRequestLike>(x => x.FanRequestId == fanRequestId && x.UserId == userId));
            if (alreadyLiked)
                throw new BadRequestException(Messages.AlreadyLiked);

            await _unitOfWork.Repository<FanRequestLike>().AddAsync(new FanRequestLike
            {
                UserId = userId,
                FanRequestId = fanRequestId,
                CreatedAt = DateTime.UtcNow
            });

            fanRequest.LikesCount++;

            if (fanRequest.Status == FanRequestStatus.Approved && fanRequest.LikesCount >= LikesRequiredForComingSoon)
            {
                fanRequest.Status = FanRequestStatus.ComingSoon;
                fanRequest.UpdatedAt = DateTime.UtcNow;
                await AwardComingSoonPointsAsync(fanRequest);
            }

            _unitOfWork.Repository<FanRequest>().Update(fanRequest);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.LikedSuccessfully
            };
        }

        public async Task<OperationResultModel> UnlikeFanRequestAsync(int fanRequestId)
        {
            var userId = GetCurrentUserId();

            var fanRequest = await _unitOfWork.Repository<FanRequest>().GetByIdAsync(fanRequestId);
            if (fanRequest is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(FanRequest)));

            var like = await _unitOfWork.Repository<FanRequestLike>().GetFirstOrDefaultAsync(
                new BaseSpecification<FanRequestLike>(x => x.FanRequestId == fanRequestId && x.UserId == userId));
            if (like is null)
                throw new BadRequestException(Messages.NotLiked);

            _unitOfWork.Repository<FanRequestLike>().Delete(like);

            fanRequest.LikesCount = Math.Max(0, fanRequest.LikesCount - 1);
            _unitOfWork.Repository<FanRequest>().Update(fanRequest);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.UnlikedSuccessfully
            };
        }

        #endregion

        #region Management

        public async Task<PaginationModel<ManagementFanRequestDto>> GetAllFanRequestsAsync(FanRequestsSpecPrams specPrams)
        {
            var repo = _unitOfWork.Repository<FanRequest>();

            var spec = new ManagementFanRequestsSpecification(specPrams);
            var totalCount = await repo.CountAsync(spec);
            spec.ApplyPagination((specPrams.PageIndex - 1) * specPrams.PageSize, specPrams.PageSize);

            var items = await repo.ListAsync(spec);
            var data = _mapper.Map<IReadOnlyList<ManagementFanRequestDto>>(items);

            return new PaginationModel<ManagementFanRequestDto>(specPrams.PageIndex, specPrams.PageSize, totalCount, data);
        }

        public async Task<ManagementFanRequestDetailsDto> GetFanRequestByIdAsync(int fanRequestId)
        {
            var spec = new BaseSpecification<FanRequest>(x => x.Id == fanRequestId);
            spec.AddInclude(x => x.User);

            var fanRequest = await _unitOfWork.Repository<FanRequest>().GetFirstOrDefaultAsync(spec);
            if (fanRequest is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(FanRequest)));

            return _mapper.Map<ManagementFanRequestDetailsDto>(fanRequest);
        }

        public async Task<PaginationModel<AuditLogDto>> GetFanRequestAuditLogsAsync(int fanRequestId, PaginationRequest dto)
        {
            var fanRequest = await _unitOfWork.Repository<FanRequest>().GetByIdAsync(fanRequestId);
            if (fanRequest is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(FanRequest)));

            var (pageIndex, pageSize) = NormalizePaging(dto.PageIndex, dto.PageSize);
            var fanRequestIdStr = fanRequestId.ToString();

            var auditLogSpec = new AuditLogSpecification(x =>
                x.EntityName == nameof(FanRequest) &&
                x.EntityId == fanRequestIdStr &&
                (x.Action != AuditAction.Update || x.OldValues != null || x.NewValues != null));

            var totalCount = await _unitOfWork.Repository<AuditLog>().CountAsync(auditLogSpec);
            auditLogSpec.ApplyPagination((pageIndex - 1) * pageSize, pageSize);

            var auditLogs = await _unitOfWork.Repository<AuditLog>().ListAsync(auditLogSpec);
            var result = _mapper.Map<List<AuditLogDto>>(auditLogs);

            return new PaginationModel<AuditLogDto>(pageIndex, pageSize, totalCount, result);
        }

        public async Task<OperationResultModel> ChangeFanRequestStatusAsync(int fanRequestId, ChangeFanRequestStatusDto dto)
        {
            var repo = _unitOfWork.Repository<FanRequest>();

            var fanRequest = await repo.GetByIdAsync(fanRequestId);
            if (fanRequest is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(FanRequest)));

            var isAllowed = (fanRequest.Status, dto.NewStatus) switch
            {
                (FanRequestStatus.Pending, FanRequestStatus.Approved) => true,
                (FanRequestStatus.Pending, FanRequestStatus.Rejected) => true,
                (FanRequestStatus.ComingSoon, FanRequestStatus.AvailableOnSite) => true,
                _ => false
            };

            if (!isAllowed)
                throw new BadRequestException(string.Format(
                    Messages.InvalidStatusTransition, fanRequest.Status, dto.NewStatus));

            if (dto.NewStatus == FanRequestStatus.Rejected)
            {
                var reason = dto.RejectedReason?.Trim();
                if (string.IsNullOrWhiteSpace(reason))
                    throw new BadRequestException(Messages.RejectionReasonRequired);

                fanRequest.RejectedReason = reason;
            }

            fanRequest.Status = dto.NewStatus;
            fanRequest.UpdatedAt = DateTime.UtcNow;

            if (dto.NewStatus == FanRequestStatus.AvailableOnSite)
                await QueueAvailableOnSiteNotificationsAsync(fanRequest);

            repo.Update(fanRequest);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.StatusChangedSuccessfully, dto.NewStatus)
            };
        }

        public async Task<OperationResultModel> DeleteFanRequestAsync(int fanRequestId)
        {
            var repo = _unitOfWork.Repository<FanRequest>();

            var fanRequest = await repo.GetByIdAsync(fanRequestId);
            if (fanRequest is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(FanRequest)));

            var imageUrl = fanRequest.ImageUrl;

            var notifications = await _unitOfWork.Repository<AvailabilityNotification>().ListAsync(
                new BaseSpecification<AvailabilityNotification>(x => x.FanRequestId == fanRequestId));
            if (notifications.Count > 0)
                _unitOfWork.Repository<AvailabilityNotification>().DeleteRange(notifications);

            repo.Delete(fanRequest);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            if (!string.IsNullOrWhiteSpace(imageUrl))
                _fileService.DeleteImageIfExists(imageUrl);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.DeletedSuccessfully, "Fan Request")
            };
        }

        #endregion

        #region Helper Methods

        private string GetCurrentUserId()
            => _currentUser.UserId ?? throw new BadRequestException(Messages.AccountNotFound);

        private static bool IsPubliclyVisible(FanRequestStatus status)
            => status is FanRequestStatus.Approved
                      or FanRequestStatus.ComingSoon
                      or FanRequestStatus.AvailableOnSite;

        private static (int PageIndex, int PageSize) NormalizePaging(int pageIndex, int pageSize)
            => (Math.Max(1, pageIndex), Math.Clamp(pageSize, 1, 100));

        private async Task AwardComingSoonPointsAsync(FanRequest fanRequest)
        {
            var alreadyRewarded = await _unitOfWork.Repository<LoyaltyPointTransaction>().AnyAsync(
                new BaseSpecification<LoyaltyPointTransaction>(x =>
                    x.FanRequestId == fanRequest.Id &&
                    x.UserId == fanRequest.UserId &&
                    x.Reason == LoyaltyTransactionReason.FanRequest));
            if (alreadyRewarded)
                return;

            var owner = await _userManager.FindByIdAsync(fanRequest.UserId);
            if (owner is null)
                return;

            owner.LoyaltyPointsBalance += ComingSoonRewardPoints;
            owner.LifetimeEarnedLoyaltyPoints += ComingSoonRewardPoints;
            owner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<LoyaltyPointTransaction>().AddAsync(new LoyaltyPointTransaction
            {
                UserId = fanRequest.UserId,
                Points = ComingSoonRewardPoints,
                Type = LoyaltyPointTransactionType.Earn,
                Reason = LoyaltyTransactionReason.FanRequest,
                FanRequestId = fanRequest.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task QueueAvailableOnSiteNotificationsAsync(FanRequest fanRequest)
        {
            var likes = await _unitOfWork.Repository<FanRequestLike>().ListAsync(
                new BaseSpecification<FanRequestLike>(x => x.FanRequestId == fanRequest.Id));

            if (likes.Count == 0)
                return;

            var now = DateTime.UtcNow;
            var notifications = likes
                .Select(x => x.UserId)
                .Distinct()
                .Select(userId => new AvailabilityNotification
                {
                    UserId = userId,
                    Type = AvailabilityNotificationType.FanRequestPublished,
                    FanRequestId = fanRequest.Id,
                    Message = Messages.AvailableOnSiteNotification,
                    IsSent = false,
                    CreatedAt = now
                })
                .ToList();

            await _unitOfWork.Repository<AvailabilityNotification>().AddRangeAsync(notifications);
        }

        private async Task FillIsLikedAsync(IReadOnlyList<FanRequestDto> items)
        {
            var userId = _currentUser.UserId;
            if (userId is null || items.Count == 0)
                return;

            var ids = items.Select(x => x.Id).ToList();

            var likes = await _unitOfWork.Repository<FanRequestLike>().ListAsync(
                new BaseSpecification<FanRequestLike>(x => x.UserId == userId && ids.Contains(x.FanRequestId)));

            var likedIds = likes.Select(x => x.FanRequestId).ToHashSet();

            foreach (var item in items)
                item.IsLikedByCurrentUser = likedIds.Contains(item.Id);
        }

        #endregion
    }
}
