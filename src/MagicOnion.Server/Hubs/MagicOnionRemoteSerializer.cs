using System.Buffers;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MessagePack;
using Microsoft.Extensions.Options;
using Multicaster.Remoting;

namespace MagicOnion.Server.Hubs;

internal class MagicOnionRemoteSerializer : IRemoteSerializer
{
    readonly IMagicOnionSerializer serializer;

    public MagicOnionRemoteSerializer(IOptions<MagicOnionOptions> options)
    {
        this.serializer = options.Value.MessageSerializer.Create(MethodType.DuplexStreaming, null);
    }

    public void SerializeArgument<T>(IBufferWriter<byte> bufferWriter, T value, in Multicaster.Remoting.SerializationContext ctx)
    {
        var writer = new MessagePackWriter(bufferWriter);
        if (ctx.MessageId is { } messageId)
        {
            // client result call/request: [type(0), 0, <MethodId>, <ClientResultMessageId>, <Arguments[]>]
            writer.WriteArrayHeader(5);
            writer.WriteInt8(0);
            writer.WriteInt8(0);
            writer.WriteInt32(FNV1A32.GetHashCode(ctx.MethodName));
            MessagePackSerializer.Serialize(ref writer, messageId);
            writer.Flush();
            serializer.Serialize(bufferWriter, value);
        }
        else
        {
            // [<MethodId>, <Arguments[]>]
            writer.WriteArrayHeader(2);
            writer.WriteInt32(FNV1A32.GetHashCode(ctx.MethodName));
            writer.Flush();
            serializer.Serialize(bufferWriter, value);
        }
    }

    public void SerializeArgument<T1, T2>(IBufferWriter<byte> bufferWriter, T1 arg1, T2 arg2, in Multicaster.Remoting.SerializationContext ctx)
        => SerializeArgument(bufferWriter, new DynamicArgumentTuple<T1, T2>(arg1, arg2), ctx);

    public T DeserializeResponse<T>(ReadOnlySequence<byte> data, in Multicaster.Remoting.SerializationContext ctx)
    {
        return serializer.Deserialize<T>(data);
    }
}
