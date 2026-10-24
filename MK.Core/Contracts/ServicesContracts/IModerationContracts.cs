using MK.Core.HelperModels;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IAiModerationService
    {
        Task<AiModerationResult> ModerateFanRequestAsync(
            string name,
            string description,
            string imagePath,
            string? lang,
            CancellationToken cancellationToken = default);
    }
    public interface IFanRequestModerationQueue
    {
        bool TryEnqueue(FanRequestModerationJob job);
        IAsyncEnumerable<FanRequestModerationJob> ReadAllAsync(CancellationToken cancellationToken);
        void MarkDone(int fanRequestId);
    }
    public interface IFanRequestModerationProcessor
    {
        Task ProcessAsync(int fanRequestId, string? lang, CancellationToken cancellationToken);
    }
}
