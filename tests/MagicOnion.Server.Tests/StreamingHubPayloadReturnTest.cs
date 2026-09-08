using System.Buffers;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Threading.Channels;
using Cysharp.Runtime.Multicast.Remoting;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Hubs.Internal;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace MagicOnion.Server.Tests;

[Collection(nameof(StreamingHubPayloadReturnTestCollection))]
public class StreamingHubPayloadReturnTest
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ClientResult_ReturnsPayloadOnce(bool isError, bool registered)
    {
        using var harness = new Harness();
        var id = Guid.NewGuid();
        var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (registered)
        {
            harness.PendingTasks.Register(harness.PendingTasks.CreateTask("Receiver", 1, id, completion,
                TestContext.Current.CancellationToken, new MagicOnionRemoteSerializer(Options.Create(new MagicOnionOptions()))));
        }
        var payload = harness.Rent(BuildClientResult(id, isError));

        await harness.ProcessAsync(payload, TestContext.Current.CancellationToken);

        harness.AssertReturnedOnce(payload);
        if (registered)
        {
            if (isError)
            {
                var error = await Assert.ThrowsAsync<RpcException>(() => completion.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
                Assert.Equal(StatusCode.InvalidArgument, error.StatusCode);
                Assert.Contains("detail", error.Status.Detail);
            }
            else
            {
                // The result must remain usable after the input buffer has been returned.
                Assert.Equal("result", await completion.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            }
        }
    }

    [Fact]
    public async Task ClientResult_InvalidResultBody_ReturnsPayloadOnce()
    {
        using var harness = new Harness();
        var id = Guid.NewGuid();
        var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        harness.PendingTasks.Register(harness.PendingTasks.CreateTask("Receiver", 1, id, completion,
            TestContext.Current.CancellationToken, new MagicOnionRemoteSerializer(Options.Create(new MagicOnionOptions()))));
        var payload = harness.Rent(BuildClientResult(id, false, invalidBody: true));

        await Assert.ThrowsAsync<MessagePackSerializationException>(() => harness.ProcessAsync(payload, TestContext.Current.CancellationToken).AsTask());
        harness.AssertReturnedOnce(payload);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Heartbeat_ReturnsInputPayloadOnce(bool clientHeartbeat)
    {
        using var harness = new Harness();
        byte[] bytes = clientHeartbeat ? [0x94, 0x7e, 0x01, 0x02, 0xa3, 0x61, 0x62, 0x63] : [0x94, 0x7f, 0x01, 0xc0, 0xc0];
        var payload = harness.Rent(bytes);

        await harness.ProcessAsync(payload, TestContext.Current.CancellationToken);

        harness.AssertReturnedOnce(payload);
        if (clientHeartbeat)
        {
            var response = await harness.Response.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.Equal(new byte[] { 0x95, 0x7e, 0x01, 0x02, 0xc0, 0xc0, 0xa3, 0x61, 0x62, 0x63 }, response.Memory.ToArray());
            Assert.NotSame(payload, response);
            StreamingHubPayloadPool.Shared.Return(response);
            harness.AssertReturnedOnce(response);
            harness.AssertReturnedOnce(payload);
        }
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 0xc0 })]
    [InlineData(new byte[] { 0x91, 0xc0 })]
    [InlineData(new byte[] { 0x94, 0x02, 0xc0, 0xc0, 0xc0 })]
    [InlineData(new byte[] { 0x94, 0x00 })]
    [InlineData(new byte[] { 0x94, 0x01 })]
    [InlineData(new byte[] { 0x94, 0x7e })]
    [InlineData(new byte[] { 0x94, 0x7f })]
    [InlineData(new byte[] { 0x93 })]
    [InlineData(new byte[] { 0x92 })]
    public async Task MalformedMessage_ReturnsPayloadOnce(byte[] bytes)
    {
        using var harness = new Harness();
        var payload = harness.Rent(bytes);

        await Assert.ThrowsAnyAsync<Exception>(() => harness.ProcessAsync(payload, TestContext.Current.CancellationToken).AsTask());

        harness.AssertReturnedOnce(payload);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_ReturnsPayloadOnlyAfterConsumption(bool fireAndForget)
    {
        using var harness = new Harness();
        var bytes = BuildRequest(fireAndForget);
        var payload = harness.Rent(bytes);

        await harness.ProcessAsync(payload, TestContext.Current.CancellationToken);

        Assert.Equal(0, harness.ReturnCount(payload));
        Assert.Equal(bytes, payload.Memory.ToArray());
        Assert.True(harness.Requests.Reader.TryPeek(out var queued));
        Assert.Same(payload, queued.Payload);
        Assert.Equal(new byte[] { 0xc0 }, queued.Body.ToArray());

        harness.Requests.Writer.Complete();
        await harness.ConsumeAsync();

        harness.AssertReturnedOnce(payload);
        Assert.Equal(1, harness.InvocationCount);
        if (!fireAndForget)
        {
            var response = await harness.Response.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            StreamingHubPayloadPool.Shared.Return(response);
        }
        else
        {
            Assert.False(harness.Response.Task.IsCompleted);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_CanceledWhileQueueIsFull_ReturnsOnlyUnqueuedPayload(bool fireAndForget)
    {
        using var harness = new Harness();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var queuedPayload = harness.Rent(BuildRequest(fireAndForget));
        // A one-element channel makes the pending write deterministic, without timing or network dependencies.
        harness.Requests = Channel.CreateBounded<(StreamingHubPayload, UniqueHashDictionary<StreamingHubHandler>, int, int, ReadOnlyMemory<byte>, bool)>(1);
        await harness.ProcessAsync(queuedPayload, TestContext.Current.CancellationToken);
        var payload = harness.Rent(BuildRequest(fireAndForget));

        var pendingWrite = harness.ProcessAsync(payload, cancellation.Token).AsTask();
        Assert.False(pendingWrite.IsCompleted);
        Assert.Equal(0, harness.ReturnCount(payload));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pendingWrite.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        harness.AssertReturnedOnce(payload);
        Assert.Equal(0, harness.ReturnCount(queuedPayload));
        Assert.True(harness.Requests.Reader.TryRead(out var queued));
        Assert.Same(queuedPayload, queued.Payload);
        Assert.False(harness.Requests.Reader.TryRead(out _));
        StreamingHubPayloadPool.Shared.Return(queued.Payload);
        harness.AssertReturnedOnce(queuedPayload);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_WaitsForQueueThenTransfersPayload(bool fireAndForget)
    {
        using var harness = new Harness();
        harness.Requests = Channel.CreateBounded<(StreamingHubPayload, UniqueHashDictionary<StreamingHubHandler>, int, int, ReadOnlyMemory<byte>, bool)>(1);
        var firstPayload = harness.Rent(BuildRequest(fireAndForget));
        await harness.ProcessAsync(firstPayload, TestContext.Current.CancellationToken);
        var bytes = BuildRequest(fireAndForget);
        var payload = harness.Rent(bytes);

        var pendingWrite = harness.ProcessAsync(payload, TestContext.Current.CancellationToken).AsTask();
        Assert.False(pendingWrite.IsCompleted);
        Assert.Equal(0, harness.ReturnCount(payload));
        Assert.True(harness.Requests.Reader.TryRead(out var first));
        StreamingHubPayloadPool.Shared.Return(first.Payload);
        await pendingWrite.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(0, harness.ReturnCount(payload));
        Assert.Equal(bytes, payload.Memory.ToArray());
        Assert.True(harness.Requests.Reader.TryRead(out var second));
        Assert.Same(payload, second.Payload);
        StreamingHubPayloadPool.Shared.Return(second.Payload);
        harness.AssertReturnedOnce(payload);
        harness.AssertReturnedOnce(firstPayload);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_ClosedQueue_ReturnsPayloadOnce(bool fireAndForget)
    {
        using var harness = new Harness();
        harness.Requests.Writer.Complete();
        var payload = harness.Rent(BuildRequest(fireAndForget));

        await Assert.ThrowsAsync<ChannelClosedException>(() => harness.ProcessAsync(payload, TestContext.Current.CancellationToken).AsTask());

        harness.AssertReturnedOnce(payload);
    }

    static byte[] BuildRequest(bool fireAndForget)
        => fireAndForget ? [0x92, 0x01, 0xc0] : [0x93, 0x00, 0x01, 0xc0];

    static byte[] BuildClientResult(Guid id, bool isError, bool invalidBody = false)
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(4);
        writer.Write(isError ? 1 : 0);
        MessagePackSerializer.Serialize(ref writer, id);
        writer.Write(1);
        if (isError)
        {
            writer.WriteArrayHeader(3);
            writer.Write((int)StatusCode.InvalidArgument);
            writer.Write("detail");
            writer.Write("error");
        }
        else if (invalidBody)
        {
            writer.Write(123);
        }
        else
        {
            writer.Write("result");
        }
        writer.Flush();
        return buffer.WrittenMemory.ToArray();
    }

    sealed class Harness : IDisposable
    {
        static readonly Type HubBaseType = typeof(StreamingHubBase<ITestHub, ITestReceiver>);
        static readonly FieldInfo PoolField = typeof(StreamingHubPayloadPool).GetField("pool", BindingFlags.Instance | BindingFlags.NonPublic)!;
        readonly ObjectPool<StreamingHubPayloadCore> originalPool;
        readonly TrackingPool pool = new();
        readonly TestHub hub = new();
        readonly ServiceProvider services;
        readonly MagicOnionMetrics metrics;
        readonly StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> context;
        readonly StreamingHubHeartbeatHandle heartbeat;
        readonly Func<StreamingHubPayload, CancellationToken, ValueTask> process;
        readonly Func<CancellationToken, Task> consume;

        public RemoteClientResultPendingTaskRegistry PendingTasks { get; } = new(Timeout.InfiniteTimeSpan);
        public TaskCompletionSource<StreamingHubPayload> Response { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int InvocationCount => hub.InvocationCount;

        public Channel<(StreamingHubPayload Payload, UniqueHashDictionary<StreamingHubHandler> Handlers, int MethodId, int MessageId, ReadOnlyMemory<byte> Body, bool HasResponse)> Requests
        {
            get => (Channel<(StreamingHubPayload, UniqueHashDictionary<StreamingHubHandler>, int, int, ReadOnlyMemory<byte>, bool)>)HubBaseType.GetField("requests", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(hub)!;
            set => SetField("requests", value);
        }

        public Harness()
        {
            services = new ServiceCollection().AddMetrics().BuildServiceProvider();
            metrics = new MagicOnionMetrics(services.GetRequiredService<IMeterFactory>());
            var method = Substitute.For<IMagicOnionGrpcMethod>();
            method.MethodType.Returns(MethodType.DuplexStreaming);
            method.ServiceName.Returns("PayloadReturnTestHub");
            var responseStream = new ResponseStream(Response);
            context = new StreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hub, method,
                Substitute.For<ServerCallContext>(), MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null),
                metrics, NullLogger.Instance, services, null, responseStream);
            ((IServiceBase)hub).Context = context;
            ((IServiceBase)hub).Metrics = metrics;
            heartbeat = NopStreamingHubHeartbeatManager.Instance.Register(context);
            SetField("heartbeatHandle", heartbeat);
            SetField("remoteClientResultPendingTasks", PendingTasks);
            SetField("timeProvider", TimeProvider.System);
            var hubMethod = new MagicOnionStreamingHubMethod<TestHub, Nil, int>("PayloadReturnTestHub", nameof(TestHub.InvokeAsync),
                static (instance, _, _) => instance.InvokeAsync());
            var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), services);
            SetField("handlers", new UniqueHashDictionary<StreamingHubHandler>((1, handler)));
            process = HubBaseType.GetMethod("ProcessMessageAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
                .CreateDelegate<Func<StreamingHubPayload, CancellationToken, ValueTask>>(hub);
            consume = HubBaseType.GetMethod("ConsumeRequestQueueAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
                .CreateDelegate<Func<CancellationToken, Task>>(hub);

            // Keep instrumentation in the tests. This collection cannot overlap other pool users.
            originalPool = (ObjectPool<StreamingHubPayloadCore>)PoolField.GetValue(StreamingHubPayloadPool.Shared)!;
            PoolField.SetValue(StreamingHubPayloadPool.Shared, pool);
        }

        public StreamingHubPayload Rent(byte[] bytes) => StreamingHubPayloadPool.Shared.RentOrCreate(bytes.AsSpan());
        public ValueTask ProcessAsync(StreamingHubPayload payload, CancellationToken cancellationToken) => process(payload, cancellationToken);
        public Task ConsumeAsync() => consume(TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        public int ReturnCount(StreamingHubPayload payload)
        {
#if DEBUG
            return pool.ReturnCount(payload.Core);
#else
            return pool.ReturnCount(payload);
#endif
        }

        public void AssertReturnedOnce(StreamingHubPayload payload)
        {
            Assert.Equal(1, ReturnCount(payload));
            Assert.Throws<InvalidOperationException>(() => payload.Memory);
        }

        void SetField(string name, object value) => HubBaseType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(hub, value);

        public void Dispose()
        {
            context.CompleteStreamingHub();
            heartbeat.Dispose();
            PendingTasks.Dispose();
            PoolField.SetValue(StreamingHubPayloadPool.Shared, originalPool);
            pool.Dispose();
            metrics.Dispose();
            services.Dispose();
        }
    }

    sealed class ResponseStream(TaskCompletionSource<StreamingHubPayload> response) : IServerStreamWriter<StreamingHubPayload>
    {
        public WriteOptions WriteOptions { get; set; }

        public Task WriteAsync(StreamingHubPayload message)
        {
            response.TrySetResult(message);
            return Task.CompletedTask;
        }
    }

    sealed class TrackingPool : ObjectPool<StreamingHubPayloadCore>, IDisposable
    {
        readonly Dictionary<StreamingHubPayloadCore, int> returns = new();

        public override StreamingHubPayloadCore Get()
        {
#if DEBUG
            var core = new StreamingHubPayloadCore();
#else
            var core = new StreamingHubPayload();
#endif
            lock (returns) returns.Add(core, 0);
            return core;
        }

        public override void Return(StreamingHubPayloadCore obj)
        {
            lock (returns) returns[obj]++;
            obj.Uninitialize();
        }

        public int ReturnCount(StreamingHubPayloadCore obj)
        {
            lock (returns) return returns[obj];
        }

        public void Dispose()
        {
            // Also release leaked buffers if a regression causes an assertion to fail.
            lock (returns)
            {
                foreach (var (core, count) in returns)
                {
                    if (count == 0) core.Uninitialize();
                }
            }
        }
    }

    public interface ITestHub : IStreamingHub<ITestHub, ITestReceiver>
    {
        [MethodId(1)]
        Task<int> InvokeAsync();
    }
    public interface ITestReceiver;
    sealed class TestHub : StreamingHubBase<ITestHub, ITestReceiver>, ITestHub
    {
        public int InvocationCount { get; private set; }

        public Task<int> InvokeAsync()
        {
            InvocationCount++;
            return Task.FromResult(123);
        }
    }
}

// The tests temporarily replace the shared pool's storage to count returns without changing production APIs.
[CollectionDefinition(nameof(StreamingHubPayloadReturnTestCollection), DisableParallelization = true)]
public class StreamingHubPayloadReturnTestCollection;
