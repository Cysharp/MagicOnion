using MagicOnion.Generator.CodeAnalysis;

namespace MagicOnion.Generator.CodeGen;

public partial class HubTemplate
{
    public string Namespace { get; set; }
    public IReadOnlyList<MagicOnionStreamingHubInfo> Hubs { get; set; }
}

public partial class RegisterTemplate
{
    public string Namespace { get; set; }
    public bool DisableAutoRegisterOnInitialize { get; set; }
    public IReadOnlyList<MagicOnionServiceInfo> Services { get; set; }
    public IReadOnlyList<MagicOnionStreamingHubInfo> Hubs { get; set; }
}

public partial class EnumTemplate
{
    public string Namespace { get; set; }
    public IReadOnlyList<EnumSerializationInfo> EnumSerializationInfos { get; set; }
}
