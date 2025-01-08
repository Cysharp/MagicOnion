using System.Net.Sockets;
using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace MagicOnion.Server.Redis.Tests;

public sealed class TemporaryRedisServerFixture : IAsyncLifetime
{
    const string redisImage = "redis:7.0";

    const ushort redisPort = 6379;

    readonly TestcontainersContainer container = new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage(redisImage)
        .WithPortBinding(GetAvailableListenerPort(), redisPort)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("redis-cli", "ping"))
        .Build();

    public string GetConnectionString()
    {
        return $"{container.Hostname}:{container.GetMappedPublicPort(redisPort)}";
    }

    ValueTask IAsyncLifetime.InitializeAsync()
    {
        return new ValueTask(container.StartAsync());
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return container.DisposeAsync();
    }

    static int GetAvailableListenerPort()
    {
        // WORKAROUND: `assignRandomHostPort` on Windows, a port within the range of `excludedport` setting may be selected.
        //             We get an available port from the operating system.
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }
    }
}
