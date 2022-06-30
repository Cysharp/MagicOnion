namespace MagicOnion.GeneratorCore.CodeAnalysis
{
    public interface IMagicOnionCompileDirectiveTarget
    {
        string IfDirectiveCondition { get; }
        bool HasIfDirectiveCondition { get; }
    }
}