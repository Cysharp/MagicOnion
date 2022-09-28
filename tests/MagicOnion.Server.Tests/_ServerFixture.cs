using MagicOnion.Client;
using MagicOnion.Server;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

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
    private Task _hostTask;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public GrpcChannel DefaultChannel { get; private set; }

    public ServerFixture()
    {
        PrepareServer();
    }

    protected abstract IEnumerable<Type> GetServiceTypes();

    protected virtual void PrepareServer()
    {
        var port = RandomProvider.ThreadRandom.Next(10000, 30000);

        // WORKAROUND: Use insecure HTTP/2 connections during testing.
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        _hostTask = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
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
                services.AddMagicOnion(GetServiceTypes(), ConfigureMagicOnion);
            })
            .ConfigureServices(ConfigureServices)
            .Build()
            .RunAsync(_cancellationTokenSource.Token);

        DefaultChannel = GrpcChannel.ForAddress($"http://localhost:{port}");
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
                endpoints.MapMagicOnionService();
            });
        }
    }

    public T CreateClient<T>()
        where T : IService<T>
    {
        return MagicOnionClient.Create<T>(DefaultChannel);
    }

    public TStreamingHub CreateStreamingHubClient<TStreamingHub, TReceiver>(TReceiver receiver)
        where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        return StreamingHubClient.ConnectAsync<TStreamingHub, TReceiver>(DefaultChannel, receiver).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        try { DefaultChannel.ShutdownAsync().Wait(1000); } catch { }

        try { _cancellationTokenSource.Cancel(); _hostTask.Wait(); } catch { }
    }
}

public class ServerFixture<TServiceOrHub> : ServerFixture
{
    protected override IEnumerable<Type> GetServiceTypes()
        => new [] { typeof(TServiceOrHub) };
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2> : ServerFixture
{
    protected override IEnumerable<Type> GetServiceTypes()
        => new[] { typeof(TServiceOrHub1), typeof(TServiceOrHub2) };
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3> : ServerFixture
{
    protected override IEnumerable<Type> GetServiceTypes()
        => new[] { typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3) };
}
public class ServerFixture<TServiceOrHub1, TServiceOrHub2, TServiceOrHub3, TServiceOrHub4> : ServerFixture
{
    protected override IEnumerable<Type> GetServiceTypes()
        => new[] { typeof(TServiceOrHub1), typeof(TServiceOrHub2), typeof(TServiceOrHub3), typeof(TServiceOrHub4) };
}

[CollectionDefinition(nameof(AllAssemblyGrpcServerFixture))]
public class AllAssemblyGrpcServerFixture : ICollectionFixture<ServerFixture>
{ }