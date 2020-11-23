using ChatApp.Client;
using ChatApp.LoadTest.DI;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace ChatApp.LoadTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var clientAmount = int.Parse(args[0]);
                    var channel = GrpcChannel.ForAddress("https://localhost:5001");
                    services.AddSingleton(channel);
                    services.AddFactory<ChatHubClient>();
                    services.AddHostedService<Worker>(sp => new Worker(
                        sp.GetRequiredService<Func<ChatHubClient>>(),
                        sp.GetRequiredService<ILogger<Worker>>(),
                        clientAmount));
                });
    }
}
