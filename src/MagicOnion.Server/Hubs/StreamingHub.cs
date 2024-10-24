using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Cysharp.Runtime.Multicast.Remoting;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Features;
using MagicOnion.Server.Features.Internal;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Hubs;

public abstract class StreamingHubBase<THubInterface, TReceiver> : ServiceBase<THubInterface>, IStreamingHub<THubInterface, TReceiver>, IStreamingHubBase
    where THubInterface : IStreamingHub<THubInterface, TReceiver>
{
    IStreamingHubFeature streamingHubFeature = default!;
    IRemoteClientResultPendingTaskRegistry remoteClientResultPendingTasks = default!;
    StreamingHubHeartbeatHandle heartbeatHandle = default!;
    TimeProvider timeProvider = default!;
    bool isReturnExceptionStackTraceInErrorDetail = false;
    UniqueHashDictionary<StreamingHubHandler> handlers = default!;

    protected static readonly Task<Nil> NilTask = Task.FromResult(Nil.Default);
    protected static readonly ValueTask CompletedTask = new ValueTask();

    static readonly Metadata ResponseHeaders = new Metadata()
    {
        { "x-magiconion-streaminghub-version", "2" },
    };

    // response:  [messageId, methodId, response]
    // HACK: If the ID of the message is `-1`, the client will ignore the message.
    static ReadOnlySpan<byte> MarkerResponseBytes => [0x93, 0xff, 0x00, 0x0c]; // MsgPack: [-1, 0, nil]

    readonly record struct StreamingHubMethodRequest(StreamingHubPayload Payload, int MethodId, int MessageId, ReadOnlyMemory<byte> Body, bool HasResponse);

    readonly Channel<StreamingHubMethodRequest> requests = Channel.CreateBounded<StreamingHubMethodRequest>(new BoundedChannelOptions(capacity: 10)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        });

    public HubGroupRepository<TReceiver> Group { get; private set; } = default!;
    public TReceiver Client { get; private set; } = default!;

    internal StreamingServiceContext<StreamingHubPayload, StreamingHubPayload> StreamingServiceContext
        => (StreamingServiceContext<StreamingHubPayload, StreamingHubPayload>)Context;

    protected Guid ConnectionId
        => Context.ContextId;
    
    /// <summary>
    /// Called before connect, instead of constructor.
    /// </summary>
    protected virtual ValueTask OnConnecting()
    {
        return CompletedTask;
    }

    /// <summary>
    /// Called after connect (headers and marker have been sent).
    /// Allow the server send message to the client or broadcast to group.
    /// </summary>
    protected virtual ValueTask OnConnected()
    {
        return CompletedTask;
    }

    /// <summary>
    /// Called after disconnect.
    /// </summary>
    protected virtual ValueTask OnDisconnected()
    {
        return CompletedTask;
    }

    async Task<DuplexStreamingResult<StreamingHubPayload, StreamingHubPayload>> IStreamingHubBase.Connect()
    {
        Metrics.StreamingHubConnectionIncrement(Context.Metrics, Context.ServiceName);

        var streamingContext = GetDuplexStreamingContext<StreamingHubPayload, StreamingHubPayload>();
        var serviceProvider = streamingContext.ServiceContext.ServiceProvider;

        var features = this.Context.CallContext.GetHttpContext().Features;
        streamingHubFeature = features.GetRequiredFeature<IStreamingHubFeature>();
        var magicOnionOptions = serviceProvider.GetRequiredService<IOptions<MagicOnionOptions>>().Value;
        timeProvider = magicOnionOptions.TimeProvider ?? TimeProvider.System;
        isReturnExceptionStackTraceInErrorDetail = magicOnionOptions.IsReturnExceptionStackTraceInErrorDetail;

        handlers = streamingHubFeature.Handlers;

        var remoteProxyFactory = serviceProvider.GetRequiredService<IRemoteProxyFactory>();
        var remoteSerializer = serviceProvider.GetRequiredService<IRemoteSerializer>();
        this.remoteClientResultPendingTasks = new RemoteClientResultPendingTaskRegistry(magicOnionOptions.ClientResultsDefaultTimeout, timeProvider);
        this.Client = remoteProxyFactory.CreateDirect<TReceiver>(new MagicOnionRemoteReceiverWriter(StreamingServiceContext), remoteSerializer, remoteClientResultPendingTasks);

        this.Group = new HubGroupRepository<TReceiver>(Client, StreamingServiceContext, streamingHubFeature.GroupProvider);

        heartbeatHandle = streamingHubFeature.HeartbeatManager.Register(StreamingServiceContext);
        features.Set<IMagicOnionHeartbeatFeature>(new MagicOnionHeartbeatFeature(heartbeatHandle));

        try
        {
            await OnConnecting();
            await HandleMessageAsync();
        }
        catch (OperationCanceledException)
        {
            // NOTE: If DuplexStreaming is disconnected by the client, OperationCanceledException will be thrown.
            //       However, such behavior is expected. the exception can be ignored.
        }
        catch (IOException ex) when (ex.InnerException is ConnectionAbortedException)
        {
            // NOTE: If DuplexStreaming is disconnected by the client, IOException will be thrown.
            //       However, such behavior is expected. the exception can be ignored.
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            var httpRequestLifetimeFeature = features.Get<IHttpRequestLifetimeFeature>();

            // NOTE: If the connection is completed when a message is written, PipeWriter throws an InvalidOperationException.
            // NOTE: If the connection is closed with STREAM_RST, PipeReader throws an IOException.
            //       However, such behavior is expected. the exception can be ignored.
            //       https://github.com/dotnet/aspnetcore/blob/v6.0.0/src/Servers/Kestrel/Core/src/Internal/Http2/Http2Stream.cs#L516-L523
            if (httpRequestLifetimeFeature is null || httpRequestLifetimeFeature.RequestAborted.IsCancellationRequested is false)
            {
                throw;
            }
        }
        finally
        {
            Metrics.StreamingHubConnectionDecrement(Context.Metrics, Context.ServiceName);

            requests.Writer.Complete();
            StreamingServiceContext.CompleteStreamingHub();
            heartbeatHandle.Unregister(); // NOTE: To be able to use CancellationToken within OnDisconnected event, separate the calls to Dispose and Unregister.

            await OnDisconnected();

            await this.Group.DisposeAsync();
            heartbeatHandle.Dispose();
            remoteClientResultPendingTasks.Dispose();
        }

        return default;
    }

    async Task HandleMessageAsync()
    {
        var ct = CancellationTokenSource.CreateLinkedTokenSource(Context.CallContext.CancellationToken, heartbeatHandle.TimeoutToken).Token;
        var reader = StreamingServiceContext.RequestStream!;
        var writer = StreamingServiceContext.ResponseStream!;

        // Send a hint to the client to start sending messages.
        // The client can read the response headers before any StreamingHub's message.
        await Context.CallContext.WriteResponseHeadersAsync(ResponseHeaders);

        // Write a marker that is the beginning of the stream.
        // NOTE: To prevent buffering by AWS ALB or reverse-proxy.
        await writer.WriteAsync(StreamingHubPayloadPool.Shared.RentOrCreate(MarkerResponseBytes));

        // Call OnConnected after sending the headers and marker.
        // The server can send messages or broadcast to client after OnConnected.
        // eg: Send the current game state to the client.
        await OnConnected();

        // Starts a loop that consumes the request queue.
        var consumeRequestsTask = ConsumeRequestQueueAsync();

        // Main loop of StreamingHub.
        // Be careful to allocation and performance.
        while (await reader.MoveNext(ct))
        {
            var payload = reader.Current;

            await ProcessMessageAsync(payload, ct);

            // NOTE: DO NOT return the StreamingHubPayload to the pool here.
            //       Client requests may be pending at this point.
        }
    }


    ValueTask ProcessMessageAsync(StreamingHubPayload payload, CancellationToken cancellationToken)
    {
        var reader = new StreamingHubServerMessageReader(payload.Memory);
        var messageType = reader.ReadMessageType();

        switch (messageType)
        {
            case StreamingHubMessageType.Request:
                {
                    var requestMessage = reader.ReadRequest();
                    return requests.Writer.WriteAsync(new (payload, requestMessage.MethodId, requestMessage.MessageId, requestMessage.Body, true), cancellationToken);
                }
            case StreamingHubMessageType.RequestFireAndForget:
                {
                    var requestMessage = reader.ReadRequestFireAndForget();
                    return requests.Writer.WriteAsync(new (payload, requestMessage.MethodId, -1, requestMessage.Body, false), cancellationToken);
                }
            case StreamingHubMessageType.ClientResultResponse:
                {
                    var responseMessage = reader.ReadClientResultResponse();
                    if (remoteClientResultPendingTasks.TryGetAndUnregisterPendingTask(responseMessage.ClientResultMessageId, out var pendingMessage))
                    {
                        pendingMessage.TrySetResult(responseMessage.Body);
                    }
                    return default;
                }
            case StreamingHubMessageType.ClientResultResponseWithError:
                {
                    var responseMessage = reader.ReadClientResultResponseForError();
                    if (remoteClientResultPendingTasks.TryGetAndUnregisterPendingTask(responseMessage.ClientResultMessageId, out var pendingMessage))
                    {
                        pendingMessage.TrySetException(new RpcException(new Status((StatusCode)responseMessage.StatusCode, responseMessage.Message + (string.IsNullOrEmpty(responseMessage.Detail) ? string.Empty : Environment.NewLine + responseMessage.Detail))));
                    }
                    return default;
                }
            case StreamingHubMessageType.ServerHeartbeatResponse:
            {
                    var seq = reader.ReadServerHeartbeatResponse();
                    heartbeatHandle.Ack(seq);
                    return default;
                }
            case StreamingHubMessageType.ClientHeartbeat:
                {
                    var (seq, clientSentAt, heartbeatBody) = reader.ReadClientHeartbeat();

                    using var bufferWriter = ArrayPoolBufferWriter.RentThreadStaticWriter();
                    StreamingHubMessageWriter.WriteClientHeartbeatMessageResponse(bufferWriter, seq, clientSentAt);
                    bufferWriter.Write(heartbeatBody.Span); // Copy an extra body to the response message.

                    StreamingServiceContext.QueueResponseStreamWrite(StreamingHubPayloadPool.Shared.RentOrCreate(bufferWriter.WrittenSpan));
                    return default;
                }
            default:
                ThrowUnknownMessageType(messageType);
                return default;
        }

        [DoesNotReturn]
        static void ThrowUnknownMessageType(StreamingHubMessageType messageType)
            => throw new InvalidOperationException($"Unknown MessageType: {messageType}");
    }

    async ValueTask ConsumeRequestQueueAsync()
    {
        // Create and reuse a single StreamingHubContext for each hub connection.
        var hubContext = new StreamingHubContext();

        // We need to process client requests sequentially.
        // NOTE: Do not pass a CancellationToken to avoid allocation. We call Writer.Complete when we want to stop the consumption loop.
        await foreach (var request in requests.Reader.ReadAllAsync(default))
        {
            try
            {
                var handler = GetOrThrowHandler(request.MethodId);

                hubContext.Initialize(
                    handler: handler,
                    streamingServiceContext: (IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>)Context,
                    hubInstance: this,
                    request: request.Body,
                    messageId: request.MessageId,
                    timestamp: timeProvider.GetUtcNow().UtcDateTime
                );

                var isErrorOrInterrupted = false;
                var methodStartingTimestamp = timeProvider.GetTimestamp();
                MagicOnionServerLog.BeginInvokeHubMethod(Context.Logger, hubContext, hubContext.Request, handler.RequestType);

                try
                {
                    await handler.MethodBody.Invoke(hubContext);
                }
                catch (Exception ex)
                {
                    isErrorOrInterrupted = true;
                    HandleException(hubContext, ex, request.HasResponse);
                }
                finally
                {
                    CleanupRequest(hubContext, methodStartingTimestamp, isErrorOrInterrupted);
                }
            }
            finally
            {
                StreamingHubPayloadPool.Shared.Return(request.Payload);
            }
        }
    }

    StreamingHubHandler GetOrThrowHandler(int methodId)
    {
        if (!handlers.TryGetValue(methodId, out var handler))
        {
            throw new InvalidOperationException("Handler not found in received methodId, methodId:" + methodId);
        }

        return handler;
    }

    void HandleException(StreamingHubContext hubContext, Exception ex, bool hasResponse)
    {
        if (ex is ReturnStatusException rse)
        {
            if (hasResponse)
            {
                hubContext.WriteErrorMessage((int)rse.StatusCode, rse.Detail, null, false);
            }
        }
        else
        {
            MagicOnionServerLog.Error(Context.Logger, ex, hubContext);
            Metrics.StreamingHubException(Context.Metrics, hubContext.Handler, ex);

            if (hasResponse)
            {
                hubContext.WriteErrorMessage((int)StatusCode.Internal, $"An error occurred while processing handler '{hubContext.Handler}'.", ex, isReturnExceptionStackTraceInErrorDetail);
            }
        }
    }

    void CleanupRequest(StreamingHubContext hubContext, long methodStartingTimestamp, bool isErrorOrInterrupted)
    {
        var methodEndingTimestamp = timeProvider.GetTimestamp();
        var elapsed = timeProvider.GetElapsedTime(methodStartingTimestamp, methodEndingTimestamp);
        MagicOnionServerLog.EndInvokeHubMethod(Context.Logger, hubContext, hubContext.ResponseSize, hubContext.ResponseType, elapsed.TotalMilliseconds, isErrorOrInterrupted);
        Metrics.StreamingHubMethodCompleted(Context.Metrics, hubContext.Handler, methodStartingTimestamp, methodEndingTimestamp, isErrorOrInterrupted);
        hubContext.Uninitialize();
    }

    // Interface methods for Client

    THubInterface IStreamingHub<THubInterface, TReceiver>.FireAndForget()
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }

    Task IStreamingHub<THubInterface, TReceiver>.DisposeAsync()
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }

    Task IStreamingHub<THubInterface, TReceiver>.WaitForDisconnect()
    {
        throw new NotSupportedException("Invoke from client proxy only");
    }
}
