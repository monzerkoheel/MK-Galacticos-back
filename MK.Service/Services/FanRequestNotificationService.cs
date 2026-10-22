using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.Models;
using MK.Repository.Specifications;
using MK.Service.HelperServices.EmailSevrices.Templates;

namespace MK.Service.Services
{
    public class FanRequestNotificationService : IFanRequestNotificationService
    {
        private const int BatchSize = 100;
        private const string EmailSubject = "Your Fan Request is Now Available - MK Galácticos";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FanRequestNotificationService> _logger;

        public FanRequestNotificationService(IUnitOfWork unitOfWork,
            IEmailService emailService,
            IFileService fileService,
            IConfiguration configuration,
            ILogger<FanRequestNotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _fileService = fileService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<int> SendPendingNotificationsAsync(CancellationToken cancellationToken = default)
        {
            var repo = _unitOfWork.Repository<AvailabilityNotification>();
            var storeUrl = (_configuration["FrontendUrl"] ?? string.Empty).TrimEnd('/');

            var sentCount = 0;
            var lastId = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                // Id-based paging, so a notification that fails is skipped for this run
                // (it stays IsSent = false and is retried in the next daily run).
                var spec = new BaseSpecification<AvailabilityNotification>(x =>
                    x.Type == AvailabilityNotificationType.FanRequestPublished &&
                    !x.IsSent &&
                    x.Id > lastId);
                spec.AddInclude(x => x.User);
                spec.AddInclude(x => x.FanRequest!);
                spec.AddOrderBy(x => x.Id);
                spec.ApplyPagination(0, BatchSize);

                var batch = await repo.ListAsync(spec);
                if (batch.Count == 0)
                    break;

                foreach (var notification in batch)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lastId = notification.Id;

                    var user = notification.User;
                    var fanRequest = notification.FanRequest;

                    // Nobody to send to (deleted user / no email / request removed):
                    // close it so it is not picked again. SentAt stays null = "not really sent".
                    if (user is null || fanRequest is null ||
                        user.Status == UserStatus.Deleted ||
                        string.IsNullOrWhiteSpace(user.Email))
                    {
                        notification.IsSent = true;
                        repo.Update(notification);
                        continue;
                    }

                    try
                    {
                        var body = FanRequestAvailableTemplate.Generate(
                            user.FullName,
                            fanRequest.Name,
                            _fileService.GetFullUrl(fanRequest.ImageUrl ?? string.Empty),
                            storeUrl);

                        await _emailService.SendEmailAsync(user.Email, EmailSubject, body);

                        notification.IsSent = true;
                        notification.SentAt = DateTime.UtcNow;
                        repo.Update(notification);
                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send Fan Request availability email. NotificationId={NotificationId}",
                            notification.Id);
                    }
                }

                await _unitOfWork.CompleteAsync();
            }

            return sentCount;
        }
    }
}
