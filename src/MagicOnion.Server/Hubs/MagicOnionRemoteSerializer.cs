using System.Buffers;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MessagePack;
using Multicaster.Remoting;

namespace MagicOnion.Server.Hubs;

internal class MagicOnionRemoteSerializer : IRemoteSerializer
{
    readonly IMagicOnionSerializer serializer;

    public MagicOnionRemoteSerializer(IMagicOnionSerializer serializer)
    {
        this.serializer = serializer;
    }

    public void Serialize<T>(IBufferWriter<byte> bufferWriter, T value, Multicaster.Remoting.SerializationContext ctx)
    {
        var writer = new MessagePackWriter(bufferWriter);
        writer.WriteArrayHeader(2);
        writer.WriteInt32(FNV1A32.GetHashCode(ctx.MethodName));
        writer.Flush();
        serializer.Serialize(bufferWriter, value);
    }
}
