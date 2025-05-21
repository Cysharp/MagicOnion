using System.Buffers;
using System.Diagnostics;
using System.Threading.Channels;
using MagicOnion.Internal;

namespace MagicOnion.Client.Tests;

class StreamingHubClientTestHelper<TStreamingHub, TReceiver>
    where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    where TReceiver : class
{
    readonly Channel<StreamingHubPayload> requestChannel;
    readonly Channel<StreamingHubPayload> responseChannel;
    readonly CallInvoker callInvokerMock;
    readonly TReceiver receiver;
    readonly IStreamingHubClientFactoryProvider? factoryProvider;

    public TReceiver Receiver => receiver;
    public CallInvoker CallInvoker => callInvokerMock;

    public StreamingHubClientTestHelper(IStreamingHubClientFactoryProvider? factoryProvider = null, Func<Metadata, Task>? onResponseHeaderAsync = null, Action? onDuplexStreamingCallDisposeAction = null)
    {
        requestChannel = Channel.CreateUnbounded<StreamingHubPayload>();
        var requestStream = new ChannelClientStreamWriter<StreamingHubPayload>(requestChannel);
        responseChannel = Channel.CreateUnbounded<StreamingHubPayload>();
        var responseStream = new ChannelAsyncStreamReader<StreamingHubPayload>(responseChannel);

        this.factoryProvider = factoryProvider;

        callInvokerMock = Substitute.For<CallInvoker>();
        callInvokerMock.AsyncDuplexStreamingCall(default(Method<StreamingHubPayload, StreamingHubPayload>)!, default, default)
        .ReturnsForAnyArgs(x =>
        {
            return new AsyncDuplexStreamingCall<StreamingHubPayload, StreamingHubPayload>(
                requestStream,
                responseStream,
                async _ =>
                {
                    var metadata = new Metadata { { "x-magiconion-streaminghub-version", "2" } };
                    if (onResponseHeaderAsync != null)
                    {
                        await onResponseHeaderAsync(metadata).ConfigureAwait(false);
                    }
                    return metadata;
                },
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ =>
                {
                    onDuplexStreamingCallDisposeAction?.Invoke();
                },
                new object());
        });

        receiver = Substitute.For<TReceiver>();
    }

    public static async Task<(StreamingHubClientTestHelper<TStreamingHub, TReceiver> Helper, TStreamingHub Client)> CreateAndConnectAsync(CancellationToken cancellationToken = default)
    {
        var helper = new StreamingHubClientTestHelper<TStreamingHub, TReceiver>();
        return (helper, await helper.ConnectAsync(cancellationToken));
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

    public void ThrowIOException()
    {
        responseChannel.Writer.TryComplete(new IOException("Connection reset by peer. (Simulated)"));
    }

    public void ThrowRpcException()
    {
        responseChannel.Writer.TryComplete(new RpcException(new Status(StatusCode.Aborted, "Connection has been closed.", new IOException("Connection reset by peer. (Simulated)"))));
    }

    static StreamingHubPayload BuildResponsePayload<T>(int messageId, int methodId, T response)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var messagePackWriter = new MessagePackWriter(bufferWriter);
        messagePackWriter.WriteArrayHeader(3);
        messagePackWriter.Write(messageId);
        messagePackWriter.Write(methodId);
        MessagePackSerializer.Serialize(ref messagePackWriter, response);
        messagePackWriter.Flush();
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
