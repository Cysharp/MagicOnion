using System.Buffers;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MagicOnion.Integration.Tests;

public class HandCraftedStreamingHubClientTest : IClassFixture<MagicOnionApplicationFactory<HandCraftedStreamingHubClientTestHub>>
{
    readonly WebApplicationFactory<Program> factory;

    public HandCraftedStreamingHubClientTest(MagicOnionApplicationFactory<HandCraftedStreamingHubClientTestHub> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task MethodParameterless()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var clientConnectTask = StreamingHubClient.ConnectAsync<IHandCraftedStreamingHubClientTestHub, IHandCraftedStreamingHubClientTestHubReceiver>(channel.CreateCallInvoker(), receiver, StreamingHubClientOptions.CreateWithDefault(), factoryProvider: HandCraftedClientFactoryProvider.Instance);

        // Act
        var client = await clientConnectTask;
        var retVal = await client.MethodParameterless();

        // Assert
        retVal.Should().Be(123);
    }

    [Fact]
    public async Task Callback()
    {
        // Arrange
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = factory.CreateDefaultClient() });
        var receiver = new Receiver();
        var clientConnectTask = StreamingHubClient.ConnectAsync<IHandCraftedStreamingHubClientTestHub, IHandCraftedStreamingHubClientTestHubReceiver>(channel.CreateCallInvoker(), receiver, StreamingHubClientOptions.CreateWithDefault(), factoryProvider: HandCraftedClientFactoryProvider.Instance);

        // Act
        var client = await clientConnectTask;
        var retVal = await client.Callback(1234, "FooBarBaz");
        await Task.Delay(500); // Wait for the broadcast queue to be consumed.

        // Assert
        retVal.Should().Be(123);
        receiver.Results.Should().Contain("1234,FooBarBaz");
    }

    class Receiver : IHandCraftedStreamingHubClientTestHubReceiver
    {
        public List<string> Results { get; } = new List<string>();
        public void OnMessage(int arg0, string arg1)
        {
            Results.Add($"{arg0},{arg1}");
        }
    }

    class HandCraftedClientFactoryProvider : IStreamingHubClientFactoryProvider
    {
        public static IStreamingHubClientFactoryProvider Instance { get; } = new HandCraftedClientFactoryProvider();

        public bool TryGetFactory<TStreamingHub, TReceiver>(out StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver> factory) where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            factory = (receiver, invoker, options) => (TStreamingHub)(object)new __HandCraftedClient__IHandCraftedStreamingHubClientTestHub((IHandCraftedStreamingHubClientTestHubReceiver)(object)receiver!, invoker, options);
            return true;
        }
    }

    class __HandCraftedClient__IHandCraftedStreamingHubClientTestHub : StreamingHubClientBase<IHandCraftedStreamingHubClientTestHub, IHandCraftedStreamingHubClientTestHubReceiver>, IHandCraftedStreamingHubClientTestHub
    {
        public __HandCraftedClient__IHandCraftedStreamingHubClientTestHub(IHandCraftedStreamingHubClientTestHubReceiver receiver, CallInvoker callInvoker, StreamingHubClientOptions options)
            : base(nameof(IHandCraftedStreamingHubClientTestHub), receiver, callInvoker, options)
        {
        }


        protected override void OnResponseEvent(int methodId, object taskCompletionSource, ReadOnlyMemory<byte> data)
        {
            if (FNV1A32.GetHashCode(nameof(MethodParameterless)) == methodId)
            {
                base.SetResultForResponse<int>(taskCompletionSource, data);
            }
            else if (FNV1A32.GetHashCode(nameof(Callback)) == methodId)
            {
                base.SetResultForResponse<int>(taskCompletionSource, data);
            }
        }

        protected override void OnBroadcastEvent(int methodId, ReadOnlyMemory<byte> data)
        {
            if (FNV1A32.GetHashCode(nameof(IHandCraftedStreamingHubClientTestHubReceiver.OnMessage)) == methodId)
            {
                var value = base.Deserialize<DynamicArgumentTuple<int, string>>(data);
                receiver.OnMessage(value.Item1, value.Item2);
            }
        }

        protected override void OnClientResultEvent(int methodId, Guid messageId, ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public IHandCraftedStreamingHubClientTestHub FireAndForget()
        {
            return new FireAndForgetClient(this);
        }

        [Ignore]
        class FireAndForgetClient : IHandCraftedStreamingHubClientTestHub
        {
            readonly __HandCraftedClient__IHandCraftedStreamingHubClientTestHub parent;

            public FireAndForgetClient(__HandCraftedClient__IHandCraftedStreamingHubClientTestHub parent)
                => this.parent = parent;

            public IHandCraftedStreamingHubClientTestHub FireAndForget() => this;
            public Task DisposeAsync() => throw new NotSupportedException();
            public Task WaitForDisconnect() => throw new NotSupportedException();

            public Task MethodReturnWithoutValue()
            {
                return parent.WriteMessageFireAndForgetTaskAsync<MessagePack.Nil, MessagePack.Nil>(FNV1A32.GetHashCode(nameof(MethodReturnWithoutValue)), MessagePack.Nil.Default);
            }

            public Task<int> MethodParameterless()
            {
                return parent.WriteMessageFireAndForgetTaskAsync<MessagePack.Nil, int>(FNV1A32.GetHashCode(nameof(MethodParameterless)), MessagePack.Nil.Default);
            }

            public Task<int> MethodParameter_One(int arg0)
            {
                return parent.WriteMessageFireAndForgetTaskAsync<int, int>(FNV1A32.GetHashCode(nameof(MethodParameter_One)), arg0);
            }

            public Task<int> MethodParameter_Many(int arg0, string arg1)
            {
                return parent.WriteMessageFireAndForgetTaskAsync<DynamicArgumentTuple<int, string>, int>(FNV1A32.GetHashCode(nameof(MethodParameter_Many)), new DynamicArgumentTuple<int, string>(arg0, arg1));
            }

            public Task<int> Callback(int arg0, string arg1)
            {
                return parent.WriteMessageFireAndForgetTaskAsync<DynamicArgumentTuple<int, string>, int>(FNV1A32.GetHashCode(nameof(Callback)), new DynamicArgumentTuple<int, string>(arg0, arg1));
            }
        }

        public Task MethodReturnWithoutValue()
        {
            return base.WriteMessageWithResponseTaskAsync<MessagePack.Nil, MessagePack.Nil>(FNV1A32.GetHashCode(nameof(MethodReturnWithoutValue)), MessagePack.Nil.Default);
        }

        public Task<int> MethodParameterless()
        {
            return WriteMessageWithResponseTaskAsync<MessagePack.Nil, int>(FNV1A32.GetHashCode(nameof(MethodParameterless)), MessagePack.Nil.Default);
        }

        public Task<int> MethodParameter_One(int arg0)
        {
            return WriteMessageWithResponseTaskAsync<int, int>(FNV1A32.GetHashCode(nameof(MethodParameter_One)), arg0);
        }

        public Task<int> MethodParameter_Many(int arg0, string arg1)
        {
            return WriteMessageWithResponseTaskAsync<DynamicArgumentTuple<int, string>, int>(FNV1A32.GetHashCode(nameof(MethodParameter_Many)), new DynamicArgumentTuple<int, string>(arg0, arg1));
        }

        public Task<int> Callback(int arg0, string arg1)
        {
            return WriteMessageWithResponseTaskAsync<DynamicArgumentTuple<int, string>, int>(FNV1A32.GetHashCode(nameof(Callback)), new DynamicArgumentTuple<int, string>(arg0, arg1));
        }
    }
}



