using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Filters;

/// <summary>
/// An filter that surrounds execution of the StreamingHub method.
/// </summary>
public interface IStreamingHubFilter : IMagicOnionFilterMetadata
{
    ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next);
}