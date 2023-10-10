namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

public interface IMagicOnionServiceInfo : IMagicOnionCompileDirectiveTarget
{
    MagicOnionTypeInfo ServiceType { get; }
}
