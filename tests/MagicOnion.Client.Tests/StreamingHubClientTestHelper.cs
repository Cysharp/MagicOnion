using System.Buffers;
using System.Diagnostics;
using System.Threading.Channels;

namespace MagicOnion.Client.Tests;

class StreamingHubClientTestHelper<TStreamingHub, TReceiver>
    where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    where TReceiver : class
{
    readonly Channel<byte[]> requestChannel;
    readonly Channel<byte[]> responseChannel;
    readonly CallInvoker callInvokerMock;
    readonly TReceiver receiver;
    readonly IStreamingHubClientFactoryProvider? factoryProvider;

    public TReceiver Receiver => receiver;
    public CallInvoker CallInvoker => callInvokerMock;

    public StreamingHubClientTestHelper(IStreamingHubClientFactoryProvider? factoryProvider = null)
    {
        requestChannel = Channel.CreateUnbounded<byte[]>();
        var requestStream = new ChannelClientStreamWriter<byte[]>(requestChannel);
        responseChannel = Channel.CreateUnbounded<byte[]>();
        var responseStream = new ChannelAsyncStreamReader<byte[]>(responseChannel);

        this.factoryProvider = factoryProvider;

        callInvokerMock = Substitute.For<CallInvoker>();
        callInvokerMock.AsyncDuplexStreamingCall(default(Method<byte[], byte[]>)!, default, default)
        .ReturnsForAnyArgs(x =>
        {
            return new AsyncDuplexStreamingCall<byte[], byte[]>(
                requestStream,
                responseStream,
                _ => Task.FromResult(new Metadata { { "x-magiconion-streaminghub-version", "2" } }),
                _ => Status.DefaultSuccess,
                _ => Metadata.Empty,
                _ => { },
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

    public async Task<(int MessageId, int MethodId, T Requst)> ReadRequestAsync<T>()
    {
        var requestPayload = await requestChannel.Reader.ReadAsync();
        return ReadRequestPayload<T>(requestPayload);
    }

    public async Task<(int MethodId, T Requst)> ReadFireAndForgetRequestAsync<T>()
    {
        var requestPayload = await requestChannel.Reader.ReadAsync();
        return ReadFireAndForgetRequestPayload<T>(requestPayload);
    }

    public void WriteResponse<T>(int messageId, int methodId, T response)
    {
        responseChannel.Writer.TryWrite(BuildResponsePayload(messageId, methodId, response));
    }

    static byte[] BuildResponsePayload<T>(int messageId, int methodId, T response)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var messagePackWriter = new MessagePackWriter(bufferWriter);
        messagePackWriter.WriteArrayHeader(3);
        messagePackWriter.Write(messageId);
        messagePackWriter.Write(methodId);
        MessagePackSerializer.Serialize(ref messagePackWriter, response);
        messagePackWriter.Flush();
        return bufferWriter.WrittenSpan.ToArray();
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
