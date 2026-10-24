using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Authentication;
using MK.Core.DTOs.Account.Management;
using MK.Core.HelperModels;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;

namespace MK.Service.Services
{
    /// <summary>
    /// Registered as Scoped (it uses the DbContext). Called by FanRequestModerationWorker.
    /// </summary>
    public class FanRequestModerationProcessor : IFanRequestModerationProcessor
    {
        private const int AiRejectionsBanThreshold = 3;     // 3 or more RejectedByAi => auto ban
        private const int MaxRejectedReasonLength = 500;
        private const int MaxAttempts = 3;

        private static readonly TimeSpan[] RetryDelays =
        {
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5)
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiModerationService _aiModerationService;
        private readonly IUserManagementService _userManagementService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FanRequestModerationProcessor> _logger;

        public FanRequestModerationProcessor(
            IUnitOfWork unitOfWork,
            IAiModerationService aiModerationService,
            IUserManagementService userManagementService,
            IWebHostEnvironment env,
            ILogger<FanRequestModerationProcessor> logger)
        {
            _unitOfWork = unitOfWork;
            _aiModerationService = aiModerationService;
            _userManagementService = userManagementService;
            _env = env;
            _logger = logger;
        }

        public async Task ProcessAsync(int fanRequestId, string? lang, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<FanRequest>();

            var fanRequest = await repo.GetByIdAsync(fanRequestId);

            // Deleted meanwhile, or already handled => nothing to do.
            if (fanRequest is null || fanRequest.Status != FanRequestStatus.Processing)
                return;

            AiModerationResult? result = null;

            var imagePath = GetPhysicalPath(fanRequest.ImageUrl);

            if (imagePath is not null && File.Exists(imagePath))
                result = await TryModerateAsync(fanRequest, imagePath, lang, cancellationToken);
            else
                _logger.LogWarning("Image file not found for fan request {Id}.", fanRequestId);

            if (result is null)
                _logger.LogWarning(
                    "AI moderation was NOT available for fan request {Id}. Sending it to manual review (Pending).",
                    fanRequestId);
            else
                _logger.LogInformation(
                    "Fan request {Id} was moderated by the AI. Approved={Approved}, Code={Code}.",
                    fanRequestId, result.IsApproved, result.ReasonCode);

            if (result is null)
            {
                // The AI could not answer => send it to the admin for manual review.
                fanRequest.Status = FanRequestStatus.Pending;
                fanRequest.RejectedReason = null;
            }
            else if (result.IsApproved)
            {
                fanRequest.Status = FanRequestStatus.Pending;
                fanRequest.RejectedReason = null;
            }
            else
            {
                var reason = string.IsNullOrWhiteSpace(result.RejectionReason)
                    ? Messages.AiRejectionFallbackReason
                    : result.RejectionReason.Trim();

                if (reason.Length > MaxRejectedReasonLength)
                    reason = reason[..MaxRejectedReasonLength];

                fanRequest.Status = FanRequestStatus.RejectedByAi;
                fanRequest.RejectedReason = reason;
            }

            fanRequest.UpdatedAt = DateTime.UtcNow;
            repo.Update(fanRequest);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            if (fanRequest.Status == FanRequestStatus.RejectedByAi)
                await ApplyAutoBanRuleAsync(fanRequest.UserId);
        }

        private async Task<AiModerationResult?> TryModerateAsync(
            FanRequest fanRequest,
            string imagePath,
            string? lang,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    return await _aiModerationService.ModerateFanRequestAsync(
                        fanRequest.Name,
                        fanRequest.Description,
                        imagePath,
                        lang,
                        cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw; // application is shutting down
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "AI moderation attempt {Attempt}/{Max} failed for fan request {Id}.",
                        attempt, MaxAttempts, fanRequest.Id);

                    if (attempt < MaxAttempts)
                        await Task.Delay(RetryDelays[attempt - 1], cancellationToken);
                }
            }

            return null;
        }

        private async Task ApplyAutoBanRuleAsync(string userId)
        {
            var aiRejectedCount = await _unitOfWork.Repository<FanRequest>().CountAsync(
                new BaseSpecification<FanRequest>(x => x.UserId == userId && x.Status == FanRequestStatus.RejectedByAi));

            if (aiRejectedCount < AiRejectionsBanThreshold)
                return;

            try
            {
                await _userManagementService.BanUserAsync(userId, new BanUserDto
                {
                    BannedUntil = null,
                    BanReason = string.Format(Messages.AutoBanReason, AiRejectionsBanThreshold)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not auto-ban user {UserId}.", userId);
            }
        }

        private string? GetPhysicalPath(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            return Path.Combine(
                _env.WebRootPath,
                imageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
    }
}
