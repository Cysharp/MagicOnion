using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using ZLogger;

namespace Benchmark.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // expand thread pool
            //ModifyThreadPool(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

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

        private static void ModifyThreadPool(int workerThread, int completionPortThread)
        {
            GetCurrentThread();
            SetThread(workerThread, completionPortThread);
            GetCurrentThread();
        }
        private static void GetCurrentThread()
        {
            ThreadPool.GetMinThreads(out var minWorkerThread, out var minCompletionPorlThread);
            ThreadPool.GetAvailableThreads(out var availWorkerThread, out var availCompletionPorlThread);
            ThreadPool.GetMaxThreads(out var maxWorkerThread, out var maxCompletionPorlThread);
            Console.WriteLine($"min: {minWorkerThread} {minCompletionPorlThread}");
            Console.WriteLine($"max: {maxWorkerThread} {maxCompletionPorlThread}");
            Console.WriteLine($"available: {availWorkerThread} {availCompletionPorlThread}");
        }

        private static void SetThread(int workerThread, int completionPortThread)
        {
            Console.WriteLine($"Changing ThreadPools. workerthread: {workerThread} completionPortThread: {completionPortThread}");
            ThreadPool.SetMinThreads(workerThread, completionPortThread);
        }

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

    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Configure Configuration with embedded appsettings.json
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureEmbeddedConfiguration(this IHostBuilder builder, string[] args)
            => ConfigureEmbeddedConfiguration(builder, args, typeof(Program).Assembly, AppContext.BaseDirectory);

        /// <summary>
        /// Configure Configuration with embedded appsettings.json
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="args"></param>
        /// <param name="assembly"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureEmbeddedConfiguration(this IHostBuilder builder, string[] args, Assembly assembly, string rootPath)
        {
            var embedded = new EmbeddedFileProvider(assembly);
            var physical = new PhysicalFileProvider(rootPath);

            // added Embedded Config selection for AddJsonFile, based on https://github.com/dotnet/runtime/blob/6a5a78bec9a6e14b4aa52cd5ac558f6cf5c6a211/src/libraries/Microsoft.Extensions.Hosting/src/Host.cs
            return builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                IHostEnvironment env = hostingContext.HostingEnvironment;
                bool reloadOnChange = hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);

                config.AddJsonFile(new CompositeFileProvider(physical, embedded), "appsettings.json", optional: true, reloadOnChange: reloadOnChange)
                      .AddJsonFile(new CompositeFileProvider(physical, embedded), $"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);

                if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null)
                    {
                        config.AddUserSecrets(appAssembly, optional: true, reloadOnChange: reloadOnChange);
                    }
                }

                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            });
        }
    }
}
