using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading.Channels;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Internal;
using MagicOnion.Internal.Buffers;
using MessagePack;

namespace Microbenchmark.Client;

class StreamingHubClientTestHelper<TStreamingHub, TReceiver>
    where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    where TReceiver : class
{
    readonly Channel<StreamingHubPayload> requestChannel;
    readonly ChannelClientStreamWriter<StreamingHubPayload> requestStream;
    readonly Channel<StreamingHubPayload> responseChannel;
    readonly ChannelAsyncStreamReader<StreamingHubPayload> responseStream;

    readonly CallInvoker callInvokerMock;
    readonly TReceiver receiver;
    readonly IStreamingHubClientFactoryProvider? factoryProvider;

    public TReceiver Receiver => receiver;
    public CallInvoker CallInvoker => callInvokerMock;

    public StreamingHubClientTestHelper(TReceiver receiver, IStreamingHubClientFactoryProvider? factoryProvider = null)
    {
        this.receiver = receiver;
        this.requestChannel = Channel.CreateUnbounded<StreamingHubPayload>(new() { SingleReader = true, SingleWriter = true });
        this.requestStream = new ChannelClientStreamWriter<StreamingHubPayload>(requestChannel);
        this.responseChannel = Channel.CreateUnbounded<StreamingHubPayload>(new() { SingleReader = true, SingleWriter = true });
        this.responseStream = new ChannelAsyncStreamReader<StreamingHubPayload>(responseChannel);

        this.factoryProvider = factoryProvider;

        callInvokerMock = new MockCallInvoker(this);
    }

    class MockCallInvoker(StreamingHubClientTestHelper<TStreamingHub, TReceiver> parent) : CallInvoker
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
            => throw new NotImplementedException();

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
            => throw new NotImplementedException();

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
            => throw new NotImplementedException();

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
            => throw new NotImplementedException();

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
            => new(
                (IClientStreamWriter<TRequest>)parent.requestStream,
                (IAsyncStreamReader<TResponse>)parent.responseStream,
                _ => Task.FromResult(new Metadata { { "x-magiconion-streaminghub-version", "2" } }),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
                new object());
    }

    public async Task<TStreamingHub> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return await StreamingHubClient.ConnectAsync<TStreamingHub, TReceiver>(
            callInvokerMock,
            receiver,
            cancellationToken: cancellationToken,
            factoryProvider: factoryProvider
        );
    }

    public async Task<TStreamingHub> ConnectAsync(StreamingHubClientOptions options, CancellationToken cancellationToken = default)
    {
        return await StreamingHubClient.ConnectAsync<TStreamingHub, TReceiver>(
            callInvokerMock,
            receiver,
            options,
            cancellationToken: cancellationToken,
            factoryProvider: factoryProvider
        );
    }

    public async Task<ReadOnlyMemory<byte>> ReadRequestRawAsync()
    {
        var requestPayload = await requestChannel.Reader.ReadAsync();
        return requestPayload.Memory;
    }

    public async Task<(int MessageId, int MethodId, T Request)> ReadRequestAsync<T>()
    {
        var requestPayload = await requestChannel.Reader.ReadAsync();
        return ReadRequestPayload<T>(requestPayload.Memory);
    }

    public async Task<(int MessageId, int MethodId, ReadOnlyMemory<byte> Request)> ReadRequestNoDeserializeAsync()
    {
        var requestPayload = await requestChannel.Reader.ReadAsync();
        return ReadRequestPayload(requestPayload.Memory);
    }

    public async Task<(int MethodId, T Request)> ReadFireAndForgetRequestAsync<T>()
    {
        var requestPayload = await requestChannel.Reader.ReadAsync();
        return ReadFireAndForgetRequestPayload<T>(requestPayload.Memory);
    }

    public void WriteResponseRaw(ReadOnlySpan<byte> data)
    {
        responseChannel.Writer.TryWrite(StreamingHubPayloadPool.Shared.RentOrCreate(data));
    }

    public void WriteResponse<T>(int messageId, int methodId, T response)
    {
        responseChannel.Writer.TryWrite(BuildResponsePayload(messageId, methodId, response));
    }

    public void WriteResponse(int messageId, int methodId, ReadOnlySpan<byte> response)
    {
        responseChannel.Writer.TryWrite(BuildResponsePayload(messageId, methodId, response));
    }

    static StreamingHubPayload BuildResponsePayload<T>(int messageId, int methodId, T response)
    {
        using var bufferWriter = ArrayPoolBufferWriter.RentThreadStaticWriter();
        var messagePackWriter = new MessagePackWriter(bufferWriter);
        messagePackWriter.WriteArrayHeader(3);
        messagePackWriter.Write(messageId);
        messagePackWriter.Write(methodId);
        MessagePackSerializer.Serialize(ref messagePackWriter, response);
        messagePackWriter.Flush();
        return StreamingHubPayloadPool.Shared.RentOrCreate(bufferWriter.WrittenSpan);
    }

    static StreamingHubPayload BuildResponsePayload(int messageId, int methodId, ReadOnlySpan<byte> response)
    {
        using var bufferWriter = ArrayPoolBufferWriter.RentThreadStaticWriter();
        var messagePackWriter = new MessagePackWriter(bufferWriter);
        messagePackWriter.WriteArrayHeader(3);
        messagePackWriter.Write(messageId);
        messagePackWriter.Write(methodId);
        messagePackWriter.Flush();
        bufferWriter.Write(response);
        return StreamingHubPayloadPool.Shared.RentOrCreate(bufferWriter.WrittenSpan);
    }

    static (int MessageId, int MethodId, T Body) ReadRequestPayload<T>(ReadOnlyMemory<byte> payload)
    {
        // Array[3][messageId (int), methodId (int), request body...]
        var messagePackReader = new MessagePackReader(payload);
        var arraySize = messagePackReader.ReadArrayHeader();
        Debug.Assert(arraySize == 3);
        var messageId = messagePackReader.ReadInt32();
        var methodId = messagePackReader.ReadInt32();
        return (messageId, methodId, MessagePackSerializer.Deserialize<T>(ref messagePackReader));
    }

    static (int MessageId, int MethodId, ReadOnlyMemory<Byte> Body) ReadRequestPayload(ReadOnlyMemory<byte> payload)
    {
        // Array[3][messageId (int), methodId (int), request body...]
        var messagePackReader = new MessagePackReader(payload);
        var arraySize = messagePackReader.ReadArrayHeader();
        Debug.Assert(arraySize == 3);
        var messageId = messagePackReader.ReadInt32();
        var methodId = messagePackReader.ReadInt32();
        return (messageId, methodId, payload.Slice((int)messagePackReader.Consumed));
    }

    static (int MethodId, T Body) ReadFireAndForgetRequestPayload<T>(ReadOnlyMemory<byte> payload)
    {
        // Array[2][methodId (int), request body...]
        var messagePackReader = new MessagePackReader(payload);
        var arraySize = messagePackReader.ReadArrayHeader();
        Debug.Assert(arraySize == 2);
        var methodId = messagePackReader.ReadInt32();
        return (methodId, MessagePackSerializer.Deserialize<T>(ref messagePackReader));
    }
}
