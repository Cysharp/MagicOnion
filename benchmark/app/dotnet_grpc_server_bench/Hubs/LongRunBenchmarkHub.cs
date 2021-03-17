using System;
using Benchmark.Server.Shared;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Benchmark.Server.Hubs
{
    public partial class LongRunBenchmarkHub : StreamingHubBase<ILongRunBenchmarkHub, ILongRunBenchmarkHubReciever>, ILongRunBenchmarkHub
    {
        private readonly ILogger<ILongRunBenchmarkHub> _logger;

        public LongRunBenchmarkHub(ILogger<LongRunBenchmarkHub> logger)
        {
            _logger = logger;
        }

        public async Task Process(LongRunBenchmarkData data)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(data.WaitMilliseconds));
        }

        protected override ValueTask OnConnecting()
        {
            Statistics.HubConnected();
            _logger.LogTrace($"{Statistics.HubConnections} New Client coming. ({Context.ContextId})");
            return CompletedTask;
        }
        protected override ValueTask OnDisconnected()
        {
            Statistics.HubDisconnected();
            _logger.LogTrace($"Client disconnected.");
            return CompletedTask;
        }
    }
}
