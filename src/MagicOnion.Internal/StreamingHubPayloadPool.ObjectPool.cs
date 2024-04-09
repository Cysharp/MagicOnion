#if USE_OBJECTPOOL_STREAMINGHUBPAYLOADPOOL
using Microsoft.Extensions.ObjectPool;
using System.Buffers;

namespace MagicOnion.Internal;

internal class StreamingHubPayloadPool
{
    const int MaximumRetained = 2 << 7;

    readonly ObjectPool<StreamingHubPayload> pool = new DefaultObjectPool<StreamingHubPayload>(new Policy(), MaximumRetained);

    public static StreamingHubPayloadPool Shared { get; } = new StreamingHubPayloadPool();

    public StreamingHubPayload RentOrCreate(ReadOnlySequence<byte> data)
    {
        var payload = pool.Get();
        ((IStreamingHubPayload)payload).Initialize(data);
        return payload;
    }

    public StreamingHubPayload RentOrCreate(ReadOnlySpan<byte> data)
    {
        var payload = pool.Get();
        ((IStreamingHubPayload)payload).Initialize(data);
        return payload;
    }

    public StreamingHubPayload RentOrCreate(ReadOnlyMemory<byte> data)
    {
        var payload = pool.Get();
        ((IStreamingHubPayload)payload).Initialize(data);
        return payload;
    }

    public void Return(StreamingHubPayload payload)
    {
        pool.Return(payload);
    }

    class Policy : IPooledObjectPolicy<StreamingHubPayload>
    {
        public StreamingHubPayload Create()
        {
            return new StreamingHubPayload();
        }

        public bool Return(StreamingHubPayload obj)
        {
            ((IStreamingHubPayload)obj).Uninitialize();
            return true;
        }
    }
}
#endif
