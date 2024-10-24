using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MagicOnion.Client;
using System.Security.Cryptography;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Tests;

public static class RandomProvider
{
    [ThreadStatic]
    static Random random;

    public static Random ThreadRandom
    {
        get
        {
            if (random == null)
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    var buffer = new byte[sizeof(int)];
                    rng.GetBytes(buffer);
                    var seed = BitConverter.ToInt32(buffer, 0);
                    random = new Random(seed);
                }
            }

            return random;
        }
    }
}

public abstract class ServerFixture : IDisposable
{
    Task hostTask;
    readonly CancellationTokenSource cancellationTokenSource = new();
    readonly TaskCompletionSource serverStartupWaiter = new();

    public GrpcChannel DefaultChannel { get; private set; }
    public Task ServerStarted => serverStartupWaiter.Task;

    public ConcurrentDictionary<string, object> Items { get; } = new();
    public const string ItemsServiceKey = "ServerFixture.Items";

    public ServerFixture()
    {
        PrepareServer();
    }

    protected abstract IEnumerable<Type> GetServiceTypes();

    [MemberNotNull(nameof(hostTask))]
    void PrepareServer()
    {
        var port = RandomProvider.ThreadRandom.Next(10000, 30000);

        // WORKAROUND: Use insecure HTTP/2 connections during testing.
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        hostTask = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
#if DEBUG
                logging.AddFilter(x => true); // Disable all rules.
#endif
                logging.AddDebug();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseKestrel(options =>
                    {
                        options.ListenLocalhost(port, listenOptions =>
                        {
                            // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during testing.
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                    })
                    .UseStartup<Startup>();
            })
            .ConfigureServices(services =>
            {
                services.AddMagicOnion(ConfigureMagicOnion);
                services.AddKeyedSingleton(ItemsServiceKey, Items);

                services.AddKeyedSingleton(HostStartupService.WaiterKey, serverStartupWaiter);
                services.AddHostedService<HostStartupService>();
                services.AddSingleton(this);
            })
            .ConfigureServices(ConfigureServices)
            .Build()
            .RunAsync(cancellationTokenSource.Token);

        if (hostTask.IsFaulted) hostTask.GetAwaiter().GetResult();

        DefaultChannel = GrpcChannel.ForAddress($"http://localhost:{port}");
    }

    class HostStartupService([FromKeyedServices(HostStartupService.WaiterKey)] TaskCompletionSource waiter, IHostApplicationLifetime applicationLifetime) : IHostedService
    {
        public const string WaiterKey = $"{nameof(HostStartupService)}.Waiter";

        public Task StartAsync(CancellationToken cancellationToken)
        {
            applicationLifetime.ApplicationStarted.Register(() => waiter.SetResult());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            waiter.TrySetCanceled();
            return Task.CompletedTask;
        }
    }

    protected virtual void ConfigureMagicOnion(MagicOnionOptions options)
    {
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(); // MagicOnion depends on ASP.NET Core gRPC service.
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService([..app.ApplicationServices.GetRequiredService<ServerFixture>().GetServiceTypes()]);
            });
        }
    }

    public T CreateClient<T>()
        where T : IService<T>
    {
        return MagicOnionClient.Create<T>(DefaultChannel);
    }

    public TStreamingHub CreateStreamingHubClient<TStreamingHub, TReceiver>(TReceiver receiver, StreamingHubClientOptions options = default)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        return CreateStreamingHubClientAsync<TStreamingHub, TReceiver>(receiver, options).GetAwaiter().GetResult();
    }

    public Task<TStreamingHub> CreateStreamingHubClientAsync<TStreamingHub, TReceiver>(TReceiver receiver, StreamingHubClientOptions options = default)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        options ??= StreamingHubClientOptions.CreateWithDefault();
        return StreamingHubClient.ConnectAsync<TStreamingHub, TReceiver>(DefaultChannel, receiver, options);
    }

    public void Dispose()
    {
        try { DefaultChannel.ShutdownAsync().Wait(1000); } catch { }
        try { DefaultChannel.Dispose(); } catch { }

        try { cancellationTokenSource.Cancel(); hostTask.Wait(); } catch { }
    }
}

public class ServerFixture<TServiceOrHub> : ServerFixture
    where TServiceOrHub : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub)];
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2> : ServerFixture
    where TServiceOrHub1 : IServiceMarker
    where TServiceOrHub2 : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub1), typeof(TServiceOrHub2)];
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3> : ServerFixture
    where TServiceOrHub1 : IServiceMarker
    where TServiceOrHub2 : IServiceMarker
    where TServiceOrHub3 : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3)];
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3, TServiceOrHub4> : ServerFixture
    where TServiceOrHub1 : IServiceMarker
    where TServiceOrHub2 : IServiceMarker
    where TServiceOrHub3 : IServiceMarker
    where TServiceOrHub4 : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3), typeof(TServiceOrHub4)];
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3, TServiceOrHub4, TServiceOrHub5> : ServerFixture
    where TServiceOrHub1 : IServiceMarker
    where TServiceOrHub2 : IServiceMarker
    where TServiceOrHub3 : IServiceMarker
    where TServiceOrHub4 : IServiceMarker
    where TServiceOrHub5 : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3), typeof(TServiceOrHub4), typeof(TServiceOrHub5)];
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3, TServiceOrHub4, TServiceOrHub5, TServiceOrHub6> : ServerFixture
    where TServiceOrHub1 : IServiceMarker
    where TServiceOrHub2 : IServiceMarker
    where TServiceOrHub3 : IServiceMarker
    where TServiceOrHub4 : IServiceMarker
    where TServiceOrHub5 : IServiceMarker
    where TServiceOrHub6 : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3), typeof(TServiceOrHub4), typeof(TServiceOrHub5), typeof(TServiceOrHub6)];
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3, TServiceOrHub4, TServiceOrHub5, TServiceOrHub6, TServiceOrHub7> : ServerFixture
    where TServiceOrHub1 : IServiceMarker
    where TServiceOrHub2 : IServiceMarker
    where TServiceOrHub3 : IServiceMarker
    where TServiceOrHub4 : IServiceMarker
    where TServiceOrHub5 : IServiceMarker
    where TServiceOrHub6 : IServiceMarker
    where TServiceOrHub7 : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3), typeof(TServiceOrHub4), typeof(TServiceOrHub5), typeof(TServiceOrHub6), typeof(TServiceOrHub7)];
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3, TServiceOrHub4, TServiceOrHub5, TServiceOrHub6, TServiceOrHub7, TServiceOrHub8> : ServerFixture
    where TServiceOrHub1 : IServiceMarker
    where TServiceOrHub2 : IServiceMarker
    where TServiceOrHub3 : IServiceMarker
    where TServiceOrHub4 : IServiceMarker
    where TServiceOrHub5 : IServiceMarker
    where TServiceOrHub6 : IServiceMarker
    where TServiceOrHub7 : IServiceMarker
    where TServiceOrHub8 : IServiceMarker
{
    protected override IEnumerable<Type> GetServiceTypes()
        => [typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3), typeof(TServiceOrHub4), typeof(TServiceOrHub5), typeof(TServiceOrHub6), typeof(TServiceOrHub7), typeof(TServiceOrHub8)];
}

[CollectionDefinition(nameof(AllAssemblyGrpcServerFixture))]
public class AllAssemblyGrpcServerFixture : ICollectionFixture<ServerFixture>
{ }
