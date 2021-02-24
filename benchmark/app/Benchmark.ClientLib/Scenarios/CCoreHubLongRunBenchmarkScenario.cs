using Benchmark.ClientLib.Reports;
using Benchmark.Server.Shared;
using Grpc.Core;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class CCoreHubLongRunBenchmarkScenario : ILongRunBenchmarkHubReciever, IAsyncDisposable
    {
        private readonly Channel _channel;
        private readonly BenchReporter _reporter;
        private ILongRunBenchmarkHub _client;
        private int _errors = 0;

        public CCoreHubLongRunBenchmarkScenario(Channel channel, BenchReporter reporter)
        {
            _channel = channel;
            _reporter = reporter;
            _errors = 0;
        }

        public async Task Run(int requestCount, int waitMilliseonds, bool parallel)
        {
            using (var total = new Statistics("Total"))
            {
                using (var statistics = new Statistics(nameof(ConnectAsync)))
                {
                    await ConnectAsync("console-client");

                    _reporter.AddBenchDetail(new BenchReportItem
                    {
                        ExecuteId = _reporter.ExecuteId,
                        ClientId = _reporter.ClientId,
                        TestName = nameof(ConnectAsync),
                        Begin = statistics.Begin,
                        End = DateTime.UtcNow,
                        Duration = statistics.Elapsed,
                        RequestCount = 0, // connect is setup, not count as request.
                        Errors = _errors,
                        Type = nameof(Grpc.Core.MethodType.DuplexStreaming),
                    });
                }

                using (var statistics = new Statistics(nameof(ProcessAsync)))
                {
                    await ProcessAsync(requestCount, waitMilliseonds, parallel);

                    _reporter.AddBenchDetail(new BenchReportItem
                    {
                        ExecuteId = _reporter.ExecuteId,
                        ClientId = _reporter.ClientId,
                        TestName = nameof(ProcessAsync),
                        Begin = statistics.Begin,
                        End = DateTime.UtcNow,
                        Duration = statistics.Elapsed,
                        RequestCount = requestCount,
                        Errors = _errors,
                        Type = nameof(Grpc.Core.MethodType.DuplexStreaming),
                    });
                }

                using (var statistics = new Statistics(nameof(EndAsync)))
                {
                    await EndAsync();
                    _reporter.AddBenchDetail(new BenchReportItem
                    {
                        ExecuteId = _reporter.ExecuteId,
                        ClientId = _reporter.ClientId,
                        TestName = nameof(EndAsync),
                        Begin = statistics.Begin,
                        End = DateTime.UtcNow,
                        Duration = statistics.Elapsed,
                        RequestCount = 0, // end is teardown, not count as request.
                        Errors = _errors,
                        Type = nameof(Grpc.Core.MethodType.DuplexStreaming),
                    });
                }
            }
        }

        private async Task ConnectAsync(string roomName)
        {
            _errors = 0;
            try
            {
                _client = await StreamingHubClient.ConnectAsync<ILongRunBenchmarkHub, ILongRunBenchmarkHubReciever>(new DefaultCallInvoker(_channel), this);
                var name = Guid.NewGuid().ToString();
                await _client.Ready(roomName, name);
            }
            catch (Exception)
            {
                _errors++;
            }
        }

        private async Task ProcessAsync(int requestCount, int waitMilliseonds, bool parallel)
        {
            if (parallel)
            {
                await ProcessParallelAsync(requestCount, waitMilliseonds);
            }
            else
            {
                await ProcessSequentialAsync(requestCount, waitMilliseonds);
            }
        }

        private async Task ProcessSequentialAsync(int requestCount, int waitMilliseonds)
        {
            _errors = 0;
            var data = new LongRunBenchmarkData
            {
                WaitMilliseconds = waitMilliseonds,
            };
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    await _client.Process(data);
                }
                catch (Exception)
                {
                    _errors++;
                }
            }
        }

        private async Task ProcessParallelAsync(int requestCount, int waitMilliseonds)
        {
            _errors = 0;
            var tasks = new List<Task>();
            var data = new LongRunBenchmarkData
            {
                WaitMilliseconds = waitMilliseonds,
            };
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    // no meaing.
                    // same streaming client will wait sequentially at server, you should not use itelation but must separate client.
                    var task = _client.Process(data);
                    tasks.Add(task);
                }
                catch (Exception)
                {
                    _errors++;
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task EndAsync()
        {
            _errors = 0;
            try
            {
                await _client.End();
            }
            catch (Exception)
            {
                _errors++;
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _client?.DisposeAsync();
        }

        void ILongRunBenchmarkHubReciever.OnStart(string requestType)
        {
            throw new NotImplementedException();
        }
        void ILongRunBenchmarkHubReciever.OnProcess()
        {
            throw new NotImplementedException();
        }
        void ILongRunBenchmarkHubReciever.OnEnd()
        {
            throw new NotImplementedException();
        }
    }
}
