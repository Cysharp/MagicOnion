using Benchmark.Server.Shared;
using Benchmark.Shared;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Benchmark.Client
{
    public class UnaryBenchmarkScenario
    {
        private readonly IBenchmarkService client;

        public UnaryBenchmarkScenario(GrpcChannel channel)
        {
            client = MagicOnionClient.Create<IBenchmarkService>(channel);
        }

        public async Task Run(int requestCount)
        {
            using (var statistics = new Statistics())
            {
                // todo: write my scenario
                await PlainTextAsync(requestCount);
            }
        }

        private async Task SumAsync(int requestCount)
        {
            for (var i = 0; i <= requestCount; i++)
            {
                // Call the server-side method using the proxy.
                _ = await client.SumAsync(i, i);

                if (i % 1000 == 0)
                {
                    Console.WriteLine($"Completed {i} requests.");
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
                _ = await client.PlainTextAsync(data);

                if (i % 1000 == 0)
                {
                    Console.WriteLine($"Completed {i} requests.");
                }
            }
        }

        public async Task SumParallel(int requestCount)
        {
            var tasks = new List<UnaryResult<int>>();
            for (var i = 0; i <= requestCount; i++)
            {
                // Call the server-side method using the proxy.
                var task = client.SumAsync(i, i);
                tasks.Add(task);
            }
            await ValueTaskUtils.WhenAll(tasks.ToArray());
        }
    }
}
