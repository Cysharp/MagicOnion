using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen;

public partial class RegisterTemplate
{
    public string Namespace { get; init; } = default!;
    public bool DisableAutoRegisterOnInitialize { get; init; } = default!;
    public IReadOnlyList<MagicOnionServiceInfo> Services { get; init; } = default!;
    public IReadOnlyList<MagicOnionStreamingHubInfo> Hubs { get; init; } = default!;
}

public partial class EnumTemplate
{
    public string Namespace { get; init; } = default!;
    public IReadOnlyList<EnumSerializationInfo> EnumSerializationInfos { get; init; } = default!;
}
