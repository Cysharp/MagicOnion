using MagicOnion.Internal.Buffers;
using MessagePack;
using System.Collections.Concurrent;
using System.Diagnostics;
using MagicOnion.Internal;

namespace MagicOnion.Server.Hubs;

public class StreamingHubContext
{
    IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> streamingServiceContext = default!;
    ConcurrentDictionary<string, object>? items;
    StreamingHubHandler handler = default!;

    /// <summary>Object storage per invoke.</summary>
    public ConcurrentDictionary<string, object> Items
    {
        get
        {
            lock (this) // lock per self! is this dangerous?
            {
                if (items == null) items = new ConcurrentDictionary<string, object>();
            }
            return items;
        }
    }

    public string Path => handler.ToString();
    public ILookup<Type, Attribute> AttributeLookup => handler.AttributeLookup;

    public object HubInstance { get; private set; } = default!;

    public ReadOnlyMemory<byte> Request { get; private set; }
    public DateTime Timestamp { get; private set; }

    public Guid ConnectionId => streamingServiceContext.ContextId;

    public IServiceContext ServiceContext => streamingServiceContext;

    internal int MessageId { get; private set; }
    internal int MethodId => handler.MethodId;

    internal int ResponseSize { get; private set; } = -1;
    internal Type? ResponseType { get; private set; }

    internal void Initialize(StreamingHubHandler handler, IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> streamingServiceContext, object hubInstance, ReadOnlyMemory<byte> request, DateTime timestamp, int messageId)
    {
#if DEBUG
        Debug.Assert(this.handler is null);
        Debug.Assert(this.streamingServiceContext is null);
        Debug.Assert(this.HubInstance is null);
#endif
        this.handler = handler;
        this.streamingServiceContext = streamingServiceContext;
        HubInstance = hubInstance;
        Request = request;
        Timestamp = timestamp;
        MessageId = messageId;
    }

    internal void Uninitialize()
    {
#if DEBUG
        Debug.Assert(this.handler is not null);
        Debug.Assert(this.streamingServiceContext is not null);
        Debug.Assert(this.HubInstance is not null);
#endif

        handler = default!;
        streamingServiceContext = default!;
        HubInstance = default!;
        Request = default!;
        Timestamp = default!;
        MessageId = default!;
        ResponseSize = -1;
        items?.Clear();
    }

    // helper for reflection
    internal ValueTask WriteResponseMessageNil(ValueTask value)
    {
        if (MessageId == -1) // No need to write a response. We do not write response.
        {
            return default;
        }

        ResponseType = typeof(Nil);
        if (value.IsCompletedSuccessfully)
        {
            WriteMessageCore(BuildMessage());
            return default;
        }
        return Await(this, value);

        static async ValueTask Await(StreamingHubContext ctx, ValueTask value)
        {
            await value.ConfigureAwait(false);
            ctx.WriteMessageCore(ctx.BuildMessage());
        }
    }

    internal ValueTask WriteResponseMessage<T>(ValueTask<T> value)
    {
        if (MessageId == -1) // No need to write a response. We do not write response.
        {
            return default;
        }

        ResponseType = typeof(T);
        if (value.IsCompletedSuccessfully)
        {
            WriteMessageCore(BuildMessage(value.Result));
            return default;
        }
        return Await(this, value);

        static async ValueTask Await(StreamingHubContext ctx, ValueTask<T> value)
        {
            var vv = await value.ConfigureAwait(false);
            ctx.WriteMessageCore(ctx.BuildMessage(vv));
        }
    }

    internal void WriteErrorMessage(int statusCode, string detail, Exception? ex, bool isReturnExceptionStackTraceInErrorDetail)
    {
        WriteMessageCore(BuildMessageForError(statusCode, detail, ex, isReturnExceptionStackTraceInErrorDetail));
    }

    void WriteMessageCore(StreamingHubPayload payload)
    {
        ResponseSize = payload.Length; // NOTE: We cannot use the payload after QueueResponseStreamWrite.
        streamingServiceContext.QueueResponseStreamWrite(payload);
    }

    StreamingHubPayload BuildMessage()
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteResponseMessage(buffer, MethodId, MessageId);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }

    StreamingHubPayload BuildMessage<T>(T v)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteResponseMessage(buffer, MethodId, MessageId, v, streamingServiceContext.MessageSerializer);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }

    StreamingHubPayload BuildMessageForError(int statusCode, string detail, Exception? ex, bool isReturnExceptionStackTraceInErrorDetail)
    {
        using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();
        StreamingHubMessageWriter.WriteResponseMessageForError(buffer, MessageId, statusCode, detail, ex, isReturnExceptionStackTraceInErrorDetail);
        return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
    }
}
