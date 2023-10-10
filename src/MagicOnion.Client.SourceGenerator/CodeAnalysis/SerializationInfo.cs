namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

// MessagePack Definitions
public interface ISerializationFormatterRegisterInfo
{
    string FullName { get; }
    string FormatterName { get; }
    string FormatterConstructorArgs { get; }

    IReadOnlyList<string> IfDirectiveConditions { get; }
    bool HasIfDirectiveConditions { get; }
}

public class GenericSerializationInfo : ISerializationFormatterRegisterInfo
{
    public string FullName { get; }

    public string FormatterName { get; }
    public string FormatterConstructorArgs { get; }

    public IReadOnlyList<string> IfDirectiveConditions { get; }
    public bool HasIfDirectiveConditions => IfDirectiveConditions.Any();

    public GenericSerializationInfo(string fullName, string formatterName, string formatterConstructorArgs, IReadOnlyList<string> ifDirectiveConditions)
    {
        FullName = fullName;
        FormatterName = formatterName;
        FormatterConstructorArgs = formatterConstructorArgs;
        IfDirectiveConditions = ifDirectiveConditions;
    }
}

public class EnumSerializationInfo : ISerializationFormatterRegisterInfo
{
    public string Namespace { get; }
    public string Name { get;}
    public string FullName { get; }
    public string UnderlyingType { get; }

    public string FormatterName => $"{Name.Replace(".", "_")}Formatter";
    public string FormatterConstructorArgs => "()";

    public IReadOnlyList<string> IfDirectiveConditions { get; }
    public bool HasIfDirectiveConditions => IfDirectiveConditions.Any();

    public EnumSerializationInfo(string @namespace, string name, string fullName, string underlyingType, IReadOnlyList<string> ifDirectiveConditions)
    {
        Namespace = @namespace;
        Name = name;
        FullName = fullName;
        UnderlyingType = underlyingType;
        IfDirectiveConditions = ifDirectiveConditions;
    }
}

public class SerializationTypeHintInfo : ISerializationFormatterRegisterInfo
{
    public string FullName { get; }

    string ISerializationFormatterRegisterInfo.FormatterName => string.Empty; // Dummy
    string ISerializationFormatterRegisterInfo.FormatterConstructorArgs => string.Empty; // Dummy

    public IReadOnlyList<string> IfDirectiveConditions { get; }
    public bool HasIfDirectiveConditions => IfDirectiveConditions.Any();

    public SerializationTypeHintInfo(string fullName, IReadOnlyList<string> ifDirectiveConditions)
    {
        FullName = fullName;
        IfDirectiveConditions = ifDirectiveConditions;
    }
}
