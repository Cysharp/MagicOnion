using Benchmark.ClientLib.Reports;
using Benchmark.Server.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class ApiBenchmarkScenario : ScenarioBase
    {
        private readonly ApiClient _client;
        private readonly BenchReporter _reporter;

        public ApiBenchmarkScenario(ApiClient client, BenchReporter reporter, bool failFast) : base(failFast)
        {
            _client = client;
            _reporter = reporter;
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
                    Errors = Error,
                });

                statistics.HasError(Error);
            }
        }

        private async Task SumAsync(int requestCount)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    // Call the server-side method using the proxy.
                    var task = _client.SumAsync(i, i);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    if (FailFast)
                        throw;
                    IncrementError();
                    PostException(ex);
                }
            }
            await Task.WhenAll(tasks);
        }

        private async Task PlainTextAsync(int requestCount)
        {
            for (var i = 0; i < requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                var json = JsonSerializer.Serialize<BenchmarkData>(data);
                try
                {
                    await _client.PlainTextAsync(json);
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

        private async Task PlainTextAsyncParallel(int requestCount)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                var json = JsonSerializer.Serialize<BenchmarkData>(data);
                try
                {
                    var task = _client.PlainTextAsync(json);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    if (FailFast)
                        throw;
                    IncrementError();
                    PostException(ex);
                }
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// ThreadSafe client
        /// </summary>
        public class ApiClient
        {
            private readonly HttpClient _client;
            private readonly string _endpointPlainText;
            private readonly string _endpointSum;

            public ApiClient(string endpoint)
            {
                _client = new HttpClient();
                _endpointPlainText = endpoint + "/plaintext";
                _endpointSum = endpoint + "/sum";
            }

            public async Task PlainTextAsync(string json)
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync(_endpointPlainText, content);
                res.EnsureSuccessStatusCode();
            }

            public async Task SumAsync(int x, int y)
            {
                var res = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, _endpointSum + $"?x={x}&y={y}"));
                res.EnsureSuccessStatusCode();
            }
        }
    }
}
