namespace MK.Core.Contracts.ServicesContracts
{
    public interface IFanRequestNotificationService
    {
        Task<int> SendPendingNotificationsAsync(CancellationToken cancellationToken = default);
    }
}
