namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

// MessagePack Definitions
public interface ISerializationFormatterRegisterInfo
{
    string FullName { get; }
    string FormatterName { get; }
    string FormatterConstructorArgs { get; }
}

public class GenericSerializationInfo : ISerializationFormatterRegisterInfo
{
    public string FullName { get; }

    public string FormatterName { get; }
    public string FormatterConstructorArgs { get; }

    public GenericSerializationInfo(string fullName, string formatterName, string formatterConstructorArgs)
    {
        FullName = fullName;
        FormatterName = formatterName;
        FormatterConstructorArgs = formatterConstructorArgs;
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

    public EnumSerializationInfo(string @namespace, string name, string fullName, string underlyingType)
    {
        Namespace = @namespace;
        Name = name;
        FullName = fullName;
        UnderlyingType = underlyingType;
    }
}

public class SerializationTypeHintInfo : ISerializationFormatterRegisterInfo
{
    public string FullName { get; }

    string ISerializationFormatterRegisterInfo.FormatterName => string.Empty; // Dummy
    string ISerializationFormatterRegisterInfo.FormatterConstructorArgs => string.Empty; // Dummy

    public SerializationTypeHintInfo(string fullName)
    {
        FullName = fullName;
    }
}
