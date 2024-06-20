#if !USE_OBJECTPOOL_STREAMINGHUBPAYLOADPOOL
#nullable enable
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MagicOnion.Internal
{
    internal class ObjectPool<T> where T : class
    {
        T? item1;
        T? item2;
        T? item3;
        T? item4;

        protected T RentOrCreateCore(Func<T> factory)
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

        protected void ReturnCore(T item)
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

    internal class StreamingHubPayloadPool : ObjectPool<StreamingHubPayload>
    {
        public static StreamingHubPayloadPool Shared { get; } = new();

        public void Return(StreamingHubPayload payload)
        {
            ((IStreamingHubPayload)payload).Uninitialize();
            ReturnCore(payload);
        }

        public StreamingHubPayload RentOrCreate(ReadOnlySequence<byte> data)
        {
            var payload = RentOrCreateCore(static () => new StreamingHubPayload());
            ((IStreamingHubPayload)payload).Initialize(data);
            return payload;
        }

        public StreamingHubPayload RentOrCreate(ReadOnlySpan<byte> data)
        {
            var payload = RentOrCreateCore(static () => new StreamingHubPayload());
            ((IStreamingHubPayload)payload).Initialize(data);
            return payload;
        }

        public StreamingHubPayload RentOrCreate(ReadOnlyMemory<byte> data)
        {
            var payload = RentOrCreateCore(static () => new StreamingHubPayload());
            ((IStreamingHubPayload)payload).Initialize(data);
            return payload;
        }
    }
}
#endif
