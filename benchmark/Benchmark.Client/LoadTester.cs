using Benchmark.Client.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Client
{
    public interface ILoadTester
    {
        Task<ClientInfo[]> ListClients();
        Task Run(int workerCount, int executeCount, string hostAddress, string reportId);
    }

    public enum ClientStatus
    {
        Running,
        Ready,
    }

    public class ClientInfo
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public ClientStatus Status { get; set; }
    }

    public static class LoadTesterFactory
    {
        private static ILoadTester loadTester;
        public static ILoadTester Create(ILogger logger, BenchmarkRunner runner)
        {
            if (loadTester != null)
                return loadTester;

            // todo: Google... etc...?
            if (AmazonUtils.IsAmazonEc2())
            {
                loadTester = new SsmLoadTester(logger, runner);
            }
            else
            {
                // fall back
                loadTester = new LocalLoadTester(logger, runner);
            }
            return loadTester;
        }
    }

    public class LocalLoadTester : ILoadTester
    {
        public ClientInfo CurrentInfo { get; private set; }

        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly BenchmarkRunner _runner;

        public LocalLoadTester(ILogger logger, BenchmarkRunner runner)
        {
            _logger = logger;
            _client = new HttpClient();
            _runner = runner;

            var name = Dns.GetHostName();
            CurrentInfo = new ClientInfo
            {
                Id = name,
                Name = name,
                Status = ClientStatus.Ready,
            };
        }

        public async Task<ClientInfo[]> ListClients()
        {
            return new[] { CurrentInfo };
        }

        public async Task Run(int workerCount, int executeCount, string hostAddress, string reportId)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < workerCount; i++)
            {
                var task = _runner.Bench(hostAddress, iteration: executeCount, reportId: reportId);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
    }

    public class SsmLoadTester : ILoadTester
    {
        private readonly ILogger _logger;
        private readonly BenchmarkRunner _runner;

        public SsmLoadTester(ILogger logger, BenchmarkRunner runner)
        {
            _logger = logger;
            _runner = runner;
        }
        public Task<ClientInfo[]> ListClients()
        {
            throw new NotImplementedException();
        }

        public Task Run(int workerCount, int executeCount, string hostAddress, string reportId)
        {
            throw new NotImplementedException();
        }
    }
}
