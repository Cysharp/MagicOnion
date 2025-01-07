#if !USE_OBJECTPOOL_STREAMINGHUBPAYLOADPOOL
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MagicOnion.Internal;

internal class ObjectPool<T> where T : class
{
    readonly Func<T> factory;

    T? item1;
    T? item2;
    T? item3;
    T? item4;

    public ObjectPool(Func<T> factory)
    {
        this.factory = factory;
    }

    public T RentOrCreateCore()
    {
        T? tmpItem;
        if (!(TryGet(ref item1, out tmpItem) ||
              TryGet(ref item2, out tmpItem) ||
              TryGet(ref item3, out tmpItem) ||
              TryGet(ref item4, out tmpItem)))
        {
            tmpItem = factory();
        }

        return tmpItem;
    }

    public void ReturnCore(T item)
    {
        var pooled = TryReturn(ref item1, item) ||
                     TryReturn(ref item2, item) ||
                     TryReturn(ref item3, item) ||
                     TryReturn(ref item4, item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool TryReturn(ref T? field, T payload)
        => Interlocked.CompareExchange(ref field, payload, null) == null;

    bool TryGet(ref T? field, [NotNullWhen(true)] out T? item)
    {
        var tmp = field;
        if (tmp != null && Interlocked.CompareExchange(ref field, null, tmp) == tmp)
        {
            item = tmp;
            return true;
        }

        item = null;
        return false;
    }
}

internal class StreamingHubPayloadPool
{
#if DEBUG
    readonly ObjectPool<StreamingHubPayloadCore> pool = new(static () => new StreamingHubPayloadCore());
#else
    readonly ObjectPool<StreamingHubPayloadCore> pool = new(static () => new StreamingHubPayload());
#endif

    public static StreamingHubPayloadPool Shared { get; } = new();

    public void Return(StreamingHubPayload payload)
    {
#if DEBUG
        payload.Core.Uninitialize();
        pool.ReturnCore(payload.Core);
#else
        payload.Uninitialize();
        pool.ReturnCore(payload);
#endif
    }

    public StreamingHubPayload RentOrCreate(ReadOnlySequence<byte> data)
    {
        var payload = pool.RentOrCreateCore();
        payload.Initialize(data);
#if DEBUG
        return new StreamingHubPayload(payload);
#else
        return (StreamingHubPayload)payload;
#endif
    }

    public StreamingHubPayload RentOrCreate(ReadOnlySpan<byte> data)
    {
        var payload = pool.RentOrCreateCore();
        payload.Initialize(data);
#if DEBUG
        return new StreamingHubPayload(payload);
#else
        return (StreamingHubPayload)payload;
#endif
    }

    public StreamingHubPayload RentOrCreate(ReadOnlyMemory<byte> data, bool holdReference)
    {
        var payload = pool.RentOrCreateCore();
        payload.Initialize(data, holdReference);
#if DEBUG
        return new StreamingHubPayload(payload);
#else
        return (StreamingHubPayload)payload;
#endif
    }
}
#endif
