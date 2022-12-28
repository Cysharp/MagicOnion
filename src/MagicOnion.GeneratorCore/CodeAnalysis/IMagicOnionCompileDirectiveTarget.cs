namespace MagicOnion.Generator.CodeAnalysis;

public interface IMagicOnionCompileDirectiveTarget
{
    string IfDirectiveCondition { get; }
    bool HasIfDirectiveCondition { get; }
}
