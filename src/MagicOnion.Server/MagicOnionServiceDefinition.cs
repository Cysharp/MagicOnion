using MagicOnion.Server.Hubs;

namespace MagicOnion.Server;

public class MagicOnionServiceDefinition
{
    public IReadOnlyList<MethodHandler> MethodHandlers { get; }
    public IReadOnlyList<StreamingHubHandler> StreamingHubHandlers { get; }
    public IReadOnlyList<Type> TargetTypes { get; }

    public MagicOnionServiceDefinition(IReadOnlyList<MethodHandler> handlers, IReadOnlyList<StreamingHubHandler> streamingHubHandlers, IReadOnlyList<Type> targetTypes)
    {
        this.MethodHandlers = handlers;
        this.StreamingHubHandlers = streamingHubHandlers;
        this.TargetTypes = targetTypes;
    }
}
