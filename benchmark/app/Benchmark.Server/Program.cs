using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using ZLogger;

namespace Benchmark.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // expand thread pool
            //ThreadPools.ModifyThreadPool(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

            //EnableDebugOutput();
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureEmbeddedConfiguration(args)
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddZLoggerConsole(configure => configure.EnableStructuredLogging = false);
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
                            options.ConfigureEndpointDefaults(endpointOptions =>
                            {
                                endpointOptions.Protocols = HttpProtocols.Http2;
                            });
                        })
                        .UseStartup<Startup>();
                });

        private static void EnableDebugOutput()
        {
            DebugPath();
            DebugEmbedded();
        }
        private static void DebugPath()
        {
            Console.WriteLine("Debugging path");
            var assemblyPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            using var processModule = Process.GetCurrentProcess().MainModule;
            var processPath = Path.GetDirectoryName(processModule?.FileName);
            var appContextPath = AppContext.BaseDirectory;

            Console.WriteLine($"{nameof(assemblyPath)}: {assemblyPath}");
            Console.WriteLine($"{nameof(processPath)}: {processPath}");
            Console.WriteLine($"{nameof(appContextPath)}: {appContextPath}");
        }

        private static void DebugEmbedded()
        {
            Console.WriteLine("Debugging embedded files");
            foreach (var resname in typeof(Program).Assembly.GetManifestResourceNames())
            {
                Console.WriteLine($"resname = {resname}");
                using (var stm = typeof(Program).Assembly.GetManifestResourceStream(resname))
                {
                    if (stm != null)
                    {
                        Console.WriteLine($"{resname} length is {stm.Length}");
                    }
                }
            }
        }
    }
}
