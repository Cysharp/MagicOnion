#if !USE_OBJECTPOOL_STREAMINGHUBPAYLOADPOOL
#nullable enable
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MagicOnion.Internal
{
    internal class StreamingHubPayloadPool
    {
        StreamingHubPayload? pool1;
        StreamingHubPayload? pool2;
        StreamingHubPayload? pool3;
        StreamingHubPayload? pool4;

        static StreamingHubPayloadPool pool = new();

        public static StreamingHubPayloadPool Shared => pool;

        public void Return(StreamingHubPayload payload)
        {
            ((IStreamingHubPayload)payload).Uninitialize();

            var pooled = TryReturn(ref pool1, payload) ||
                         TryReturn(ref pool2, payload) ||
                         TryReturn(ref pool3, payload) ||
                         TryReturn(ref pool4, payload);
        }

        public StreamingHubPayload RentOrCreate(ReadOnlySequence<byte> data)
        {
            StreamingHubPayload? tmpPayload;
            if (!(TryGet(ref pool1, out tmpPayload) ||
                  TryGet(ref pool2, out tmpPayload) ||
                  TryGet(ref pool3, out tmpPayload) ||
                  TryGet(ref pool4, out tmpPayload)))
            {
                tmpPayload = new StreamingHubPayload();
            }

            ((IStreamingHubPayload)tmpPayload).Initialize(data);

            return tmpPayload;
        }

        public StreamingHubPayload RentOrCreate(ReadOnlySpan<byte> data)
        {
            StreamingHubPayload? tmpPayload;
            if (!(TryGet(ref pool1, out tmpPayload) ||
                  TryGet(ref pool2, out tmpPayload) ||
                  TryGet(ref pool3, out tmpPayload) ||
                  TryGet(ref pool4, out tmpPayload)))
            {
                tmpPayload = new StreamingHubPayload();
            }

            ((IStreamingHubPayload)tmpPayload).Initialize(data);

            return tmpPayload;
        }

        public StreamingHubPayload RentOrCreate(ReadOnlyMemory<byte> data)
        {
            StreamingHubPayload? tmpPayload;
            if (!(TryGet(ref pool1, out tmpPayload) ||
                  TryGet(ref pool2, out tmpPayload) ||
                  TryGet(ref pool3, out tmpPayload) ||
                  TryGet(ref pool4, out tmpPayload)))
            {
                tmpPayload = new StreamingHubPayload();
            }

            ((IStreamingHubPayload)tmpPayload).Initialize(data);

            return tmpPayload;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TryReturn(ref StreamingHubPayload? field, StreamingHubPayload payload)
            => Interlocked.CompareExchange(ref field, payload, null) == null;

        bool TryGet(ref StreamingHubPayload? field, [NotNullWhen(true)] out StreamingHubPayload? payload)
        {
            var tmp = field;
            if (tmp != null && Interlocked.CompareExchange(ref field, null, tmp) == tmp)
            {
                payload = tmp;
                return true;
            }

            payload = null;
            return false;
        }
    }
}
#endif