public interface IHandCraftedStreamingHubClientTestHubReceiver
{
    void OnMessage(int arg0, string arg1);
}

public interface IHandCraftedStreamingHubClientTestHub : IStreamingHub<IHandCraftedStreamingHubClientTestHub, IHandCraftedStreamingHubClientTestHubReceiver>
{
    Task MethodReturnWithoutValue();
    Task<int> MethodParameterless();
    Task<int> MethodParameter_One(int arg0);
    Task<int> MethodParameter_Many(int arg0, string arg1);
    Task<int> Callback(int arg0, string arg1);
}

public class HandCraftedStreamingHubClientTestHub : StreamingHubBase<IHandCraftedStreamingHubClientTestHub, IHandCraftedStreamingHubClientTestHubReceiver>, IHandCraftedStreamingHubClientTestHub
{
    public Task<int> MethodAsync(int arg0, string arg1) => Task.FromResult(123);
    public Task MethodReturnWithoutValue() => Task.CompletedTask;
    public Task<int> MethodParameterless() => Task.FromResult(123);
    public Task<int> MethodParameter_One(int arg0) => Task.FromResult(arg0 + 123);
    public Task<int> MethodParameter_Many(int arg0, string arg1) => Task.FromResult(arg0 + 123);

    public async Task<int> Callback(int arg0, string arg1)
    {
        var group = await Group.AddAsync(Guid.NewGuid().ToString());
        group.All.OnMessage(arg0, arg1);
        return 123;
    }
}
