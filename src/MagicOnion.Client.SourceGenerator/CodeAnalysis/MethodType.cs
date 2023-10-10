namespace MagicOnion.Generator.CodeAnalysis;

public enum MethodType
{
    Unary = 0,
    ClientStreaming = 1,
    ServerStreaming = 2,
    DuplexStreaming = 3,
    Other = 99
}
