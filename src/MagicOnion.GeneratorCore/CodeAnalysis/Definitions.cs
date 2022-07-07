using System.Collections.Generic;
using System.Linq;

namespace MagicOnion.Generator.CodeAnalysis
{
    public enum MethodType
    {
        Unary = 0,
        ClientStreaming = 1,
        ServerStreaming = 2,
        DuplexStreaming = 3,
        Other = 99
    }

    // MessagePack Definitions
    public interface IMessagePackFormatterResolverRegisterInfo
    {
        string FullName { get; }
        string FormatterName { get; }

        IReadOnlyList<string> IfDirectiveConditions { get; }
        bool HasIfDirectiveConditions { get; }
    }

    public class GenericSerializationInfo : IMessagePackFormatterResolverRegisterInfo
    {
        public string FullName { get; }

        public string FormatterName { get; }

        public IReadOnlyList<string> IfDirectiveConditions { get; }
        public bool HasIfDirectiveConditions => IfDirectiveConditions.Any();

        public GenericSerializationInfo(string fullName, string formatterName, IReadOnlyList<string> ifDirectiveConditions)
        {
            FullName = fullName;
            FormatterName = formatterName;
            IfDirectiveConditions = ifDirectiveConditions;
        }
    }

    public class EnumSerializationInfo : IMessagePackFormatterResolverRegisterInfo
    {
        public string Namespace { get; }
        public string Name { get;}
        public string FullName { get; }
        public string UnderlyingType { get; }

        public string FormatterName => Name + "Formatter()";

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
}
