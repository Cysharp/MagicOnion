using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Hosting;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
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
                            options.Service.GlobalStreamingHubFilters.Add<MyStreamingHubFilterAttribute>();
                            // options.Service.GlobalStreamingHubFilters.Add(new MyStreamingHubFilterAttribute(logger));

                            options.Service.GlobalFilters.Add<MyFilterAttribute>(); // Register filter by type.
                            options.Service.GlobalFilters.Add(new MyFilterUsingFactoryAttribute("Global")); // Register filter with IMagicOnionFilterFactory.
                            // options.Service.GlobalFilters.Add(new MyFilterAttribute(null));

                            // options.ServerPorts = new[]{ new MagicOnionHostingServerPortOptions(){ Port = 12345, Host = "0.0.0.0", UseInsecureConnection = true } };
                        }
                        options.ChannelOptions.MaxReceiveMessageLength = 1024 * 1024 * 10;
                        options.ChannelOptions.Add(new ChannelOption("grpc.keepalive_time_ms", 10000));
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

    public class MyStreamingHubFilterAttribute : StreamingHubFilterAttribute
    {
        private readonly ILogger _logger;

        public MyStreamingHubFilterAttribute(ILogger<MyStreamingHubFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            _logger.LogInformation($"MyStreamingHubFilter Begin: {context.Path}");
            await next(context);
            _logger.LogInformation($"MyStreamingHubFilter End: {context.Path}");
        }
    }


    public class MyFilterAttribute : MagicOnionFilterAttribute
    {
        private readonly ILogger _logger;

        public MyFilterAttribute(ILogger<MyFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            _logger.LogInformation($"MyFilter Begin: {context.CallContext.Method}");
            await next(context);
            _logger.LogInformation($"MyFilter End: {context.CallContext.Method}");
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class MyFilterUsingFactoryAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public int Order { get; set; }
        public string Label { get; set; }

        public MyFilterUsingFactoryAttribute(string label)
        {
            Label = label;
        }

        MagicOnionFilterAttribute IMagicOnionFilterFactory<MagicOnionFilterAttribute>.CreateInstance(IServiceLocator serviceLocator)
        {
            return new FilterImpl(serviceLocator.GetService<ILoggerFactory>().CreateLogger<MyFilterUsingFactoryAttribute>(), Label);
        }

        internal class FilterImpl : MagicOnionFilterAttribute
        {
            private readonly ILogger _logger;
            private readonly string _label;

            public FilterImpl(ILogger logger, string label)
            {
                this._logger = logger;
                this._label = label;
            }

            public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            {
                _logger.LogInformation($"MyFilterFactoryFilterImpl[{_label}] Begin: {context.CallContext.Method}");
                await next(context);
                _logger.LogInformation($"MyFilterFactoryFilterImpl[{_label}] End: {context.CallContext.Method}");
            }
        }
    }

    public interface IMyService : IService<IMyService>
    {
        UnaryResult<string> HelloAsync();
    }
    public class MyService : ServiceBase<IMyService>, IMyService
    {
        public MyService()
        {

        }
        [MyFilterUsingFactory("PerMethod")]
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
