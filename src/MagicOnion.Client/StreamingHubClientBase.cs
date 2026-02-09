using Grpc.Core;
using MagicOnion.Client.Internal;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MagicOnion.Serialization;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Channels;

namespace MagicOnion.Client;

public class StreamingHubClientOptions
{
    public string? Host { get; }
    public CallOptions CallOptions { get; }
    public IMagicOnionSerializerProvider SerializerProvider { get; }
    public IMagicOnionClientLogger Logger { get; }

    public TimeSpan? ClientHeartbeatInterval { get; }
    public TimeSpan? ClientHeartbeatTimeout { get; }
    public Action<ServerHeartbeatEvent>? OnServerHeartbeatReceived { get; }
    public Action<ClientHeartbeatEvent>? OnClientHeartbeatResponseReceived { get; }
    public TimeProvider? TimeProvider { get; }
    public DnsEndPoint? DataChannelEndpoint { get; }

    public StreamingHubClientOptions(string? host, CallOptions callOptions, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger)
        : this(host, callOptions, serializerProvider, logger, default, default, default, default, default, default)
    {
    }

    public StreamingHubClientOptions(
        string? host,
        CallOptions callOptions,
        IMagicOnionSerializerProvider serializerProvider,
        IMagicOnionClientLogger logger,
        TimeSpan? clientHeartbeatInterval,
        TimeSpan? clientHeartbeatTimeout,
        Action<ServerHeartbeatEvent>? onServerHeartbeatReceived,
        Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived,
        TimeProvider? timeProvider,
        DnsEndPoint? dataChannelEndpoint)
    {
        Host = host;
        CallOptions = callOptions;
        SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ClientHeartbeatInterval = clientHeartbeatInterval;
        ClientHeartbeatTimeout = clientHeartbeatTimeout;
        OnServerHeartbeatReceived = onServerHeartbeatReceived;
        OnClientHeartbeatResponseReceived = onClientHeartbeatResponseReceived;
        TimeProvider = timeProvider;
        DataChannelEndpoint = dataChannelEndpoint;
    }

    public static StreamingHubClientOptions CreateWithDefault(string? host = default, CallOptions callOptions = default, IMagicOnionSerializerProvider? serializerProvider = default, IMagicOnionClientLogger? logger = default)
        => new(host, callOptions, serializerProvider ?? MagicOnionSerializerProvider.Default, logger ?? NullMagicOnionClientLogger.Instance);

    public StreamingHubClientOptions WithHost(string? host)
        => new(host, CallOptions, SerializerProvider, Logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );
    public StreamingHubClientOptions WithCallOptions(CallOptions callOptions)
        => new(Host, callOptions, SerializerProvider, Logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );
    public StreamingHubClientOptions WithSerializerProvider(IMagicOnionSerializerProvider serializerProvider)
        => new(
            Host, CallOptions, serializerProvider, Logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );
    public StreamingHubClientOptions WithLogger(IMagicOnionClientLogger logger)
        => new(Host, CallOptions, SerializerProvider, logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );

    /// <summary>
    /// Sets a heartbeat interval. If a value is <see keyword="null"/>, the heartbeat from the client is disabled.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatInterval(TimeSpan? interval)
        => new(Host, CallOptions, SerializerProvider, Logger
            , interval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );

    /// <summary>
    /// Sets a heartbeat timeout period. If a value is <see keyword="null"/>, the client does not time out.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatTimeout(TimeSpan? timeout)
        => new(Host, CallOptions, SerializerProvider, Logger
            , ClientHeartbeatInterval, timeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );

    /// <summary>
    /// Sets a heartbeat callback. If additional metadata is provided by the server in the heartbeat message, this metadata is provided as an argument.
    /// </summary>
    /// <param name="onServerHeartbeatReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithServerHeartbeatReceived(Action<ServerHeartbeatEvent>? onServerHeartbeatReceived)
        => new(Host, CallOptions, SerializerProvider, Logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, onServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );

    /// <summary>
    /// Sets a client heartbeat response callback.
    /// </summary>
    /// <param name="onClientHeartbeatResponseReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatResponseReceived(Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived)
        => new(Host, CallOptions, SerializerProvider, Logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, onClientHeartbeatResponseReceived
            , TimeProvider, DataChannelEndpoint
        );

