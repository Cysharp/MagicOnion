using Benchmark.ClientLib;
using DFrame;
using DFrame.Ecs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ZLogger;

namespace ConsoleAppEcs
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            var host = "0.0.0.0";
            var port = int.Parse(Environment.GetEnvironmentVariable("DFRAME_MASTER_CONNECT_TO_PORT") ?? "12345");
            var workerConnectToHost = Environment.GetEnvironmentVariable("DFRAME_MASTER_CONNECT_TO_HOST") ?? $"dframe-master.local";
            Console.WriteLine($"port {port}, workerConnectToHost {workerConnectToHost}");

            var reportId = Environment.GetEnvironmentVariable("BENCH_REPORTID") ?? throw new ArgumentNullException($"Environment variables BENCH_REPORTID is missing.");
            var path = Environment.GetEnvironmentVariable("BENCH_S3BUCKET") ?? throw new ArgumentNullException($"Environment variables BENCH_S3BUCKET is missing.");
            Console.WriteLine($"bucket {path}, reportId {reportId}");

            if (args.Length == 0)
            {
                // master
                // 10 100 200 <- BenchServer CPU 100% / Fargate Task CPU 100%
                // 20 10 1000 <- BenchServer CPU 100% / Fargate Task CPU 20%
                //args = "request -processCount 5 -workerPerProcess 50 -executePerWorker 100 -workerName UnaryWorker".Split(' ');
                //args = "request -processCount 10 -workerPerProcess 100 -executePerWorker 150 -workerName UnaryWorker".Split(' ');
                //args = "request -processCount 20 -workerPerProcess 10 -executePerWorker 1000 -workerName UnaryWorker".Split(' ');
                args = "request -processCount 1 -workerPerProcess 1 -executePerWorker 1000 -workerName UnaryWorker".Split(' ');
                //args = "request -processCount 1 -workerPerProcess 1 -executePerWorker 1000 -workerName GrpcWorker".Split(' ');
            }
            else if (args.Contains("--worker-flag"))
            {
                // worker
                // connect to
                host = workerConnectToHost;
            }

            Console.WriteLine($"args {string.Join(", ", args)}, host {host}");
            await Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddZLoggerConsole(options =>
                    {
                        options.EnableStructuredLogging = false;
                    });
                })
                .RunDFrameAsync(args, new DFrameOptions(host, port, workerConnectToHost, port, new EcsScalingProvider())
                {
                    Timeout = TimeSpan.FromMinutes(120),
                    OnExecuteResult = (results, option, scenario) =>
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
                        Console.WriteLine("Generating html.");
                        var benchmarker = new Benchmarker(path, null, null, cts.Token);
                        benchmarker.GenerateHtml(reportId, generateDetail: false).GetAwaiter().GetResult();
                    },
                });
                //.RunDFrameAsync(args, new DFrameOptions(host, port, workerConnectToHost, port, new InProcessScalingProvider())
                //{
                //    OnExecuteResult = async (results, option, scenario) => 
                //    {
                //        var benchmarker = new Benchmarker(path, null, default);
                //        var reports = await benchmarker.GetReports(reportId);
                //        if (reports.Any())
                //        {
                //            Console.WriteLine("Generating html.");
                //            await benchmarker.GenerateHtml(reportId, generateDetail: false);
                //        }
                //    },
                //});
        }
    }


    public class UnaryWorker : Worker
    {
        private CancellationTokenSource _cts;
        private string _hostAddress;
        private string _reportId;
        private Benchmarker _benchmarker;

        public override async Task SetupAsync(WorkerContext context)
        {
            Console.WriteLine("Setup");
            _cts = new CancellationTokenSource();
            _hostAddress = Environment.GetEnvironmentVariable("BENCH_SERVER_HOST") ?? throw new ArgumentNullException($"Environment variables BENCH_SERVER_HOST is missing.");
            _reportId = Environment.GetEnvironmentVariable("BENCH_REPORTID") ?? throw new ArgumentNullException($"Environment variables BENCH_REPORTID is missing.");
            var path = Environment.GetEnvironmentVariable("BENCH_S3BUCKET") ?? throw new ArgumentNullException($"Environment variables BENCH_S3BUCKET is missing.");
            //_hostAddress = "http://localhost:5000";
            //_reportId = "abc-123";
            //path = "sample-bucket";
            var iterations = new[] { 1, 2, 5, 10, 20, 50, 100, 200 };

            Console.WriteLine($"iterations {string.Join(",", iterations)}, hostAddress {_hostAddress}, reportId {_reportId}, path {path}");
            _benchmarker = new Benchmarker(path, iterations, null, _cts.Token);
        }
        public override async Task ExecuteAsync(WorkerContext context)
        {
            try
            {
                await _benchmarker.BenchUnary(_hostAddress, _reportId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception on ExecuteAsync. {ex.Message} {ex.StackTrace}");
                throw;
            }
        }
        public override async Task TeardownAsync(WorkerContext context)
        {
            Console.WriteLine("Teardown");
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public class HubWorker : Worker
    {
        private CancellationTokenSource _cts;
        private string _hostAddress;
        private string _reportId;
        private Benchmarker _benchmarker;

        public override async Task SetupAsync(WorkerContext context)
        {
            Console.WriteLine("Setup");
            _cts = new CancellationTokenSource();
            _hostAddress = Environment.GetEnvironmentVariable("BENCH_SERVER_HOST") ?? throw new ArgumentNullException($"Environment variables BENCH_SERVER_HOST is missing.");
            _reportId = Environment.GetEnvironmentVariable("BENCH_REPORTID") ?? throw new ArgumentNullException($"Environment variables BENCH_REPORTID is missing.");
            var path = Environment.GetEnvironmentVariable("BENCH_S3BUCKET") ?? throw new ArgumentNullException($"Environment variables BENCH_S3BUCKET is missing.");
            //_hostAddress = "http://localhost:5000";
            //_reportId = "abc-123";
            //path = "sample-bucket";
            var iterations = new[] { 1 };

            Console.WriteLine($"iterations {string.Join(",", iterations)}, hostAddress {_hostAddress}, reportId {_reportId}, path {path}");
            _benchmarker = new Benchmarker(path, iterations, null, _cts.Token);
        }
        public override async Task ExecuteAsync(WorkerContext context)
        {
            try
            {
                await _benchmarker.BenchHub(_hostAddress, _reportId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception on ExecuteAsync. {ex.Message} {ex.StackTrace}");
                throw;
            }
        }
        public override async Task TeardownAsync(WorkerContext context)
        {
            Console.WriteLine("Teardown");
            _cts.Cancel();
            _cts.Dispose();
        }
    }


    public class GrpcWorker : Worker
    {
        private CancellationTokenSource _cts;
        private string _hostAddress;
        private string _reportId;
        private Benchmarker _benchmarker;

        public override async Task SetupAsync(WorkerContext context)
        {
            Console.WriteLine("Setup");
            _cts = new CancellationTokenSource();
            _hostAddress = Environment.GetEnvironmentVariable("BENCH_SERVER_HOST") ?? throw new ArgumentNullException($"Environment variables BENCH_SERVER_HOST is missing.");
            _reportId = Environment.GetEnvironmentVariable("BENCH_REPORTID") ?? throw new ArgumentNullException($"Environment variables BENCH_REPORTID is missing.");
            var path = Environment.GetEnvironmentVariable("BENCH_S3BUCKET") ?? throw new ArgumentNullException($"Environment variables BENCH_S3BUCKET is missing.");
            //_hostAddress = "http://localhost:5000";
            //_reportId = "abc-123";
            //path = "sample-bucket";
            var iterations = new[] { 1, 2, 5, 10, 20, 50, 100, 200 };

            Console.WriteLine($"iterations {string.Join(",", iterations)}, hostAddress {_hostAddress}, reportId {_reportId}, path {path}");
            _benchmarker = new Benchmarker(path, iterations, null, _cts.Token);
        }
        public override async Task ExecuteAsync(WorkerContext context)
        {
            try
            {
                await _benchmarker.BenchGrpc(_hostAddress, _reportId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception on ExecuteAsync. {ex.Message} {ex.StackTrace}");
                throw;
            }
        }
        public override async Task TeardownAsync(WorkerContext context)
        {
            Console.WriteLine("Teardown");
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}