using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MK.Core.Contracts.ServicesContracts;

namespace MK.Service.BackgroundServices
{
    public class FanRequestAvailabilityEmailWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FanRequestAvailabilityEmailWorker> _logger;

        public FanRequestAvailabilityEmailWorker(IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<FanRequestAvailabilityEmailWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextRun();
                _logger.LogInformation("Fan Request email job will run in {Delay}.", delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                   
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IFanRequestNotificationService>();

                    var sent = await service.SendPendingNotificationsAsync(stoppingToken);
                    _logger.LogInformation("Fan Request email job finished. Emails sent: {Count}.", sent);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fan Request email job failed.");
                }
            }
        }

        private TimeSpan GetDelayUntilNextRun()
        {
            var hour = _configuration.GetValue<int?>("FanRequestNotifications:SendHour") ?? 20;
            if (hour < 0 || hour > 23)
                hour = 20;

            var timeZone = ResolveTimeZone(_configuration["FanRequestNotifications:TimeZoneId"] ?? "Africa/Cairo");

            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var nextLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, hour, 0, 0, DateTimeKind.Unspecified);
            if (nextLocal <= nowLocal)
                nextLocal = nextLocal.AddDays(1);

            var nextUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocal, timeZone);
            var delay = nextUtc - DateTime.UtcNow;

            return delay > TimeSpan.Zero ? delay : TimeSpan.FromMinutes(1);
        }

        private static TimeZoneInfo ResolveTimeZone(string id)
        {
            foreach (var candidate in new[] { id, "Africa/Cairo", "Egypt Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(candidate);
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            return TimeZoneInfo.Utc;
        }
    }
}
