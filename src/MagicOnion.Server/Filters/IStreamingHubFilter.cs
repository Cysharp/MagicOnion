using System;
using System.Threading.Tasks;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Filters;

public interface IStreamingHubFilter : IMagicOnionFilterMetadata
{
    ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next);
}