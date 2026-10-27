using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.HelperModels;
using MK.Core.Models;
using MK.Repository.Specifications;

namespace MK.Service.BackgroundJobs
{
    /// <summary>
    /// Background worker:
    ///  1. Reads fan requests from the queue and moderates them one by one.
    ///  2. On startup, re-queues requests that were left in "Processing" (e.g. server restart).
    ///  3. Every few minutes, re-queues requests stuck in "Processing" for too long.
    /// </summary>
    public class FanRequestModerationWorker : BackgroundService
    {
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan StuckAfter = TimeSpan.FromMinutes(10);

        private readonly IFanRequestModerationQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FanRequestModerationWorker> _logger;
        private readonly CultureInfo _defaultCulture;

        public FanRequestModerationWorker(
            IFanRequestModerationQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<FanRequestModerationWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _defaultCulture = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            // Anything still "Processing" at startup was lost with the old process.
            await RequeueStuckAsync(TimeSpan.Zero);

            _ = Task.Run(() => SweepLoopAsync(stoppingToken), stoppingToken);

            try
            {
                await foreach (var job in _queue.ReadAllAsync(stoppingToken))
                {
                    await HandleJobAsync(job, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutting down
            }
        }

        private async Task HandleJobAsync(FanRequestModerationJob job, CancellationToken stoppingToken)
        {
            try
            {
                SetCulture(job.Lang);

                await using var scope = _scopeFactory.CreateAsyncScope();

                var processor = scope.ServiceProvider.GetRequiredService<IFanRequestModerationProcessor>();

                await processor.ProcessAsync(job.FanRequestId, job.Lang, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // The request stays "Processing" and the sweep will retry it later.
                _logger.LogError(ex, "Moderation failed for fan request {Id}.", job.FanRequestId);
            }
            finally
            {
                _queue.MarkDone(job.FanRequestId);
            }
        }

        private async Task SweepLoopAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var timer = new PeriodicTimer(SweepInterval);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RequeueStuckAsync(StuckAfter);
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
        }

        private async Task RequeueStuckAsync(TimeSpan olderThan)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var cutoff = DateTime.UtcNow - olderThan;

                var stuck = await unitOfWork.Repository<FanRequest>().ListAsync(
                    new BaseSpecification<FanRequest>(x =>
                        x.Status == FanRequestStatus.Processing && x.UpdatedAt <= cutoff));

                foreach (var fanRequest in stuck)
                    _queue.TryEnqueue(new FanRequestModerationJob(fanRequest.Id, null));

                if (stuck.Count > 0)
                    _logger.LogInformation("Re-queued {Count} fan request(s) stuck in Processing.", stuck.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to re-queue stuck fan requests.");
            }
        }

        private void SetCulture(string? lang)
        {
            var culture = _defaultCulture;

            if (!string.IsNullOrWhiteSpace(lang))
            {
                try
                {
                    culture = CultureInfo.GetCultureInfo(lang);
                }
                catch (CultureNotFoundException)
                {
                    // keep default
                }
            }

            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
        }
    }
}
