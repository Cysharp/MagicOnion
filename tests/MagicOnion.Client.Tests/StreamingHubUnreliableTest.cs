using MagicOnion.Client.DynamicClient;
using MagicOnion.Internal;

namespace MagicOnion.Client.Tests;

/// <summary>
/// Verifies that the dynamic client sends unreliable Hub methods as fire-and-forget requests and keeps reliable methods unchanged.
/// Until an additional transport is negotiated, unreliable calls travel over the gRPC connection.
/// </summary>
public class StreamingHubUnreliableTest
{
    [Fact]
    public async Task Unreliable_Task_IsSentAsFireAndForget()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IUnreliableTestHub, IUnreliableTestHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        var t = client.MoveAsync(42);
        Assert.True(t.IsCompletedSuccessfully);
        await t;

        var (methodId, request) = await helper.ReadFireAndForgetRequestAsync<int>().WaitAsync(timeout.Token);
        Assert.Equal(FNV1A32.GetHashCode(nameof(IUnreliableTestHub.MoveAsync)), methodId);
        Assert.Equal(42, request);
    }

    [Fact]
    public async Task Unreliable_ValueTask_IsSentAsFireAndForget()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IUnreliableTestHub, IUnreliableTestHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        var t = client.MoveWithNameAsync(7, "Alice");
        Assert.True(t.IsCompletedSuccessfully);
        await t;

        var (methodId, request) = await helper.ReadFireAndForgetRequestAsync<DynamicArgumentTuple<int, string>>().WaitAsync(timeout.Token);
        Assert.Equal(FNV1A32.GetHashCode(nameof(IUnreliableTestHub.MoveWithNameAsync)), methodId);
        Assert.Equal(7, request.Item1);
        Assert.Equal("Alice", request.Item2);
    }

    [Fact]
    public async Task UnreliableUnordered_Void_IsSentAsFireAndForget()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IUnreliableTestHub, IUnreliableTestHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        client.Emote(3);

        var (methodId, request) = await helper.ReadFireAndForgetRequestAsync<int>().WaitAsync(timeout.Token);
        Assert.Equal(FNV1A32.GetHashCode(nameof(IUnreliableTestHub.Emote)), methodId);
        Assert.Equal(3, request);
    }

    [Fact]
    public async Task Unreliable_ThroughFireAndForgetProxy_IsSentAsFireAndForget()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IUnreliableTestHub, IUnreliableTestHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        await client.FireAndForget().MoveAsync(5);

        var (methodId, request) = await helper.ReadFireAndForgetRequestAsync<int>().WaitAsync(timeout.Token);
        Assert.Equal(FNV1A32.GetHashCode(nameof(IUnreliableTestHub.MoveAsync)), methodId);
        Assert.Equal(5, request);
    }

    [Fact]
    public async Task Reliable_MethodOnSameHub_StillWaitsForResponse()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IUnreliableTestHub, IUnreliableTestHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        var t = client.QueryAsync(1).WaitAsync(timeout.Token);
        Assert.False(t.IsCompleted);

        var (messageId, methodId, request) = await helper.ReadRequestAsync<int>().WaitAsync(timeout.Token);
        helper.WriteResponse(messageId, methodId, 99);

        Assert.Equal(FNV1A32.GetHashCode(nameof(IUnreliableTestHub.QueryAsync)), methodId);
        Assert.Equal(1, request);
        Assert.Equal(99, await t);
    }

    [Fact]
    public async Task InterfaceDefault_Unreliable_WithReliableOverride()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IDefaultUnreliableTestHub, IUnreliableTestHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);
        var client = await helper.ConnectAsync(timeout.Token);

        // Interface default applies: fire-and-forget.
        await client.MoveAsync();
        var (moveMethodId, _) = await helper.ReadFireAndForgetRequestAsync<Nil>().WaitAsync(timeout.Token);
        Assert.Equal(FNV1A32.GetHashCode(nameof(IDefaultUnreliableTestHub.MoveAsync)), moveMethodId);

        // [Reliable] override: request/response.
        var join = client.JoinAsync("Bob").WaitAsync(timeout.Token);
        var (messageId, joinMethodId, name) = await helper.ReadRequestAsync<string>().WaitAsync(timeout.Token);
        helper.WriteResponse(messageId, joinMethodId, 1234);
        Assert.Equal("Bob", name);
        Assert.Equal(1234, await join);
    }

    [Fact]
    public async Task InvalidContract_UnreliableMethodWithResult_FailsToConnect()
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var helper = new StreamingHubClientTestHelper<IInvalidUnreliableTestHub, IUnreliableTestHubReceiver>(factoryProvider: DynamicStreamingHubClientFactoryProvider.Instance);

        var ex = await Record.ExceptionAsync(async () => await helper.ConnectAsync(timeout.Token));

        Assert.NotNull(ex);
        // The dynamic client is built in a static constructor, so the contract error surfaces wrapped in TypeInitializationException.
        var innermost = ex;
        while (innermost.InnerException is not null) innermost = innermost.InnerException;
        var ioe = Assert.IsType<InvalidOperationException>(innermost);
        Assert.Contains("must return void, Task, or ValueTask", ioe.Message);
    }
}

public interface IUnreliableTestHubReceiver
{
    void OnMove(int x);
}

public interface IUnreliableTestHub : IStreamingHub<IUnreliableTestHub, IUnreliableTestHubReceiver>
{
    [Unreliable] Task MoveAsync(int x);
    [Unreliable] ValueTask MoveWithNameAsync(int x, string name);
    [UnreliableUnordered] void Emote(int id);
    Task<int> QueryAsync(int x);
}

[Unreliable]
public interface IDefaultUnreliableTestHub : IStreamingHub<IDefaultUnreliableTestHub, IUnreliableTestHubReceiver>
{
    Task MoveAsync();
    [Reliable] Task<int> JoinAsync(string name);
}

public interface IInvalidUnreliableTestHub : IStreamingHub<IInvalidUnreliableTestHub, IUnreliableTestHubReceiver>
{
    [Unreliable] Task<int> BadAsync();
}
