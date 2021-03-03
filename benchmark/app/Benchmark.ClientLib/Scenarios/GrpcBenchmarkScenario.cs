using Benchmark.ClientLib.Reports;
using Benchmark.Server;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class GrpcBenchmarkScenario : ScenarioBase
    {
        private readonly Greeter.GreeterClient _client;
        private readonly HelloRequest _simpleRequest;
        private readonly BenchReporter _reporter;

        public GrpcBenchmarkScenario(GrpcChannel channel, BenchReporter reporter, bool failFast) : base(failFast)
        {
            _client = new Greeter.GreeterClient(channel);
            _reporter = reporter;

            _simpleRequest = new HelloRequest { Value = true };
        }

        public async Task Run(int requestCount)
        {
            using (var statistics = new Statistics(nameof(SayHelloAsync)))
            {
                await SayHelloAsync(requestCount);

                _reporter.AddBenchDetail(new BenchReportItem
                {
                    ExecuteId = _reporter.ExecuteId,
                    ClientId = _reporter.ClientId,
                    TestName = nameof(SayHelloAsync),
                    Begin = statistics.Begin,
                    End = DateTime.UtcNow,
                    Duration = statistics.Elapsed,
                    RequestCount = requestCount,
                    Type = nameof(Grpc.Core.MethodType.Unary),
                    Errors = Error,
                });
                statistics.HasError(Error);
            }
        }

        private async Task SayHelloAsync(int requestCount)
        {
            for (var i = 0; i <= requestCount; i++)
            {
                try
                {
                    await _client.SayHelloAsync(_simpleRequest);
                }
                catch (Exception ex)
                {
                    if (FailFast)
                        throw;
                    IncrementError();
                    PostException(ex);
                }
            }
        }

    }
}
