using Benchmark.ClientLib.Reports;
using Benchmark.Server.Shared;
using Benchmark.Shared;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class UnaryBenchmarkScenario
    {
        private readonly IBenchmarkService _client;
        private readonly BenchReporter _reporter;
        private int _errors = 0;

        public UnaryBenchmarkScenario(GrpcChannel channel, BenchReporter reporter)
        {
            _client = MagicOnionClient.Create<IBenchmarkService>(channel);
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
                    ExecuteId = _reporter.ExecuteId,
                    ClientId = _reporter.ClientId,
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
            var tasks = new List<UnaryResult<int>>();
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    // Call the server-side method using the proxy.
                    var task = _client.SumAsync(i, i);
                    tasks.Add(task);
                }
                catch
                {
                    _errors++;
                }
            }
            await ValueTaskUtils.WhenAll(tasks);
        }

        private async Task PlainTextAsync(int requestCount)
        {
            for (var i = 0; i < requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                try
                {
                    await _client.PlainTextAsync(data);
                }
                catch (Exception)
                {
                    _errors++;
                }
            }
        }

        private async Task PlainTextAsyncParallel(int requestCount)
        {
            var tasks = new List<UnaryResult<Nil>>();
            for (var i = 0; i < requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                try
                {
                    var task = _client.PlainTextAsync(data);
                    tasks.Add(task);
                }
                catch (Exception)
                {
                    _errors++;
                }
            }
            await ValueTaskUtils.WhenAll(tasks);
        }
    }
}
