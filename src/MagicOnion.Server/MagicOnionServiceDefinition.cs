using MagicOnion.Server.Hubs;

namespace MagicOnion.Server;

public class MagicOnionServiceDefinition
{
    public IReadOnlyList<MethodHandler> MethodHandlers { get; private set; }
    public IReadOnlyList<StreamingHubHandler> StreamingHubHandlers { get; private set; }

    public MagicOnionServiceDefinition(IReadOnlyList<MethodHandler> handlers, IReadOnlyList<StreamingHubHandler> streamingHubHandlers)
    {
        this.MethodHandlers = handlers;
        this.StreamingHubHandlers = streamingHubHandlers;
    }
}