    /// <summary>
    /// Sets a <see cref="TimeProvider"/>
    /// </summary>
    /// <param name="timeProvider"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithTimeProvider(TimeProvider timeProvider)
        => new(Host, CallOptions, SerializerProvider, Logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , timeProvider, DataChannelEndpoint
        );

    /// <summary>
    /// Sets a <see cref="DataChannelEndpoint"/>
    /// </summary>
    /// <param name="dataChannelEndpoint"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithDataChannelEndpoint(DnsEndPoint dataChannelEndpoint)
        => new(Host, CallOptions, SerializerProvider, Logger
            , ClientHeartbeatInterval, ClientHeartbeatTimeout, OnServerHeartbeatReceived, OnClientHeartbeatResponseReceived
            , TimeProvider, dataChannelEndpoint
        );
}

public abstract partial class StreamingHubClientBase<TStreamingHub, TReceiver> : IStreamingHubClient
    where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
{
#pragma warning disable IDE1006 // Naming Styles
    const string StreamingHubVersionHeaderKey = "x-magiconion-streaminghub-version";
    const string StreamingHubVersionHeaderValue = "2";
    static readonly TimeSpan CleanupSubscriptionWait = TimeSpan.FromMilliseconds(100);
#pragma warning restore IDE1006 // Naming Styles

    readonly CallInvoker callInvoker;
    readonly StreamingHubClientOptions options;
    readonly IMagicOnionClientLogger logger;
    readonly IMagicOnionSerializer messageSerializer;
    readonly Method<StreamingHubPayload, StreamingHubPayload> duplexStreamingConnectMethod;
    // {messageId, TaskCompletionSource}
    readonly Dictionary<int, IStreamingHubResponseTaskSource> responseFutures = new();
    readonly TaskCompletionSource<DisconnectionReason> waitForDisconnect = new();
    readonly CancellationTokenSource subscriptionCts = new();
    readonly Dictionary<int, SendOrPostCallback> postCallbackCache = new();

    int messageIdSequence = 0;
    bool disposed;
    bool disconnected;
    int cleanupSentinel = 0; // 0 = false, 1 = true

    readonly Channel<StreamingHubPayload> writerQueue = Channel.CreateUnbounded<StreamingHubPayload>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = false });
    Task? writerTask;
    IClientStreamWriter<StreamingHubPayload>? writer;
    IAsyncStreamReader<StreamingHubPayload>? reader;
    ClientDataChannel? dataChannel;

    StreamingHubClientHeartbeatManager? heartbeatManager;
    Task? subscription;

    protected readonly TReceiver receiver;

    protected StreamingHubClientBase(string serviceName, TReceiver receiver, CallInvoker callInvoker, StreamingHubClientOptions options)
    {
        this.callInvoker = callInvoker;
        this.receiver = receiver;
        this.options = options;
        this.logger = options.Logger;
        this.duplexStreamingConnectMethod = CreateConnectMethod(serviceName);
        this.messageSerializer = options.SerializerProvider.Create(MethodType.DuplexStreaming, null);
    }

    // call immediately after create.
    internal async Task __ConnectAndSubscribeAsync(CancellationToken connectAndSubscribeCancellationToken)
    {
        var syncContext = SynchronizationContext.Current; // capture SynchronizationContext.

        var requestHeaders = new Metadata();
        if (options.CallOptions.Headers is not null)
        {
            foreach (var requestHeader in options.CallOptions.Headers)
            {
                requestHeaders.Add(requestHeader);
            }
        }
        if (options.DataChannelEndpoint != null)
        {
            requestHeaders.Add("x-magiconion-streaminghub-datachannel", "enabled;encryption-mode=none");
        }

        var callOptions = options.CallOptions.WithHeaders(requestHeaders);
        var callResult = callInvoker.AsyncDuplexStreamingCall(duplexStreamingConnectMethod, options.Host, callOptions);

        this.writer = callResult.RequestStream;
        this.reader = callResult.ResponseStream;

        var cancelTcs = new TaskCompletionSource<bool>();
        using var cancelRegistration = connectAndSubscribeCancellationToken.Register(static cancelTcs =>
        {
            ((TaskCompletionSource<bool>)cancelTcs!).SetCanceled();
        }, cancelTcs);

        // Establish StreamingHub connection between the client and the server.
        Metadata.Entry? messageVersion;
        Metadata responseHeaders;
        try
        {
            // The client can read the response headers before any StreamingHub's message.
            // MagicOnion.Server v4.0.x or before doesn't send any response headers. The client is incompatible with that versions.
            // NOTE: Grpc.Net:
            //           If the channel can not be connected, ResponseHeadersAsync will throw an exception.
            //       C-core:
            //           If the channel can not be connected, ResponseHeadersAsync will **return** an empty metadata.

            // ResponseHeadersAsync does not accept CancellationToken, so use TaskCompletionSource to wait for CancellationToken.
            var responseHeadersTask = callResult.ResponseHeadersAsync;
            var completedTask = await Task.WhenAny(responseHeadersTask, cancelTcs.Task).ConfigureAwait(false);

            // If cancellation is requested before the connection is established, the connection will be maintained, so callResult must be disposed.
            // After this, the only thing that passes connectAndSubscribeCancellationToken is MoveNext of Stream, so it doesn't need to be disposed.
            if (completedTask == cancelTcs.Task)
            {
                try
                {
                    connectAndSubscribeCancellationToken.ThrowIfCancellationRequested();
                }
                finally
                {
                    callResult.Dispose();
                }
                return;
            }

            responseHeaders = await responseHeadersTask.ConfigureAwait(false);
            messageVersion = responseHeaders.FirstOrDefault(x => x.Key == StreamingHubVersionHeaderKey);

            // Check message version of StreamingHub.
            if (messageVersion != null && messageVersion.Value != StreamingHubVersionHeaderValue)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"The message version of StreamingHub mismatch between the client and the server. (ServerVersion={messageVersion?.Value}; Expected={StreamingHubVersionHeaderValue})"));
            }
        }
        catch (RpcException e)
        {
            throw new RpcException(e.Status, $"Failed to connect to StreamingHub '{duplexStreamingConnectMethod.ServiceName}'. ({e.Status})");
        }

        // Try to establish DataChannel
        if (options.DataChannelEndpoint != null && responseHeaders.FirstOrDefault(x => x.Key == "x-magiconion-streaminghub-datachannel-session-id") is {} dataChannelSessionId)
        {
            var sessionId = ulong.Parse(dataChannelSessionId.Value);
            dataChannel = new ClientDataChannel(options.DataChannelEndpoint, sessionId);
            try
            {
                await dataChannel.ConnectAsync(subscriptionCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // TODO: Error logging / retry?
                dataChannel.Dispose();
                dataChannel = null;
            }
        }

        // Set up the Heartbeat Manager
        heartbeatManager = new StreamingHubClientHeartbeatManager(
            writerQueue.Writer,
            options.ClientHeartbeatInterval ?? TimeSpan.Zero /* Disable */,
            options.ClientHeartbeatTimeout ?? Timeout.InfiniteTimeSpan,
            options.OnServerHeartbeatReceived,
            options.OnClientHeartbeatResponseReceived,
            syncContext,
            options.TimeProvider ?? TimeProvider.System
        );

        // Activate the Heartbeat Manager if enabled in the options.
        var subscriptionToken = subscriptionCts.Token;
        if (options.ClientHeartbeatInterval is { } heartbeatInterval && heartbeatInterval > TimeSpan.Zero)
        {
            heartbeatManager.StartClientHeartbeatLoop();
            subscriptionToken = CancellationTokenSource.CreateLinkedTokenSource(heartbeatManager.TimeoutToken, subscriptionCts.Token).Token;
        }

        var firstMoveNextTask = reader.MoveNext(subscriptionToken);
        if (firstMoveNextTask.IsFaulted || messageVersion == null)
        {
            // NOTE: Grpc.Net:
            //           If an error is returned from `StreamingHub.Connect` method on a server-side,
            //           ResponseStream.MoveNext synchronously returns a task that is `IsFaulted = true`.
            //           `ConnectAsync` method should throw an exception here immediately.
            await firstMoveNextTask.ConfigureAwait(false);

            throw new RpcException(new Status(StatusCode.Internal, $"The request started successfully (StatusCode = OK), but the StreamingHub client has failed to negotiate with the server. ServerVersion is missing."));
        }

        this.subscription = StartSubscribe(syncContext, firstMoveNextTask, subscriptionToken);
    }

    static Method<StreamingHubPayload, StreamingHubPayload> CreateConnectMethod(string serviceName)
        => new (MethodType.DuplexStreaming, serviceName, "Connect", MagicOnionMarshallers.StreamingHubMarshaller, MagicOnionMarshallers.StreamingHubMarshaller);

    [MemberNotNull(nameof(reader))]
    [MemberNotNull(nameof(writer))]
    [MemberNotNull(nameof(heartbeatManager))]
    void EnsureConnected()
    {
        if (reader is null || writer is null || heartbeatManager is null)
        {
            throw new InvalidOperationException("The client must be connected to the server before subscribing.");
        }
    }

    readonly Channel<StreamingHubPayload> receivedDataQueue = Channel.CreateUnbounded<StreamingHubPayload>();

    async Task RunDataConsumerLoopAsync(SynchronizationContext? syncContext, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!await receivedDataQueue.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                break;
            }

            while (receivedDataQueue.Reader.TryRead(out var payload))
            {
                ConsumeData(syncContext, payload);
            }
        }
    }

    async Task RunDataChannelReaderLoopAsync(CancellationToken cancellationToken)
    {
        if (dataChannel is null) throw new InvalidOperationException();
        while (!cancellationToken.IsCancellationRequested && await dataChannel.DataReader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (dataChannel.DataReader.TryRead(out var payload))
            {
                receivedDataQueue.Writer.TryWrite(payload);
            }
        }
    }

    async Task StartSubscribe(SynchronizationContext? syncContext, Task<bool> firstMoveNext, CancellationToken subscriptionToken)
    {
        EnsureConnected();

        var disconnectionReason = new DisconnectionReason(DisconnectionType.CompletedNormally, null);
        writerTask = RunWriterLoopAsync(subscriptionToken);

        RunDataConsumerLoopAsync(syncContext, subscriptionToken);

        if (dataChannel is not null)
        {
            RunDataChannelReaderLoopAsync(subscriptionToken);
        }

        var reader = this.reader;
        try
        {
            var moveNext = firstMoveNext;
            while (await moveNext.ConfigureAwait(false)) // avoid Post to SyncContext(it losts one-frame per operation)
            {
                try
                {
                    receivedDataQueue.Writer.TryWrite(reader.Current);
                }
                catch (Exception ex)
                {
                    const string msg = "An error occurred when consuming a received message, but the subscription is still alive.";
                    // log post on main thread.
                    if (syncContext != null)
                    {
                        syncContext.Post(s => logger.Error((Exception)s!, msg), ex);
                    }
                    else
                    {
                        logger.Error(ex, msg);
                    }
                }

                moveNext = reader.MoveNext(subscriptionToken);
            }
        }
        catch (Exception ex)
        {
            // When terminating by Heartbeat or DisposeAsync, a RpcException with a Status of Canceled is thrown.
            // If `ex.InnerException` is OperationCanceledException` and `subscriptionToken.IsCancellationRequested` is true, it is treated as a normal cancellation.
            if ((ex is OperationCanceledException oce) ||
                (ex is RpcException { InnerException: OperationCanceledException } && subscriptionToken.IsCancellationRequested))
            {
                if (heartbeatManager.TimeoutToken.IsCancellationRequested)
                {
                    disconnectionReason = new DisconnectionReason(DisconnectionType.TimedOut, ex);
                }
                return;
            }

            const string msg = "An error occurred while subscribing to messages.";
            // log post on main thread.
            if (syncContext != null)
            {
                syncContext.Post(s => logger.Error((Exception)s!, msg), ex);
            }
            else
            {
                logger.Error(ex, msg);
            }

            disconnectionReason = new DisconnectionReason(DisconnectionType.Faulted, ex);
        }
        finally
        {
            receivedDataQueue.Writer.TryComplete();
            disconnected = true;

            try
            {
#if !UNITY_WEBGL
                // set syncContext before await
                // NOTE: If restore SynchronizationContext in WebGL environment, a continuation will not be executed inline and will be stuck.
                if (syncContext != null && SynchronizationContext.Current == null)
                {
                    SynchronizationContext.SetSynchronizationContext(syncContext);
                }
#endif
                await heartbeatManager.DisposeAsync().ConfigureAwait(false);
                await CleanupAsync(false).ConfigureAwait(false);
            }
            finally
            {
                waitForDisconnect.TrySetResult(disconnectionReason);
            }
        }
    }

    void ConsumeData(SynchronizationContext? syncContext, StreamingHubPayload payload)
    {
        var messageReader = new StreamingHubClientMessageReader(payload.Memory);
        switch (messageReader.ReadMessageType())
        {
            case StreamingHubMessageType.Broadcast:
                ProcessBroadcast(syncContext, payload, ref messageReader);
                break;
            case StreamingHubMessageType.Response:
                ProcessResponse(syncContext, payload, ref messageReader);
                break;
            case StreamingHubMessageType.ResponseWithError:
                ProcessResponseWithError(syncContext, payload, ref messageReader);
                break;
            case StreamingHubMessageType.ClientResultRequest:
                ProcessClientResultRequest(syncContext, payload, ref messageReader);
                break;
            case StreamingHubMessageType.ServerHeartbeat:
                heartbeatManager!.ProcessServerHeartbeat(payload);
                break;
            case StreamingHubMessageType.ClientHeartbeatResponse:
                heartbeatManager!.ProcessClientHeartbeatResponse(payload);
                break;
        }
    }

    void ProcessBroadcast(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
    {
        if (syncContext is null)
        {
            var message = messageReader.ReadBroadcastMessage();
            OnBroadcastEvent(message.MethodId, message.Body);
            StreamingHubPayloadPool.Shared.Return(payload);
        }
        else
        {
            var (methodId, consumed) = messageReader.ReadBroadcastMessageMethodId();
            if (!postCallbackCache.TryGetValue(methodId, out var postCallback))
            {
                // Create and cache a callback delegate capturing `this` and the header size.
                postCallback = postCallbackCache[methodId] = CreateBroadcastCallback(methodId, consumed);
            }
            syncContext.Post(postCallback, payload);
        }
    }

    SendOrPostCallback CreateBroadcastCallback(int methodId, int consumed)
    {
        return (s) =>
        {
            var p = (StreamingHubPayload)s!;
            this.OnBroadcastEvent(methodId, p.Memory.Slice(consumed));
            StreamingHubPayloadPool.Shared.Return(p);
        };
    }

    void ProcessResponse(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
    {
        var message = messageReader.ReadResponseMessage();

        IStreamingHubResponseTaskSource? future;
        lock (responseFutures)
        {
            if (!responseFutures.Remove(message.MessageId, out future))
            {
                return;
            }
        }

        try
        {
            OnResponseEvent(message.MethodId, future, message.Body);
            StreamingHubPayloadPool.Shared.Return(payload);
        }
        catch (Exception ex)
        {
            if (!future.TrySetException(ex))
            {
                throw;
            }
        }
    }

    void ProcessResponseWithError(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
    {
        var message = messageReader.ReadResponseWithErrorMessage();

        IStreamingHubResponseTaskSource? future;
        lock (responseFutures)
        {
            if (!responseFutures.Remove(message.MessageId, out future))
            {
                return;
            }
        }

        RpcException ex;
        if (string.IsNullOrWhiteSpace(message.Error))
        {
            ex = new RpcException(new Status((StatusCode)message.StatusCode, message.Detail ?? string.Empty));
        }
        else
        {
            ex = new RpcException(new Status((StatusCode)message.StatusCode, message.Detail ?? string.Empty), message.Detail + Environment.NewLine + message.Error);
        }

        future.TrySetException(ex);
        StreamingHubPayloadPool.Shared.Return(payload);
    }

    void ProcessClientResultRequest(SynchronizationContext? syncContext, StreamingHubPayload payload, ref StreamingHubClientMessageReader messageReader)
    {
        var message = messageReader.ReadClientResultRequestMessage();
        if (syncContext is null)
        {
            OnClientResultEvent(message.MethodId, message.ClientResultRequestMessageId, message.Body);
            StreamingHubPayloadPool.Shared.Return(payload);
        }
        else
        {
            var tuple = Tuple.Create(this, message.MethodId, message.ClientResultRequestMessageId, message.Body, payload);
            syncContext.Post(static state =>
            {
                var t = (Tuple<StreamingHubClientBase<TStreamingHub, TReceiver>, int, Guid, ReadOnlyMemory<byte>, StreamingHubPayload>)state!;
                t.Item1.OnClientResultEvent(t.Item2, t.Item3, t.Item4);
                StreamingHubPayloadPool.Shared.Return(t.Item5);
            }, tuple);
        }
    }

    async Task RunWriterLoopAsync(CancellationToken cancellationToken)
    {
        EnsureConnected();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await writerQueue.Reader.WaitToReadAsync(default).ConfigureAwait(false))
                {
                    while (writerQueue.Reader.TryRead(out var payload))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await writer.WriteAsync(payload).ConfigureAwait(false);
                    }
                }
                else
                {
                    break; // The writer has completed.
                }
            }
        }
        catch { /* Ignore */ }
    }


    void ThrowIfDisconnected()
    {
        if (disconnected)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, $"The StreamingHubClient has already been disconnected from the server."));
        }
    }

    void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException("StreamingHubClient", $"The StreamingHubClient has already been disconnected from the server.");
        }
    }

    public Task WaitForDisconnect()
        => ((IStreamingHubClient)this).WaitForDisconnectAsync();

    Task<DisconnectionReason> IStreamingHubClient.WaitForDisconnectAsync()
        => waitForDisconnect.Task;

    public async Task DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        await CleanupAsync(true).ConfigureAwait(false);
    }

    async ValueTask CleanupAsync(bool waitSubscription)
    {
        if (Interlocked.CompareExchange(ref cleanupSentinel, 1, 0) != 0)
        {
            return;
        }

        if (writer == null) return;
        try
        {
            writerQueue.Writer.Complete();
            await writer.CompleteAsync().ConfigureAwait(false);
        }
        catch { } // ignore error?
        finally
        {
            // When it is necessary to wait for subscription (message reading) completion, add a small delay before cancellation.
            // This prevents throwing an IOException (non-error) on the server side when the stream is reset if `Cancel` is performed immediately while message reading is incomplete.
            subscriptionCts.CancelAfter(CleanupSubscriptionWait);
            try
            {
                if (waitSubscription)
                {
                    if (subscription != null)
                    {
                        await subscription.ConfigureAwait(false);
                    }
                }

                // cleanup completion
                List<Exception>? aggregateException = null;
                foreach (var item in responseFutures)
                {
                    try
                    {
                        item.Value.TrySetCanceled("A task was canceled because the client has disposed.");
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is OperationCanceledException))
                        {
                            aggregateException ??= new List<Exception>();
                            aggregateException.Add(ex);
                        }
                    }
                }
                if (aggregateException != null)
                {
                    throw new AggregateException(aggregateException);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    throw;
                }
            }
        }
        subscriptionCts.Dispose();
    }

    StreamingHubPayload BuildRequestMessage<T>(int methodId, T message)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteRequestMessageVoid(buffer, methodId, message, messageSerializer);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }

    StreamingHubPayload BuildRequestMessage<T>(int methodId, int messageId, T message)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteRequestMessage(buffer, methodId, messageId, message, messageSerializer);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }

    StreamingHubPayload BuildClientResultResponseMessage<T>(int methodId, Guid messageId, T response)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteClientResultResponseMessage(buffer, methodId, messageId, response, messageSerializer);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }

    StreamingHubPayload BuildClientResultResponseMessageForError(int methodId, Guid messageId, int statusCode, string detail, Exception? ex)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteClientResultResponseMessageForError(buffer, methodId, messageId, statusCode, detail, ex, messageSerializer);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }
}
