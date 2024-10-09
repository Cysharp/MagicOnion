using MagicOnion.Internal;

namespace MagicOnion.Server.Internal;

internal interface IStreamingHubBase
{
    Task<DuplexStreamingResult<StreamingHubPayload, StreamingHubPayload>> Connect();
}
