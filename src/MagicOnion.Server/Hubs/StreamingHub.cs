using System.Diagnostics;
using Grpc.Core;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Hubs;

public abstract class StreamingHubBase<THubInterface, TReceiver> : ServiceBase<THubInterface>, IStreamingHub<THubInterface, TReceiver>
    where THubInterface : IStreamingHub<THubInterface, TReceiver>
{
    protected static readonly Task<Nil> NilTask = Task.FromResult(Nil.Default);
    protected static readonly ValueTask CompletedTask = new ValueTask();

    static readonly Metadata ResponseHeaders = new Metadata()
    {
        { "x-magiconion-streaminghub-version", "2" },
    };

    // response:  [messageId, methodId, response]
    // HACK: If the ID of the message is `-1`, the client will ignore the message.
    static readonly byte[] MarkerResponseBytes = { 0x93, 0xff, 0x00, 0x0c }; // MsgPack: [-1, 0, nil]

    public HubGroupRepository Group { get; private set; } = default!; /* lateinit */

    internal StreamingServiceContext<byte[], byte[]> StreamingServiceContext
        => (StreamingServiceContext<byte[], byte[]>)Context;

    protected Guid ConnectionId
        => Context.ContextId;

    // Broadcast Commands

    [Ignore]
    protected TReceiver Broadcast(IGroup group)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType;
        return (TReceiver)Activator.CreateInstance(type, group)!;
    }

    [Ignore]
    protected TReceiver BroadcastExceptSelf(IGroup group)
    {
        return BroadcastExcept(group, Context.ContextId);
    }

    [Ignore]
    protected TReceiver BroadcastExcept(IGroup group, Guid except)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptOne;
        return (TReceiver)Activator.CreateInstance(type, new object[] { group, except })!;
    }

    [Ignore]
    protected TReceiver BroadcastExcept(IGroup group, Guid[] excepts)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptMany;
        return (TReceiver)Activator.CreateInstance(type, new object[] { group, excepts })!;
    }

    [Ignore]
    protected TReceiver BroadcastToSelf(IGroup group)
    {
        return BroadcastTo(group, Context.ContextId);
    }

    [Ignore]
    protected TReceiver BroadcastTo(IGroup group, Guid toConnectionId)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToOne;
        return (TReceiver)Activator.CreateInstance(type, new object[] { group, toConnectionId })!;
    }

    [Ignore]
    protected TReceiver BroadcastTo(IGroup group, Guid[] toConnectionIds)
    {
        var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToMany;
        return (TReceiver)Activator.CreateInstance(type, new object[] { group, toConnectionIds })!;
    }

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

    public async Task<DuplexStreamingResult<byte[], byte[]>> Connect()
    {
        Metrics.StreamingHubConnectionIncrement(Context.Metrics, Context.MethodHandler.ServiceName);

        var streamingContext = GetDuplexStreamingContext<byte[], byte[]>();

        var group = Context.ServiceProvider.GetRequiredService<StreamingHubHandlerRepository>().GetGroupRepository(Context.MethodHandler);
        this.Group = new HubGroupRepository(this.Context, group);
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
            var httpRequestLifetimeFeature = this.Context.CallContext.GetHttpContext()?.Features.Get<IHttpRequestLifetimeFeature>();

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
            Metrics.StreamingHubConnectionDecrement(Context.Metrics, Context.MethodHandler.ServiceName);

            StreamingServiceContext.CompleteStreamingHub();
            await OnDisconnected();
            await this.Group.DisposeAsync();
        }

        return streamingContext.Result();
    }

    async Task HandleMessageAsync()
    {
        var ct = Context.CallContext.CancellationToken;
        var reader = StreamingServiceContext.RequestStream!;
        var writer = StreamingServiceContext.ResponseStream!;

        // Send a hint to the client to start sending messages.
        // The client can read the response headers before any StreamingHub's message.
        await Context.CallContext.WriteResponseHeadersAsync(ResponseHeaders);

        // Write a marker that is the beginning of the stream.
        // NOTE: To prevent buffering by AWS ALB or reverse-proxy.
        await writer.WriteAsync(MarkerResponseBytes);

        // Call OnConnected after sending the headers and marker.
        // The server can send messages or broadcast to client after OnConnected.
        // eg: Send the current game state to the client.
        await OnConnected();

        var handlers = Context.ServiceProvider.GetRequiredService<StreamingHubHandlerRepository>().GetHandlers(Context.MethodHandler);

        // Main loop of StreamingHub.
        // Be careful to allocation and performance.
        while (await reader.MoveNext(ct)) // must keep SyncContext.
        {
            var data = reader.Current;
            var (methodId, messageId, offset) = FetchHeader(data);
            var hasResponse = messageId != -1;

            if (handlers.TryGetValue(methodId, out var handler))
            {
                // Create a context for each call to the hub method.
                var context = new StreamingHubContext()
                {
                    HubInstance = this,
                    ServiceContext = (IStreamingServiceContext<byte[], byte[]>)Context,
                    Request = data.AsMemory(offset, data.Length - offset),
                    Path = handler.ToString(),
                    MethodId = handler.MethodId,
                    MessageId = messageId,
                    Timestamp = DateTime.UtcNow
                };

                var methodStartingTimestamp = Stopwatch.GetTimestamp();
                var isErrorOrInterrupted = false;
                MagicOnionServerLog.BeginInvokeHubMethod(Context.MethodHandler.Logger, context, context.Request, handler.RequestType);
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
                    MagicOnionServerLog.Error(Context.MethodHandler.Logger, ex, context);
                    Metrics.StreamingHubException(Context.Metrics, handler, ex);

                    if (hasResponse)
                    {
                        await context.WriteErrorMessage((int)StatusCode.Internal, $"An error occurred while processing handler '{handler.ToString()}'.", ex, Context.MethodHandler.IsReturnExceptionStackTraceInErrorDetail);
                    }
                }
                finally
                {
                    var methodEndingTimestamp = Stopwatch.GetTimestamp();
                    MagicOnionServerLog.EndInvokeHubMethod(Context.MethodHandler.Logger, context, context.responseSize, context.responseType, StopwatchHelper.GetElapsedTime(methodStartingTimestamp, methodEndingTimestamp).TotalMilliseconds, isErrorOrInterrupted);
                    Metrics.StreamingHubMethodCompleted(Context.Metrics, handler, methodStartingTimestamp, methodEndingTimestamp, isErrorOrInterrupted);
                }
            }
            else
            {
                throw new InvalidOperationException("Handler not found in received methodId, methodId:" + methodId);
            }
        }
    }

    static (int methodId, int messageId, int offset) FetchHeader(byte[] msgData)
    {
        var messagePackReader = new MessagePackReader(msgData);

        var length = messagePackReader.ReadArrayHeader();
        if (length == 2)
        {
            // void: [methodId, [argument]]
            var mid = messagePackReader.ReadInt32();
            var consumed = (int)messagePackReader.Consumed;

            return (mid, -1, consumed);
        }
        else if (length == 3)
        {
            // T: [messageId, methodId, [argument]]
            var msgId = messagePackReader.ReadInt32();
            var metId = messagePackReader.ReadInt32();
            var consumed = (int)messagePackReader.Consumed;
            return (metId, msgId, consumed);
        }
        else
        {
            throw new InvalidOperationException("Invalid data format.");
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
