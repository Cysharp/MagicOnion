using MagicOnion.Server.Hubs;

namespace MagicOnion.Server;

public class MagicOnionServiceDefinition
{
    public IReadOnlyList<MethodHandler> MethodHandlers { get; }
    public IReadOnlyList<StreamingHubHandler> StreamingHubHandlers { get; }

    public MagicOnionServiceDefinition(IReadOnlyList<MethodHandler> handlers, IReadOnlyList<StreamingHubHandler> streamingHubHandlers)
    {
        this.MethodHandlers = handlers;
        this.StreamingHubHandlers = streamingHubHandlers;
    }
}
