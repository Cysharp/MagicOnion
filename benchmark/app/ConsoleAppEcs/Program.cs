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
            Console.WriteLine($"port: {port}, workerConnectToHost: {workerConnectToHost}");

            if (args.Length == 0)
            {
                // master
                args = "request -processCount 10 -workerPerProcess 10 -executePerWorker 1 -workerName UnaryWorker".Split(' ');
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
                .RunDFrameLoadTestingAsync(args, new DFrameOptions(host, port, workerConnectToHost, port, new EcsScalingProvider())
                {
                });
                //.RunDFrameLoadTestingAsync(args, new DFrameOptions(host, port, workerConnectToHost, port, new InProcessScalingProvider())
                // {
                // });
        }
    }


    public class UnaryWorker : Worker
    {
        private CancellationTokenSource _cts;
        private string _hostAddress;
        private string _iterations;
        private string _reportId;
        private Benchmarker _benchmarker;

        public override async Task SetupAsync(WorkerContext context)
        {
            Console.WriteLine("SetupAsync");
            _iterations = "256,1024";
            _cts = new CancellationTokenSource();
            _hostAddress = Environment.GetEnvironmentVariable("BENCH_SERVER_HOST") ?? throw new ArgumentNullException($"Environment variables BENCH_SERVER_HOST is missing.");
            _reportId = Environment.GetEnvironmentVariable("BENCH_REPORTID") ?? throw new ArgumentNullException($"Environment variables BENCH_REPORTID is missing.");
            var path = Environment.GetEnvironmentVariable("BENCH_S3BUCKET") ?? throw new ArgumentNullException($"Environment variables BENCH_S3BUCKET is missing.");
            Console.WriteLine($"iterations {_iterations}");
            Console.WriteLine($"hostAddress {_hostAddress}");
            Console.WriteLine($"reportId {_reportId}");
            Console.WriteLine($"path {path}");

            _benchmarker = new Benchmarker(path, null, _cts.Token);
        }
        public override async Task ExecuteAsync(WorkerContext context)
        {
            Console.WriteLine("Execute");
            try
            {
                await _benchmarker.BenchUnary(_hostAddress, _iterations, _reportId);
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
            var reports = await _benchmarker.GetReports(_reportId);
            if (reports.Any())
            {
                Console.WriteLine("Generating html.");
                await _benchmarker.GenerateHtml(_reportId);
            }
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}