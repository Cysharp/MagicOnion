using MagicOnion.Internal;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Hubs.Internal;
using MagicOnion.Server.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests;

/// <summary>Verifies that hub method metadata carries the transport mode resolved from the shared contract interface.</summary>
public class StreamingHubTransportModeMetadataTest
{
    [Fact]
    public void Factory_ResolvesModeFromContractInterface()
    {
        var type = typeof(ModeHub);

        var move = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, type.GetMethod(nameof(ModeHub.MoveAsync))!);
        var query = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, type.GetMethod(nameof(ModeHub.QueryAsync))!);
        var emote = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, type.GetMethod(nameof(ModeHub.EmoteAsync))!);

        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, move.TransportMode);
        Assert.Equal(StreamingHubTransportMode.Reliable, query.TransportMode);
        Assert.Equal(StreamingHubTransportMode.UnreliableUnordered, emote.TransportMode);
    }

    [Fact]
    public void Factory_InterfaceDefault_WithReliableOverride()
    {
        var type = typeof(DefaultUnreliableModeHub);

        var move = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, type.GetMethod(nameof(DefaultUnreliableModeHub.MoveAsync))!);
        var join = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, type.GetMethod(nameof(DefaultUnreliableModeHub.JoinAsync))!);

        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, move.TransportMode);
        Assert.Equal(StreamingHubTransportMode.Reliable, join.TransportMode);
    }

    [Fact]
    public void PrecomputedCreate_MatchesFactory()
    {
        // The precomputed path (generated providers / NativeAOT) must resolve the same mode as runtime discovery.
        var expected = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(typeof(ModeHub), typeof(ModeHub).GetMethod(nameof(ModeHub.MoveAsync))!);
        var actual = StreamingHubMethodHandlerMetadata.Create(
            expected.MethodId,
            typeof(ModeHub),
            typeof(IModeHub).GetMethod(nameof(IModeHub.MoveAsync))!,
            typeof(ModeHub).GetMethod(nameof(ModeHub.MoveAsync))!,
            null,
            typeof(int),
            typeof(IModeHub));

        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, actual.TransportMode);
        Assert.Equal(expected.TransportMode, actual.TransportMode);
    }

    [Fact]
    public void Handler_ExposesTransportMode()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var hubMethod = new MagicOnionStreamingHubMethod<ModeHub, int>(nameof(ModeHub), nameof(ModeHub.MoveAsync), static (hub, _, request) => hub.MoveAsync(request));

        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);

        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, handler.TransportMode);
        Assert.Equal(FNV1A32.GetHashCode(nameof(IModeHub.MoveAsync)), handler.MethodId);
    }

    [Fact]
    public void Invalid_UnreliableMethodWithResult_Throws()
    {
        var type = typeof(InvalidResultModeHub);

        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, type.GetMethod(nameof(InvalidResultModeHub.BadAsync))!));

        var ioe = Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("must return void, Task, or ValueTask", ioe.Message);
    }

    [Fact]
    public void Invalid_TransportAttributeOnImplementation_Throws()
    {
        var type = typeof(ImplAttributeModeHub);

        var ex = Record.Exception(() => MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(type, type.GetMethod(nameof(ImplAttributeModeHub.MoveAsync))!));

        var ioe = Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("Transport attributes must be annotated to a hub interface", ioe.Message);
    }

    // Fixtures

    public interface IModeHubReceiver
    {
        void OnMove(int x);
    }

    public interface IModeHub : IStreamingHub<IModeHub, IModeHubReceiver>
    {
        [Unreliable] Task MoveAsync(int x);
        Task<int> QueryAsync();
        [UnreliableUnordered] ValueTask EmoteAsync();
    }

    public class ModeHub : IModeHub
    {
        public IModeHub FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();

        public Task MoveAsync(int x) => Task.CompletedTask;
        public Task<int> QueryAsync() => Task.FromResult(0);
        public ValueTask EmoteAsync() => default;
    }

    [Unreliable]
    public interface IDefaultUnreliableModeHub : IStreamingHub<IDefaultUnreliableModeHub, IModeHubReceiver>
    {
        Task MoveAsync();
        [Reliable] Task<int> JoinAsync();
    }

    public class DefaultUnreliableModeHub : IDefaultUnreliableModeHub
    {
        public IDefaultUnreliableModeHub FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();

        public Task MoveAsync() => Task.CompletedTask;
        public Task<int> JoinAsync() => Task.FromResult(0);
    }

    public interface IInvalidResultModeHub : IStreamingHub<IInvalidResultModeHub, IModeHubReceiver>
    {
        [Unreliable] Task<int> BadAsync();
    }

    public class InvalidResultModeHub : IInvalidResultModeHub
    {
        public IInvalidResultModeHub FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();

        public Task<int> BadAsync() => Task.FromResult(0);
    }

    public interface IImplAttributeModeHub : IStreamingHub<IImplAttributeModeHub, IModeHubReceiver>
    {
        Task MoveAsync();
    }

    public class ImplAttributeModeHub : IImplAttributeModeHub
    {
        public IImplAttributeModeHub FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();

        [Unreliable]
        public Task MoveAsync() => Task.CompletedTask;
    }
}
