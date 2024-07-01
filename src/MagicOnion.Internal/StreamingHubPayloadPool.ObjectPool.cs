#if USE_OBJECTPOOL_STREAMINGHUBPAYLOADPOOL
using Microsoft.Extensions.ObjectPool;
using System.Buffers;

namespace MagicOnion.Internal;

internal class StreamingHubPayloadPool
{
    const int MaximumRetained = 2 << 7;

    readonly ObjectPool<StreamingHubPayloadCore> pool = new DefaultObjectPool<StreamingHubPayloadCore>(new Policy(), MaximumRetained);

    public static StreamingHubPayloadPool Shared { get; } = new();

    public StreamingHubPayload RentOrCreate(ReadOnlySequence<byte> data)
    {
        var payload = pool.Get();
        payload.Initialize(data);
#if DEBUG
        return new StreamingHubPayload(payload);
#else
        return (StreamingHubPayload)payload;
#endif
    }

    public StreamingHubPayload RentOrCreate(ReadOnlySpan<byte> data)
    {
        var payload = pool.Get();
        payload.Initialize(data);
#if DEBUG
        return new StreamingHubPayload(payload);
#else
        return (StreamingHubPayload)payload;
#endif
    }

    public StreamingHubPayload RentOrCreate(ReadOnlyMemory<byte> data)
    {
        var payload = pool.Get();
        payload.Initialize(data);
#if DEBUG
        return new StreamingHubPayload(payload);
#else
        return (StreamingHubPayload)payload;
#endif
    }

    public void Return(StreamingHubPayload payload)
    {
#if DEBUG
        payload.MarkAsReturned();
        pool.Return(payload.Core);
#else
        pool.Return(payload);
#endif
    }

    class Policy : IPooledObjectPolicy<StreamingHubPayloadCore>
    {
        public StreamingHubPayloadCore Create()
        {
#if DEBUG
            return new StreamingHubPayloadCore();
#else
            return new StreamingHubPayload();
#endif
        }

        public bool Return(StreamingHubPayloadCore obj)
        {
            obj.Uninitialize();
            return true;
        }
    }
}
#endif
