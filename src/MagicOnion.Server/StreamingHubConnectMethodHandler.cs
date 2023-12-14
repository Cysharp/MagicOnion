using MagicOnion.Server.Hubs;
using MagicOnion.Server.Internal;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server;

public class StreamingHubConnectMethodHandler : MethodHandler
{
    public UniqueHashDictionary<StreamingHubHandler> StreamingHubMethodHandlers { get; }
    public IGroupRepository GroupRepository { get; }

    public StreamingHubConnectMethodHandler(Type implementationType, MethodHandlerOptions handlerOptions, IReadOnlyList<StreamingHubHandler> streamingHubMethodHandlers, IGroupRepository groupRepository, IServiceProvider serviceProvider, ILogger logger)
        : base(implementationType, implementationType.GetMethod("Connect")!, "Connect", handlerOptions, serviceProvider, logger, isStreamingHub: true)
    {
        StreamingHubMethodHandlers = new UniqueHashDictionary<StreamingHubHandler>(GetStreamingHubHandlerIdPairs(streamingHubMethodHandlers));
        GroupRepository = groupRepository;
    }

    static (int, StreamingHubHandler)[] GetStreamingHubHandlerIdPairs(IReadOnlyList<StreamingHubHandler> hubHandlers)
    {
        var list = new List<(int, StreamingHubHandler)>();
        var map = new Dictionary<int, StreamingHubHandler>();
        foreach (var item in hubHandlers)
        {
            var hash = item.MethodId;
            if (map.ContainsKey(hash))
            {
                throw new InvalidOperationException($"StreamingHubHandler.MethodName found duplicate hashCode name. Please rename or use [MethodId] to avoid conflict. {map[hash]} and {item.MethodInfo.Name}");
            }
            map.Add(hash, item);
            list.Add((hash, item));
        }

        return list.ToArray();
    }
}
