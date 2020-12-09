using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZLogger;

namespace Benchmark.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(GetBasePath())
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

        private static string GetBasePath()
        {
            // non single file path
            var assemblyPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            
            // single file path
            if (assemblyPath is null)
            {
                using var processModule = Process.GetCurrentProcess().MainModule;
                return Path.GetDirectoryName(processModule?.FileName);
            }
            return assemblyPath;
        }
    }
}
