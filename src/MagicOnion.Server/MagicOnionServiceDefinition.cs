using MagicOnion.Server.Hubs;

namespace MagicOnion.Server;

public class MagicOnionServiceDefinition
{
    public IReadOnlyList<Type> TargetTypes { get; }

    public MagicOnionServiceDefinition(IReadOnlyList<Type> targetTypes)
    {
        this.TargetTypes = targetTypes;
    }
}
