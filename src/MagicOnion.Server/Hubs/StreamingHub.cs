using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Cysharp.Runtime.Multicast.Remoting;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Features;
using MagicOnion.Server.Hubs.Internal;
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

    protected static readonly Task<Nil> NilTask = Task.FromResult(Nil.Default);
    protected static readonly ValueTask CompletedTask = new ValueTask();

    static readonly Metadata ResponseHeaders = new Metadata()
    {
        { "x-magiconion-streaminghub-version", "2" },
    };

    // response:  [messageId, methodId, response]
    // HACK: If the ID of the message is `-1`, the client will ignore the message.
    static ReadOnlySpan<byte> MarkerResponseBytes => [0x93, 0xff, 0x00, 0x0c]; // MsgPack: [-1, 0, nil]

    readonly Channel<(StreamingHubPayload Payload, UniqueHashDictionary<StreamingHubHandler> Handlers, int MethodId, int MessageId, ReadOnlyMemory<byte> Body, bool HasResponse)> requests
        = Channel.CreateBounded<(StreamingHubPayload, UniqueHashDictionary<StreamingHubHandler>, int, int, ReadOnlyMemory<byte>, bool)>(new BoundedChannelOptions(capacity: 10)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
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

    Task<DuplexStreamingResult<StreamingHubPayload, StreamingHubPayload>> IStreamingHubBase.Connect()
        => Connect();

    [Obsolete]
    internal async Task<DuplexStreamingResult<StreamingHubPayload, StreamingHubPayload>> Connect()
    {
        Metrics.StreamingHubConnectionIncrement(Context.Metrics, Context.ServiceName);

        var streamingContext = GetDuplexStreamingContext<StreamingHubPayload, StreamingHubPayload>();
        var serviceProvider = streamingContext.ServiceContext.ServiceProvider;

        var features = this.Context.CallContext.GetHttpContext().Features;
        streamingHubFeature = features.Get<IStreamingHubFeature>()!; // TODO: GetRequiredFeature
        var magicOnionOptions = serviceProvider.GetRequiredService<IOptions<MagicOnionOptions>>().Value;
        timeProvider = magicOnionOptions.TimeProvider ?? TimeProvider.System;
        isReturnExceptionStackTraceInErrorDetail = magicOnionOptions.IsReturnExceptionStackTraceInErrorDetail;

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

            StreamingServiceContext.CompleteStreamingHub();
            heartbeatHandle.Unregister(); // NOTE: To be able to use CancellationToken within OnDisconnected event, separate the calls to Dispose and Unregister.

            await OnDisconnected();

            await this.Group.DisposeAsync();
            heartbeatHandle.Dispose();
            remoteClientResultPendingTasks.Dispose();
        }

        return streamingContext.Result();
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

        var handlers = streamingHubFeature.Handlers;

        // Starts a loop that consumes the request queue.
        var consumeRequestsTask = ConsumeRequestQueueAsync(ct);

        // Main loop of StreamingHub.
        // Be careful to allocation and performance.
        while (await reader.MoveNext(ct))
        {
            var payload = reader.Current;

            await ProcessMessageAsync(payload, handlers, ct);

            // NOTE: DO NOT return the StreamingHubPayload to the pool here.
            //       Client requests may be pending at this point.
        }
    }

    async ValueTask ConsumeRequestQueueAsync(CancellationToken cancellationToken)
    {
        // We need to process client requests sequentially.
        await foreach (var request in requests.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await ProcessRequestAsync(request.Handlers, request.MethodId, request.MessageId, request.Body, request.HasResponse);
            }
            finally
            {
                StreamingHubPayloadPool.Shared.Return(request.Payload);
            }
        }
    }

    ValueTask ProcessMessageAsync(StreamingHubPayload payload, UniqueHashDictionary<StreamingHubHandler> handlers, CancellationToken cancellationToken)
    {
        var reader = new StreamingHubServerMessageReader(payload.Memory);
        var messageType = reader.ReadMessageType();

        switch (messageType)
        {
            case StreamingHubMessageType.Request:
                {
                    var requestMessage = reader.ReadRequest();
                    return requests.Writer.WriteAsync((payload, handlers, requestMessage.MethodId, requestMessage.MessageId, requestMessage.Body, true), cancellationToken);
                }
            case StreamingHubMessageType.RequestFireAndForget:
                {
                    var requestMessage = reader.ReadRequestFireAndForget();
                    return requests.Writer.WriteAsync((payload, handlers, requestMessage.MethodId, -1, requestMessage.Body, false), cancellationToken);
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
                throw new InvalidOperationException($"Unknown MessageType: {messageType}");
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    async ValueTask ProcessRequestAsync(UniqueHashDictionary<StreamingHubHandler> handlers, int methodId, int messageId, ReadOnlyMemory<byte> body, bool hasResponse)
    {
        if (handlers.TryGetValue(methodId, out var handler))
        {
            // Create a context for each call to the hub method.
            var context = StreamingHubContextPool.Shared.Get();
            context.Initialize(
                handler: handler,
                streamingServiceContext: (IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>)Context,
                hubInstance: this,
                request: body,
                messageId: messageId,
                timestamp: timeProvider.GetUtcNow().UtcDateTime
            );

            var methodStartingTimestamp = timeProvider.GetTimestamp();
            var isErrorOrInterrupted = false;
            MagicOnionServerLog.BeginInvokeHubMethod(Context.Logger, context, context.Request, handler.RequestType);
            try
            {
                await handler.MethodBody.Invoke(context);
            }
            catch (ReturnStatusException ex)
            {
                if (hasResponse)
                {
                    await context.WriteErrorMessage((int)ex.StatusCode, ex.Detail, null, false);
                }
            }
            catch (Exception ex)
            {
                isErrorOrInterrupted = true;
                MagicOnionServerLog.Error(Context.Logger, ex, context);
                Metrics.StreamingHubException(Context.Metrics, handler, ex);

                if (hasResponse)
                {
                    await context.WriteErrorMessage((int)StatusCode.Internal, $"An error occurred while processing handler '{handler.ToString()}'.", ex, isReturnExceptionStackTraceInErrorDetail);
                }
            }
            finally
            {
                var methodEndingTimestamp = timeProvider.GetTimestamp();
                MagicOnionServerLog.EndInvokeHubMethod(Context.Logger, context, context.ResponseSize, context.ResponseType, timeProvider.GetElapsedTime(methodStartingTimestamp, methodEndingTimestamp).TotalMilliseconds, isErrorOrInterrupted);
                Metrics.StreamingHubMethodCompleted(Context.Metrics, handler, methodStartingTimestamp, methodEndingTimestamp, isErrorOrInterrupted);

                StreamingHubContextPool.Shared.Return(context);
            }
        }
        else
        {
            throw new InvalidOperationException("Handler not found in received methodId, methodId:" + methodId);
        }
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
