using Cysharp.Runtime.Multicast;
using Cysharp.Runtime.Multicast.Distributed.Redis;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;

namespace MagicOnion.Server.Redis.Tests;

public class RedisGroupFunctionalTest : IClassFixture<MagicOnionApplicationFactory<RedisGroupFunctionalTestHub>>, IClassFixture<TemporaryRedisServerFixture>
{
    readonly WebApplicationFactory<MagicOnionTestServer.Program> factory;
    readonly WebApplicationFactory<MagicOnionTestServer.Program> factory2;
    readonly TemporaryRedisServerFixture redisServer;

    public RedisGroupFunctionalTest(MagicOnionApplicationFactory<RedisGroupFunctionalTestHub> factory, TemporaryRedisServerFixture redisServer)
    {
        this.redisServer = redisServer;
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMulticastGroupProvider>();
                services.TryAddSingleton<IMulticastGroupProvider, RedisGroupProvider>();
                services.Configure<RedisGroupOptions>(options =>
                {
                    options.ConnectionMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisServer.GetConnectionString());
                });
            });
        });
        this.factory2 = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMulticastGroupProvider>();
                services.TryAddSingleton<IMulticastGroupProvider, RedisGroupProvider>();
                services.Configure<RedisGroupOptions>(options =>
                {
                    options.ConnectionMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisServer.GetConnectionString());
                });
            });
        });
    }

    [Fact]
    public async Task Broadcast()
    {
        // Arrange
        // Client-1 on Server-1
        var httpClient1 = factory.CreateDefaultClient();
        var channel1 = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient1 });
        var receiver1 = Substitute.For<IRedisGroupFunctionalTestHubReceiver>();
        var client1 = await StreamingHubClient.ConnectAsync<IRedisGroupFunctionalTestHub, IRedisGroupFunctionalTestHubReceiver>(channel1, receiver1);
        await client1.JoinAsync("group-1");
        // Client-2 on Server-2
        var httpClient2 = factory2.CreateDefaultClient();
        var channel2 = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient2 });
        var receiver2 = Substitute.For<IRedisGroupFunctionalTestHubReceiver>();
        var client2 = await StreamingHubClient.ConnectAsync<IRedisGroupFunctionalTestHub, IRedisGroupFunctionalTestHubReceiver>(channel2, receiver2);
        await client2.JoinAsync("group-1");

        // Act
        // Cilent-1 --> Server-1 --> Redis --> Server-2 --> Client-2
        await client1.CallAsync(12345);
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver1.Received().OnMessage(12345);
        receiver2.Received().OnMessage(12345);
    }

    [Fact]
    public async Task RemoveMemberFromInMemoryGroup_KeepSubscription()
    {
        // Arrange
        var groupName = nameof(RemoveMemberFromInMemoryGroup_KeepSubscription);
        // Client-1 on Server-1
        var httpClient1 = factory.CreateDefaultClient();
        var channel1 = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient1 });
        var receiver1 = Substitute.For<IRedisGroupFunctionalTestHubReceiver>();
        var client1 = await StreamingHubClient.ConnectAsync<IRedisGroupFunctionalTestHub, IRedisGroupFunctionalTestHubReceiver>(channel1, receiver1);
        await client1.JoinAsync(groupName);
        // Client-2 on Server-2
        var httpClient2 = factory2.CreateDefaultClient();
        var channel2 = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient2 });
        var receiver2 = Substitute.For<IRedisGroupFunctionalTestHubReceiver>();
        var client2 = await StreamingHubClient.ConnectAsync<IRedisGroupFunctionalTestHub, IRedisGroupFunctionalTestHubReceiver>(channel2, receiver2);
        await client2.JoinAsync(groupName);
        // Client-3 on Server-2 (same as Client-2)
        var httpClient3 = factory2.CreateDefaultClient();
        var channel3 = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient3 });
        var receiver3 = Substitute.For<IRedisGroupFunctionalTestHubReceiver>();
        var client3 = await StreamingHubClient.ConnectAsync<IRedisGroupFunctionalTestHub, IRedisGroupFunctionalTestHubReceiver>(channel3, receiver3);
        await client3.JoinAsync(groupName);

        // Act
        await client2.LeaveAsync(); // Leave Client-2 from the group on Server-2.
        await client1.CallAsync(123); // Client-1 --> Server-1 --> Redis --> Server-2 --> Client-3
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver3.Received().OnMessage(123);
    }
}

public interface IRedisGroupFunctionalTestHubReceiver
{
    void OnMessage(int arg0);
}

public interface IRedisGroupFunctionalTestHub : IStreamingHub<IRedisGroupFunctionalTestHub, IRedisGroupFunctionalTestHubReceiver>
{
    Task JoinAsync(string groupName);
    Task LeaveAsync();
    Task CallAsync(int arg0);
}

public class RedisGroupFunctionalTestHub : StreamingHubBase<IRedisGroupFunctionalTestHub, IRedisGroupFunctionalTestHubReceiver>, IRedisGroupFunctionalTestHub
{
    IGroup<IRedisGroupFunctionalTestHubReceiver>? group;

    public async Task JoinAsync(string groupName)
    {
        group = await Group.AddAsync(groupName);
    }

    public async Task LeaveAsync()
    {
        if (group is null) return;
        await group.RemoveAsync(Context);
    }

    public Task CallAsync(int arg0)
    {
        group!.All.OnMessage(arg0);
        return Task.CompletedTask;
    }
}
