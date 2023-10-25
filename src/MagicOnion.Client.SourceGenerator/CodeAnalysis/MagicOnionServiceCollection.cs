namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

public class MagicOnionServiceCollection
{
    public IReadOnlyList<MagicOnionStreamingHubInfo> Hubs { get; }
    public IReadOnlyList<MagicOnionServiceInfo> Services { get; }

    public MagicOnionServiceCollection(IReadOnlyList<MagicOnionStreamingHubInfo> hubs, IReadOnlyList<MagicOnionServiceInfo> services)
    {
        Hubs = hubs;
        Services = services;
    }
}
