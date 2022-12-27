using System.Net.Sockets;
using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace MagicOnion.Server.Redis.Tests;

public class TemporaryRedisServerFixture : IAsyncLifetime
{
    TestcontainersContainer? container;

    public int Port { get; }

    public TemporaryRedisServerFixture()
    {
        this.Port = GetAvailableListenerPort();
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("redis")
             // WORKAROUND: `assignRandomHostPort` on Windows, a port within the range of `excludedport` setting may be selected.
             //             We get an available port from the operating system.
            .WithPortBinding(Port, 6379)
            .Build();

        await container.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (container is not null)
        {
            await container.DisposeAsync();
        }
    }

    static int GetAvailableListenerPort()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }
}
