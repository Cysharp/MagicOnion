#pragma warning disable CS1998

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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Hosting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //GrpcEnvironment.SetLogger(new ConsoleLogger());

            var isDevelopment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development";
            var creds = isDevelopment ? ChannelCredentials.Insecure : new SslCredentials(File.ReadAllText("./server.crt"));

            var clientMyService = MagicOnionClient.Create<IMyService>(new Channel("localhost", 12345, creds));

            var hostTask = MagicOnionHost.CreateDefaultBuilder()
                //.UseMagicOnion()
                .UseMagicOnion(types: new [] { typeof(MyService), typeof(MyHub) })
                .UseMagicOnion(configurationName: "MagicOnion-Management", types: new[] { typeof(ManagementService) })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<MySingletonService>();
                    services.AddScoped<MyScopedService>();
                    services.AddTransient<MyTransientService>();

                    services.Configure<MagicOnionHostingOptions>(options =>
                    {
                        if (hostContext.HostingEnvironment.IsDevelopment())
                        {
                            options.Service.GlobalStreamingHubFilters.Add<MyStreamingHubFilterAttribute>();
                            // options.Service.GlobalStreamingHubFilters.Add(new MyStreamingHubFilterAttribute(logger));

                            options.Service.GlobalFilters.Add<MyFilterAttribute>(); // Register filter by type.
                            options.Service.GlobalFilters.Add(new MyFilterUsingFactoryAttribute("Global")); // Register filter with IMagicOnionFilterFactory.
                            // options.Service.GlobalFilters.Add(new MyFilterAttribute(null));
                            options.Service.GlobalFilters.Add<MyFilterAttribute>();
                            options.Service.GlobalFilters.Add<MyFilter2Attribute>();
                            options.Service.GlobalFilters.Add<MyFilter3Attribute>();
                            options.Service.GlobalFilters.Add<MyFilter4Attribute>();

                            // options.ServerPorts = new[]{ new MagicOnionHostingServerPortOptions(){ Port = 12345, Host = "0.0.0.0", UseInsecureConnection = true } };
                        }
                        options.ChannelOptions.MaxReceiveMessageLength = 1024 * 1024 * 10;
                        options.ChannelOptions.Add(new ChannelOption("grpc.keepalive_time_ms", 10000));
                    });
                    services.Configure<MagicOnionHostingOptions>("MagicOnion-Management", options =>
                    {
                    });

                    services.AddHostedService<MyHostedService>();
                })
                .RunConsoleAsync();

            //var clientMyService = MagicOnionClient.Create<IMyService>(new Channel("localhost", 12345, creds));
            var clientManagementService = MagicOnionClient.Create<IManagementService>(new Channel("localhost", 23456, creds));
            var result = await clientMyService.HelloAsync();
            result = await clientMyService.HelloAsync();
            result = await clientMyService.HelloAsync();
            var result2 = await clientManagementService.FooBarAsync();

            var clientHub = StreamingHubClient.Connect<IMyHub, IMyHubReceiver>(new Channel("localhost", 12345, creds), new MyHubReceiver());
            var result3 = await clientHub.HelloAsync();

            var streamingHubClient = StreamingHubClient.Connect<IMyHub, IMyHubReceiver>(new Channel("localhost", 12345, creds), null);
            await streamingHubClient.HelloAsync();

            var streamingHubClient2 = StreamingHubClient.Connect<IMyHub, IMyHubReceiver>(new Channel("localhost", 12345, creds), null);
            await streamingHubClient2.HelloAsync();
            await streamingHubClient2.HelloAsync();
            await streamingHubClient2.HelloAsync();
            await streamingHubClient2.DisposeAsync();

            await hostTask;
        }

        class MyHubReceiver : IMyHubReceiver
        {
            public void OnNantoka(string value)
            {
                Console.WriteLine(value);
            }
        }
    }

    public class MyHostedService : IHostedService
    {
        public MyHostedService(MySingletonService service)
        {

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class MySingletonService : IDisposable
    {
        public MySingletonService() => Console.WriteLine($"{this.GetType().Name}..ctor");
        public void Dispose() => Console.WriteLine($"{this.GetType().Name}.Dispose");
    }

    public class MyTransientService : IDisposable
    {
        public MyTransientService() => Console.WriteLine($"{this.GetType().Name}..ctor");
        public void Dispose() => Console.WriteLine($"{this.GetType().Name}.Dispose");
    }

    public class MyScopedService : IDisposable
    {
        public MyScopedService() => Console.WriteLine($"{this.GetType().Name}..ctor");
        public void Dispose() => Console.WriteLine($"{this.GetType().Name}.Dispose");
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
            context.ServiceLocator.GetService<MyTransientService>();
            context.ServiceLocator.GetService<MyTransientService>();
            context.ServiceLocator.GetService<MyTransientService>();
            context.ServiceLocator.GetService<MySingletonService>();
            context.ServiceLocator.GetService<MySingletonService>();
            context.ServiceLocator.GetService<MySingletonService>();
            context.ServiceLocator.GetService<MyScopedService>();
            context.ServiceLocator.GetService<MyScopedService>();
            context.ServiceLocator.GetService<MyScopedService>();

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
            return new MyFilterUsingFactory(serviceLocator.GetService<ILoggerFactory>().CreateLogger<MyFilterUsingFactoryAttribute>(), Label);
        }
    }

    internal class MyFilterUsingFactory : MagicOnionFilterAttribute
    {
        private readonly ILogger _logger;
        private readonly string _label;

        public MyFilterUsingFactory(ILogger logger, string label)
        {
            this._logger = logger;
            this._label = label;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            _logger.LogInformation($"MyFilterUsingFactory[{_label}] Begin: {context.CallContext.Method}");
            await next(context);
            _logger.LogInformation($"MyFilterUsingFactory[{_label}] End: {context.CallContext.Method}");
        }
    }

    public class MyFilter2Attribute : MagicOnionFilterAttribute
    {
        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next) => await next(context);
    }


    public class MyFilter3Attribute : MagicOnionFilterAttribute
    {
        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next) => await next(context);
    }

    public class MyFilter4Attribute : IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public MagicOnionFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new FilterImpl();
        }

        public int Order { get; }

        class FilterImpl : MagicOnionFilterAttribute
        {
            public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            {
                return next(context);
            }
        }
    }

    public interface IMyService : IService<IMyService>
    {
        UnaryResult<string> HelloAsync();
    }
    public class MyService : ServiceBase<IMyService>, IMyService
    {
        public MyService(MySingletonService foo, MyScopedService bar)
        {
        }
        [MyFilterUsingFactoryAttribute("PerMethod")]
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

    public interface IMyHubReceiver
    {
        void OnNantoka(string value);
    }

    public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
        Task<string> HelloAsync();
    }

    public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>, IMyHub
    {
        public MyHub(MyScopedService scopedService)
        { }

        public MyHub() => Console.WriteLine($"{this.GetType().Name}..ctor");

        public async Task<string> HelloAsync()
        {
            var group = await this.Group.AddAsync("Nantoka");
            group.CreateBroadcaster<IMyHubReceiver>().OnNantoka("BroadcastAll");
            group.CreateBroadcasterTo<IMyHubReceiver>(Context.ContextId).OnNantoka("BroadcastTo(Self)");
            group.CreateBroadcasterTo<IMyHubReceiver>(Guid.NewGuid()).OnNantoka("BroadcastTo(Non-self)");
            group.CreateBroadcasterTo<IMyHubReceiver>(new[] { Guid.NewGuid(), Guid.NewGuid() }).OnNantoka("BroadcastTo(Non-self, Non-self)");
            group.CreateBroadcasterTo<IMyHubReceiver>(new[] { Context.ContextId, Guid.NewGuid() }).OnNantoka("BroadcastTo(Self, Non-self)");
            group.CreateBroadcasterExcept<IMyHubReceiver>(Context.ContextId).OnNantoka("BroadcastExcept(Self)");
            group.CreateBroadcasterExcept<IMyHubReceiver>(Guid.NewGuid()).OnNantoka("BroadcastExcept(Non-self)");
            group.CreateBroadcasterExcept<IMyHubReceiver>(new[] { Guid.NewGuid(), Guid.NewGuid() }).OnNantoka("BroadcastExcept(Non-self, Non-self)");
            group.CreateBroadcasterExcept<IMyHubReceiver>(new[] { Context.ContextId, Guid.NewGuid() }).OnNantoka("BroadcastExcept(Self, Non-self)");

            return "Konnnichiwa!";
        }
    }
}
