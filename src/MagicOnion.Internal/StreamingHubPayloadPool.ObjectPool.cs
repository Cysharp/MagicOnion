#if USE_OBJECTPOOL_STREAMINGHUBPAYLOADPOOL
using System.Buffers;

namespace MagicOnion.Internal;

internal class StreamingHubPayloadPool
{
    public static StreamingHubPayloadPool Shared { get; } = new();

    public StreamingHubPayload RentOrCreate(ReadOnlySequence<byte> data)
    {
#if DEBUG
        var payload = new StreamingHubPayloadCore();
        payload.Initialize(data);
        return new StreamingHubPayload(payload);
#else
        var payload = new StreamingHubPayload();
        payload.Initialize(data);
        return payload;
#endif
    }

    public StreamingHubPayload RentOrCreate(ReadOnlySpan<byte> data)
    {
#if DEBUG
        var payload = new StreamingHubPayloadCore();
        payload.Initialize(data);
        return new StreamingHubPayload(payload);
#else
        var payload = new StreamingHubPayload();
        payload.Initialize(data);
        return payload;
#endif
    }

    public StreamingHubPayload RentOrCreate(ReadOnlyMemory<byte> data, bool holdReference)
    {
#if DEBUG
        var payload = new StreamingHubPayloadCore();
        payload.Initialize(data, holdReference);
        return new StreamingHubPayload(payload);
#else
        var payload = new StreamingHubPayload();
        payload.Initialize(data, holdReference);
        return payload;
#endif
    }

    public void Return(StreamingHubPayload payload)
    {
    }
}
#endif
