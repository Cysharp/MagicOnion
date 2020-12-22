using Benchmark.Client.Reports;
using Benchmark.Server.Shared;
using Benchmark.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using System;
using System.Threading.Tasks;

namespace Benchmark.Client.Scenarios
{
    public class UnaryBenchmarkScenario
    {
        private readonly IBenchmarkService client;
        private readonly BenchReporter _reporter;
        private int _errors = 0;

        public UnaryBenchmarkScenario(GrpcChannel channel, BenchReporter reporter)
        {
            client = MagicOnionClient.Create<IBenchmarkService>(channel);
            _reporter = reporter;
            _errors = 0;
        }

        public async Task Run(int requestCount)
        {
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
                    Type = nameof(Grpc.Core.MethodType.Unary),
                });
            }
        }

        private async Task SumAsync(int requestCount)
        {
            for (var i = 0; i <= requestCount; i++)
            {
                try
                {
                    // Call the server-side method using the proxy.
                    _ = await client.SumAsync(i, i);
                }
                catch
                {
                    _errors++;
                }

            }
        }

        private async Task PlainTextAsync(int requestCount)
        {
            for (var i = 0; i <= requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                try
                {
                    _ = await client.PlainTextAsync(data);
                }
                catch (Exception)
                {
                    _errors++;
                }
            }
        }
    }
}
