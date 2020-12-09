using Benchmark.Server.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Client
{
    public class HubBenchmarkScenario : IBenchmarkHubReciever, IAsyncDisposable
    {
        private IBenchmarkHub _client;
        private GrpcChannel _channel;

        public HubBenchmarkScenario(GrpcChannel channel)
        {
            _channel = channel;
        }

        public async Task Run(int requestCount)
        {
            using (var statistics = new Statistics())
            {
                // todo: write my scenario
                await ConnectAsync("console-client");
                await PlainTextAsync(requestCount);
            }
        }

        private async Task ConnectAsync(string roomName)
        {
            _client = StreamingHubClient.Connect<IBenchmarkHub, IBenchmarkHubReciever>(_channel, this);
            var name = Guid.NewGuid().ToString();
            await _client.Ready(roomName, name, "plaintext");
        }

        private async Task PlainTextAsync(int requestCount)
        {
            var tasks = new List<Task>();
            for (var i = 0; i <= requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                var task = _client.Process(data);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
            await _client.End();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _client.DisposeAsync();
        }

        void IBenchmarkHubReciever.OnStart(string requestType)
        {
            throw new NotImplementedException();
        }
        void IBenchmarkHubReciever.OnProcess()
        {
            throw new NotImplementedException();
        }
        void IBenchmarkHubReciever.OnEnd()
        {
            throw new NotImplementedException();
        }
    }
}
