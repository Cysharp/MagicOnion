using System.Buffers;

namespace MagicOnion.Server.Hubs;

public interface IStreamingHubHeartbeatMetadataProvider
{
    bool TryWriteMetadata(IBufferWriter<byte> writer);
}
