using System.Collections.Concurrent;
using System.Threading.Channels;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.HelperModels;

namespace MK.Service.Services
{
    public class FanRequestModerationQueue : IFanRequestModerationQueue
    {
        private readonly Channel<FanRequestModerationJob> _channel =
            Channel.CreateUnbounded<FanRequestModerationJob>(new UnboundedChannelOptions
            {
                SingleReader = true
            });

        private readonly ConcurrentDictionary<int, byte> _inFlight = new();

        public bool TryEnqueue(FanRequestModerationJob job)
        {
            if (!_inFlight.TryAdd(job.FanRequestId, 0))
                return false;

            if (_channel.Writer.TryWrite(job))
                return true;

            _inFlight.TryRemove(job.FanRequestId, out _);
            return false;
        }

        public IAsyncEnumerable<FanRequestModerationJob> ReadAllAsync(CancellationToken cancellationToken)
            => _channel.Reader.ReadAllAsync(cancellationToken);

        public void MarkDone(int fanRequestId)
            => _inFlight.TryRemove(fanRequestId, out _);
    }
}
