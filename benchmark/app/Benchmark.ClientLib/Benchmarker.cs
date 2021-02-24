using Benchmark.ClientLib.Converters;
using Benchmark.ClientLib.LoadTester;
using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Scenarios;
using Benchmark.ClientLib.Storage;
using Benchmark.ClientLib.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.ClientLib
{
    public class Benchmarker
    {
        private readonly string _path;
        private readonly int[] _iterations;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly string _clientId;
        private ConcurrentDictionary<string, GrpcChannel> _channelCache;

        public Benchmarker(string path, int[] iterations, ILogger logger, CancellationToken cancellationToken)
        {
            _path = path;
            _iterations = iterations;
            _logger = logger;
            _cancellationToken = cancellationToken;
            _clientId = Guid.NewGuid().ToString();
            _channelCache = new ConcurrentDictionary<string, GrpcChannel>();
        }

        private static string NewReportId() => DateTime.UtcNow.ToString("yyyyMMddHHmmss.fff") + "-" + Guid.NewGuid().ToString();
        
        private GrpcChannel GetOrCreateChannel(string hostAddress)
        {
            return _channelCache.GetOrAdd(hostAddress, GrpcChannel.ForAddress(hostAddress, new GrpcChannelOptions
            {
                // default HTTP/2 MutipleConnections = 100, true enable additional HTTP/2 connection via channel.
                // memo: create Channel Pool and random get pool for each connection to avoid too match channel connection.
                HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                }
            }));
        }

        /// <summary>
        /// Run Unary and Hub Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchAll(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();
            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId);
            reporter.Begin();
            {
                // Connect to the server using gRPC channel.
                var channel = GetOrCreateChannel(hostAddress);
                var unary = new UnaryBenchmarkScenario(channel, reporter);
                await using var hub = new HubBenchmarkScenario(channel, reporter);

                foreach (var iteration in _iterations)
                {
                    // Unary
                    _logger?.LogInformation($"Begin unary {iteration} requests.");
                    await unary.Run(iteration);

                    // StreamingHub
                    _logger?.LogInformation($"Begin Streaming {iteration} requests.");
                    await hub.Run(iteration);
                }
            }
            reporter.End();

            // output
            var benchJson = reporter.ToJson();

            // put json to s3
            var storage = StorageFactory.Create(_logger);
            await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: _cancellationToken);
        }

        /// <summary>
        /// Run Unary Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchUnary(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId);
            reporter.Begin();
            {
                // Connect to the server using gRPC channel.
                var channel = GetOrCreateChannel(hostAddress);
                var scenario = new UnaryBenchmarkScenario(channel, reporter);

                foreach (var iteration in _iterations)
                {
                    // Unary
                    _logger?.LogInformation($"Begin unary {iteration} requests.");
                    await scenario.Run(iteration);
                }
            }
            reporter.End();

            // output
            var benchJson = reporter.ToJson();

            // put json to s3
            var storage = StorageFactory.Create(_logger);
            await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: _cancellationToken);
        }

        /// <summary>
        /// Run Hub Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchHub(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId);
            reporter.Begin();
            {
                // Connect to the server using gRPC channel.
                var channel = GetOrCreateChannel(hostAddress);
                await using var scenario = new HubBenchmarkScenario(channel, reporter);

                foreach (var iteration in _iterations)
                {
                    // StreamingHub
                    _logger?.LogInformation($"Begin Streaming {iteration} requests.");
                    await scenario.Run(iteration);
                }
            }
            reporter.End();

            // output
            var benchJson = reporter.ToJson();

            // put json to s3
            var storage = StorageFactory.Create(_logger);
            await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: _cancellationToken);
        }


        /// <summary>
        /// Run Grpc Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchGrpc(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.GrpcDotnet);
            reporter.Begin();
            {
                // Connect to the server using gRPC channel.
                var channel = GetOrCreateChannel(hostAddress);
                var scenario = new GrpcBenchmarkScenario(channel, reporter);

                foreach (var iteration in _iterations)
                {
                    // Unary
                    _logger?.LogInformation($"Begin grpc {iteration} requests.");
                    await scenario.Run(iteration);
                }
            }
            reporter.End();

            // output
            var benchJson = reporter.ToJson();

            // put json to s3
            var storage = StorageFactory.Create(_logger);
            await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: _cancellationToken);
        }

        /// <summary>
        /// List Reports
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task<string[]> ListReports(string reportId)
        {
            // access s3 and List json from reportId
            var storage = StorageFactory.Create(_logger);
            var reports = await storage.List(_path, $"reports/{reportId}", _cancellationToken);
            _logger.LogInformation($"Total {reports.Length} reports");
            foreach (var report in reports)
            {
                _logger?.LogInformation(report);
            }
            return reports;
        }

        /// <summary>
        /// Get Report
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task<BenchReport[]> GetReports(string reportId)
        {
            // access s3 and get jsons from reportId
            var storage = StorageFactory.Create(_logger);
            var reportJsons = await storage.Get(_path, $"reports/{reportId}", _cancellationToken);
            var reports = new List<BenchReport>();
            foreach (var json in reportJsons)
            {
                var report = JsonConvert.Deserialize<BenchReport>(json);
                reports.Add(report);
            }
            return reports.ToArray();
        }

        /// <summary>
        /// Generate Report Html
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="generateDetail"></param>
        /// <param name="htmlFileName"></param>
        /// <returns></returns>
        public async Task GenerateHtml(string reportId, bool generateDetail = true, string htmlFileName = "index.html")
        {
            // access s3 and download json from reportId
            var reports = await GetReports(reportId);
            if (!reports.Any())
                return;

            // generate html based on json data
            var htmlReporter = new HtmlBenchReporter();
            var htmlReport = htmlReporter.CreateReport(reports, generateDetail);
            var page = new BenchmarkReportPageTemplate()
            {
                Report = htmlReport,
            };
            var content = NormalizeNewLineLf(page.TransformText());

            // upload html report to s3
            var storage = StorageFactory.Create(_logger);
            var outputUri = await storage.Save(_path, $"html/{reportId}", htmlFileName, content, overwrite: true, _cancellationToken);

            _logger?.LogInformation($"HtmlReport Uri: {outputUri}");
        }

        public async Task<ClientInfo[]> ListClients()
        {
            // call ssm to list up client instanceids
            var loadTester = LoadTesterFactory.Create(_logger, this);
            var clients = await loadTester.ListClients();
            var json = JsonConvert.Serialize(clients);
            _logger?.LogInformation(json);
            return clients;
        }

        public async Task RunAllClient(int processCount, string iterations = "256,1024,4096,16384", string benchCommand = "benchall", string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            _logger?.LogInformation($"reportId: {reportId}");

            // call ssm to execute Clients via CLI mode.
            var loadTester = LoadTesterFactory.Create(_logger, this);
            try
            {
                await loadTester.Run(processCount, iterations, benchCommand, hostAddress, reportId, _cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Run failed.");
            }

            // Generate Html Report
            await GenerateHtml(reportId);
        }

        public async Task CancelCommands()
        {
            var config = AmazonUtils.IsAmazonEc2()
                ? new Amazon.SimpleSystemsManagement.AmazonSimpleSystemsManagementConfig
                {
                    RegionEndpoint = Amazon.Util.EC2InstanceMetadata.Region,
                }
                : new Amazon.SimpleSystemsManagement.AmazonSimpleSystemsManagementConfig
                {
                    RegionEndpoint = Amazon.RegionEndpoint.APNortheast1,
                };
            var client = new Amazon.SimpleSystemsManagement.AmazonSimpleSystemsManagementClient(config);
            var commands = await client.ListCommandInvocationsAsync(new Amazon.SimpleSystemsManagement.Model.ListCommandInvocationsRequest
            {
                Filters = new List<Amazon.SimpleSystemsManagement.Model.CommandFilter>
                {
                    new Amazon.SimpleSystemsManagement.Model.CommandFilter
                    {
                        Key = "Status",
                        Value = "InProgress",
                    }
                },
            }, _cancellationToken);

            foreach (var command in commands.CommandInvocations)
            {
                _logger?.LogInformation($"Cancelling {command.CommandId}");
                await client.CancelCommandAsync(new Amazon.SimpleSystemsManagement.Model.CancelCommandRequest
                {
                    CommandId = command.CommandId,
                }, _cancellationToken);
            }
        }

        private static string NormalizeNewLine(string content)
        {
            return content
                .Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("\n", Environment.NewLine, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeNewLineLf(string content)
        {
            return content
                .Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase);
        }
    }
}
