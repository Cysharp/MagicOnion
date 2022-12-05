using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using MagicOnionTestServer;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Integration.Tests;

public class MessageSerializerStreamingHubTest : IClassFixture<MagicOnionApplicationFactory<MessageSerializerTestHub>>
{
    readonly WebApplicationFactory<Program> factory;

    public MessageSerializerStreamingHubTest(MagicOnionApplicationFactory<MessageSerializerTestHub> factory)
    {
        this.factory = factory.WithMagicOnionOptions(x =>
        {
            x.MessageSerializer = XorMagicOnionMessagePackSerializer.Instance;
        });
    }

    public static IEnumerable<object[]> EnumerateStreamingHubClientFactory()
    {
        yield return new [] { new TestStreamingHubClientFactory<IMessageSerializerTestHub, IMessageSerializerTestHubReceiver>("Dynamic", (callInvoker, receiver, messageSerializer) => StreamingHubClient.ConnectAsync<IMessageSerializerTestHub, IMessageSerializerTestHubReceiver>(callInvoker, receiver, messageSerializer: messageSerializer)) };
        yield return new [] { new TestStreamingHubClientFactory<IMessageSerializerTestHub, IMessageSerializerTestHubReceiver>("Static", async (callInvoker, receiver, messageSerializer) =>
        {
            var client = new MessageSerializerTestHubClient(callInvoker, string.Empty, new CallOptions(), messageSerializer ?? MagicOnionMessageSerializer.Default, NullMagicOnionClientLogger.Instance);
            await client.__ConnectAndSubscribeAsync(receiver, default);
            return client;
        })};
    }

    [Theory]
    [MemberData(nameof(EnumerateStreamingHubClientFactory))]
    public async Task StreamingHub_Parameterless(TestStreamingHubClientFactory<IMessageSerializerTestHub, IMessageSerializerTestHubReceiver> clientFactory)
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var client = await clientFactory.CreateAndConnectAsync(channel, receiver, messageSerializer: XorMagicOnionMessagePackSerializer.Instance);

        // Act
        var result  = await client.MethodParameterless();

        // Assert
        result.Should().Be(123);
    }

    class Receiver : IMessageSerializerTestHubReceiver
    {
        public List<(int Arg0, string Arg1)> Received { get; } = new List<(int Arg0, string Arg1)>();
        public void OnMessage(int arg0, string arg1)
        {
            Received.Add((arg0, arg1));
        }
    }
}


public interface IMessageSerializerTestHubReceiver
{
    void OnMessage(int arg0, string arg1);
}

public interface IMessageSerializerTestHub : IStreamingHub<IMessageSerializerTestHub, IMessageSerializerTestHubReceiver>
{
    Task MethodReturnWithoutValue();
    Task<int> MethodParameterless();
    Task<int> MethodParameter_One(int arg0);
    Task<int> MethodParameter_Many(int arg0, string arg1);
    Task<int> Callback(int arg0, string arg1);
}

public class MessageSerializerTestHub : StreamingHubBase<IMessageSerializerTestHub, IMessageSerializerTestHubReceiver>, IMessageSerializerTestHub
{
    public Task<int> MethodAsync(int arg0, string arg1) => Task.FromResult(123);
    public Task MethodReturnWithoutValue() => Task.CompletedTask;
    public Task<int> MethodParameterless() => Task.FromResult(123);
    public Task<int> MethodParameter_One(int arg0) => Task.FromResult(arg0 + 123);
    public Task<int> MethodParameter_Many(int arg0, string arg1) => Task.FromResult(arg0 + 123);

    public async Task<int> Callback(int arg0, string arg1)
    {
        var group = await Group.AddAsync(Guid.NewGuid().ToString());
        Broadcast(group).OnMessage(arg0, arg1);
        return 123;
    }
}

