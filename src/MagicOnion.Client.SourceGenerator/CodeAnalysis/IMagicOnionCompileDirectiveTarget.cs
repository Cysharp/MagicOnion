namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

public interface IMagicOnionCompileDirectiveTarget
{
    string IfDirectiveCondition { get; }
    bool HasIfDirectiveCondition { get; }
}
