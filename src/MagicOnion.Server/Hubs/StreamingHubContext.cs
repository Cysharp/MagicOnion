using MagicOnion.Internal.Buffers;
using MessagePack;
using System.Collections.Concurrent;
using MagicOnion.Internal;
using Microsoft.Extensions.ObjectPool;
using Grpc.Core;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Hubs;

internal class StreamingHubContextPool
{
    const int MaxRetainedCount = 16;
    readonly ObjectPool<StreamingHubContext> pool = new DefaultObjectPool<StreamingHubContext>(new Policy(), MaxRetainedCount);

    public static StreamingHubContextPool Shared { get; } = new();

    public StreamingHubContext Get() => pool.Get();
    public void Return(StreamingHubContext ctx) => pool.Return(ctx);

    class Policy : IPooledObjectPolicy<StreamingHubContext>
    {
        public StreamingHubContext Create()
        {
            return new StreamingHubContext();
        }

        public bool Return(StreamingHubContext obj)
        {
            obj.Uninitialize();
            return true;
        }
    }
}

public class StreamingHubContext
{
    ConcurrentDictionary<string, object>? items;

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

    public object HubInstance { get; private set; } = default!;

    public ReadOnlyMemory<byte> Request { get; private set; }
    public string Path { get; private set; } = default!;
    public DateTime Timestamp { get; private set; }

    public Guid ConnectionId => ServiceContext.ContextId;

    /// <summary>Raw gRPC Context.</summary>
    internal IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> ServiceContext { get; private set; } = default!;

    internal int MessageId { get; private set; }
    internal int MethodId { get; private set; }

    internal int ResponseSize { get; private set; } = -1;
    internal Type? ResponseType { get; private set; }

    internal void Initialize(IStreamingServiceContext<StreamingHubPayload, StreamingHubPayload> serviceContext, object hubInstance, ReadOnlyMemory<byte> request, string path, DateTime timestamp, int messageId, int methodId)
    {
        ServiceContext = serviceContext;
        HubInstance = hubInstance;
        Request = request;
        Path = path;
        Timestamp = timestamp;
        MessageId = messageId;
        MethodId = methodId;
    }

    internal void Uninitialize()
    {
        ServiceContext = default!;
        HubInstance = default!;
        Request = default!;
        Path = default!;
        Timestamp = default!;
        MessageId = default!;
        MethodId = default!;
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

    internal ValueTask WriteErrorMessage(int statusCode, string detail, Exception? ex, bool isReturnExceptionStackTraceInErrorDetail)
    {
        WriteMessageCore(BuildMessageForError(statusCode, detail, ex, isReturnExceptionStackTraceInErrorDetail));
        return default;
    }

    void WriteMessageCore(StreamingHubPayload payload)
    {
        ResponseSize = payload.Length; // NOTE: We cannot use the payload after QueueResponseStreamWrite.
        ServiceContext.QueueResponseStreamWrite(payload);
    }

    // MessageFormat:
    // response:  [messageId, methodId, response]
    StreamingHubPayload BuildMessage()
    {
        using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
        {
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(MessageId);
            writer.Write(MethodId);
            writer.WriteNil();
            writer.Flush();
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }
    }

    // MessageFormat:
    // response:  [messageId, methodId, response]
    StreamingHubPayload BuildMessage<T>(T v)
    {
        using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
        {
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(MessageId);
            writer.Write(MethodId);
            writer.Flush();
            ServiceContext.MessageSerializer.Serialize(buffer, v);
            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }
    }

    // MessageFormat:
    // error-response:  [messageId, statusCode, detail, StringMessage]
    StreamingHubPayload BuildMessageForError(int statusCode, string detail, Exception? ex, bool isReturnExceptionStackTraceInErrorDetail)
    {
        using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
        {
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(4);
            writer.Write(MessageId);
            writer.Write(statusCode);
            writer.Write(detail);

            var msg = (isReturnExceptionStackTraceInErrorDetail && ex != null)
                ? ex.ToString()
                : null;

            writer.Write(msg);
            writer.Flush();

            return StreamingHubPayloadPool.Shared.RentOrCreate(buffer.WrittenSpan);
        }
    }
}
