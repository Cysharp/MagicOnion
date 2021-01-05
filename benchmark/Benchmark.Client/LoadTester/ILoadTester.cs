using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Benchmark.Client.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Client.LoadTester
{
    public interface ILoadTester
    {
        Task<ClientInfo[]> ListClients(CancellationToken ct = default);
        Task Run(int processCount, int executeCount, string hostAddress, string reportId, CancellationToken ct = default);
    }

    public class ClientInfo
    {
        /// <summary>
        /// Unique Id of client, expected i-xxxx like format. local machine will be machine name.
        /// </summary>
        public string Id { get; init; }
        /// <summary>
        /// Name of client, expected machine name.
        /// </summary>
        public string Name { get; init; }
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
                var config = new AmazonSimpleSystemsManagementConfig
                {
                    RegionEndpoint = Amazon.Util.EC2InstanceMetadata.Region,
                };
                loadTester = new SsmLoadTester(logger, runner, config);
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
            };
        }

        public async Task<ClientInfo[]> ListClients(CancellationToken ct = default)
        {
            return new[] { CurrentInfo };
        }

        public async Task Run(int processCount, int executeCount, string hostAddress, string reportId, CancellationToken ct = default)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < processCount; i++)
            {
                var task = _runner.BenchAll(hostAddress, reportId: reportId);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
    }

    public class SsmLoadTester : ILoadTester
    {
        private readonly ILogger _logger;
        private readonly BenchmarkRunner _runner;
        private readonly AmazonSimpleSystemsManagementClient _client;

        public SsmLoadTester(ILogger logger, BenchmarkRunner runner, AmazonSimpleSystemsManagementConfig config)
        {
            _logger = logger;
            _runner = runner;
            _client = new AmazonSimpleSystemsManagementClient(config);
        }

        public async Task<ClientInfo[]> ListClients(CancellationToken ct = default)
        {
            var instances = await _client.DescribeInstanceInformationAsync(new DescribeInstanceInformationRequest
            {
                Filters = new List<InstanceInformationStringFilter>
                {
                    new InstanceInformationStringFilter
                    {
                        Key = "tag-key",
                        Values = new List<string>{"bench"},
                    }
                },
            });
            // aws ssm describe-instance-information --output json --filters Key=tag-key,Values=bench --filters Key=PingStatus,Values=Online | jq - r ".InstanceInformationList[].InstanceId"
            var clients = instances.InstanceInformationList
                .Where(x => x.PingStatus == PingStatus.Online)
                .Select(x => new ClientInfo
                {
                    Id = x.InstanceId,
                    Name = x.ComputerName,
                })
                .ToArray();
            return clients;
        }

        public async Task Run(int processCount, int executeCount, string hostAddress, string reportId, CancellationToken ct = default)
        {
            var benchCommand = "benchall";
            var command = new List<string> {
                "#!/bin/bash",
                "export DOTNET_CLI_HOME=/tmp",
                $"BENCHCLIENT_RUNASWEB=false ~/client/Benchmark.Client benchmarkrunner {benchCommand} -hostAddress {hostAddress} -reportId {reportId}"
            };

            // get target clients
            var clients = await ListClients(ct);

            // execute
            var workers = new List<string>();
            for (var i = 0; i < processCount; i++)
            {
                workers.Add(clients[i % clients.Length].Id);
            }

            if (processCount <= clients.Length)
            {
                _logger.LogInformation($"Running on {string.Join(',', workers)}");
                await RunCore(command, workers, ct);
            }
            else
            {
                var tasks = workers.Select(async x =>
                {
                    _logger.LogInformation($"Running on {x}");
                    await RunCore(command, new List<string> { x }, ct);
                });
                await Task.WhenAll(tasks);
            }
        }

        private async Task RunCore(List<string> command, List<string> workers, CancellationToken ct)
        {
            // aws ssm send-command --document-name "AWS-RunShellScript" --targets "Key=InstanceIds,Values=${instanceId}" --cli-input-json file://benchmark/run_client_cli.json --output json
            var response = await _client.SendCommandAsync(new SendCommandRequest
            {
                DocumentName = "AWS-RunShellScript",
                InstanceIds = workers,
                Parameters = new Dictionary<string, List<string>>
                    {
                        {"commands", command},
                    },
            }, ct);

            // wait while processing
            bool running = true;
            while (running)
            {
                var commandresult = await _client.ListCommandInvocationsAsync(new ListCommandInvocationsRequest
                {
                    CommandId = response.Command.CommandId,
                }, ct);

                if (!commandresult.CommandInvocations.Any())
                {
                    // wait 10 sec. tekito-
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                    continue;
                }

                var fails = commandresult.CommandInvocations.Where(x => x.Status == CommandInvocationStatus.Failed).ToArray();
                if (fails.Any())
                {
                    foreach (var failed in fails)
                    {
                        _logger.LogInformation($"Command failed on {failed.InstanceId} for {failed.StatusDetails}");
                    }
                }

                running = commandresult.CommandInvocations.Any(x => x.Status == CommandInvocationStatus.InProgress || x.Status == CommandInvocationStatus.Pending);
                if (running)
                {
                    // wait 10 sec. tekito-
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                }
            }
        }
    }
}
