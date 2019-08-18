using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Hosting;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sandbox.Hosting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //GrpcEnvironment.SetLogger(new ConsoleLogger());

            var hostTask = MagicOnionHost.CreateDefaultBuilder()
                //.UseMagicOnion()
                .UseMagicOnion(types: new[] { typeof(MyService) })
                .UseMagicOnion(configurationName: "MagicOnion-Management", types: new[] { typeof(ManagementService) })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<MagicOnionHostingOptions>(options =>
                    {
                        if (hostContext.HostingEnvironment.IsDevelopment())
                        {
                            options.Service.GlobalFilters = new[] { new MyFilterAttribute(null) };
                        }
                        options.ChannelOptions.MaxReceiveMessageLength = 1024 * 1024 * 10;
                    });
                    services.Configure<MagicOnionHostingOptions>("MagicOnion-Management", options =>
                    {
                    });
                })
                .RunConsoleAsync();


            var isDevelopment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") == "Development";
            var creds = isDevelopment ? ChannelCredentials.Insecure : new SslCredentials(File.ReadAllText("./server.crt"));

            var clientMyService = MagicOnionClient.Create<IMyService>(new Channel("localhost", 12345, creds));
            var clientManagementService = MagicOnionClient.Create<IManagementService>(new Channel("localhost", 23456, creds));
            var result = await clientMyService.HelloAsync();
            var result2 = await clientManagementService.FooBarAsync();

            await hostTask;
        }
    }

    public class MyFilterAttribute : MagicOnionFilterAttribute
    {
        public MyFilterAttribute(Func<ServiceContext, ValueTask> next) : base(next) { }

        public override async ValueTask Invoke(ServiceContext context)
        {
            Console.WriteLine($"MyFilter Begin: {context.CallContext.Method}");
            await Next(context);
            Console.WriteLine($"MyFilter End: {context.CallContext.Method}");
        }
    }

    public interface IMyService : IService<IMyService>
    {
        UnaryResult<string> HelloAsync();
    }
    public class MyService : ServiceBase<IMyService>, IMyService
    {
        public async UnaryResult<string> HelloAsync()
        {
            return "Konnichiwa";
        }
    }

    public interface IManagementService : IService<IManagementService>
    {
        UnaryResult<int> FooBarAsync();
    }
    public class ManagementService : ServiceBase<IManagementService>, IManagementService
    {
        public async UnaryResult<int> FooBarAsync()
        {
            return 123456789;
        }
    }
}
