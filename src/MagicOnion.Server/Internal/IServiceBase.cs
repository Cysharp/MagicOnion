using MagicOnion.Server.Diagnostics;

namespace MagicOnion.Server.Internal;

internal interface IServiceBase
{
    ServiceContext Context { get; set; }
    MagicOnionMetrics Metrics { get; set; }
}
