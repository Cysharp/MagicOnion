using System.Buffers;
using System.Threading.Channels;
using Grpc.Core;
using MagicOnion.Client.Tests;

namespace MagicOnion.Client.NativeAot.Tests;

public class MockCallInvoker : CallInvoker
{
    public List<byte[]> RequestPayloads { get; } = new();
    public Channel<byte[]> ResponseChannel { get; } = Channel.CreateUnbounded<byte[]>();

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
    {
        throw new NotImplementedException();
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
    {
        var serializationContext = new MockSerializationContext();
        method.RequestMarshaller.ContextualSerializer(request, serializationContext);
        RequestPayloads.Add(serializationContext.ToMemory().ToArray());

        return new AsyncUnaryCall<TResponse>(
            ResponseChannel.Reader.ReadAsync().AsTask().ContinueWith(x => method.ResponseMarshaller.ContextualDeserializer(new MockDeserializationContext(x.Result))),
            Task.FromResult(Metadata.Empty),
            () => Status.DefaultSuccess,
            () => Metadata.Empty,
            () => { });
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
    {
        throw new NotImplementedException();
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
    {
        throw new NotImplementedException();
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
    {
        throw new NotImplementedException();
    }
}

class MockDeserializationContext(byte[] payload) : DeserializationContext
{
    public override int PayloadLength => payload.Length;
    public override byte[] PayloadAsNewBuffer() => payload.ToArray();
    public override ReadOnlySequence<byte> PayloadAsReadOnlySequence() => new ReadOnlySequence<byte>(payload);
}
