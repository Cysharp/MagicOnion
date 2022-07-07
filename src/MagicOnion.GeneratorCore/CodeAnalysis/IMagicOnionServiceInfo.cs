namespace MagicOnion.Generator.CodeAnalysis
{
    public interface IMagicOnionServiceInfo : IMagicOnionCompileDirectiveTarget
    {
        MagicOnionTypeInfo ServiceType { get; }
    }
}