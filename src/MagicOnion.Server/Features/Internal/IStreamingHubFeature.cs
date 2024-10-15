using System.Diagnostics.CodeAnalysis;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Features.Internal;

internal interface IStreamingHubFeature
{
    MagicOnionManagedGroupProvider GroupProvider { get; }
    IStreamingHubHeartbeatManager HeartbeatManager { get; }
    UniqueHashDictionary<StreamingHubHandler> Handlers { get; }

    bool TryGetMethod(int methodId, [NotNullWhen(true)] out StreamingHubHandler? handler);
}
