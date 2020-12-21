using Benchmark.Client.Reports;
using Benchmark.Server.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Benchmark.Client.Scenarios
{
    public class HubBenchmarkScenario : IBenchmarkHubReciever, IAsyncDisposable
    {
        private IBenchmarkHub _client;
        private GrpcChannel _channel;
        private BenchReporter _reporter;

        public HubBenchmarkScenario(GrpcChannel channel, BenchReporter reporter)
        {
            _channel = channel;
            _reporter = reporter;
        }

        public async Task Run(int requestCount)
        {
            using (var statistics = new Statistics(nameof(ConnectAsync)))
            {
                await ConnectAsync("console-client");

                _reporter.AddBenchDetail(new BenchReportItem
                {
                    TestName = nameof(ConnectAsync),
                    Begin = statistics.Begin,
                    End = DateTime.UtcNow,
                    Duration = statistics.Elapsed,
                    RequestCount = 1,
                    Type = nameof(HubBenchmarkScenario),
                });
            }

            using (var statistics = new Statistics(nameof(PlainTextAsync)))
            {
                await PlainTextAsync(requestCount);

                _reporter.AddBenchDetail(new BenchReportItem
                {
                    TestName = nameof(PlainTextAsync),
                    Begin = statistics.Begin,
                    End = DateTime.UtcNow,
                    Duration = statistics.Elapsed,
                    RequestCount = requestCount,
                    Type = nameof(HubBenchmarkScenario),
                });
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
