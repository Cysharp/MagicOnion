using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MagicOnion.GeneratorCore.CodeAnalysis
{
    [DebuggerDisplay("{ToDisplayName(DisplayNameFormat.Short),nq}")]
    public class MagicOnionTypeInfo : IEquatable<MagicOnionTypeInfo>
    {
        public static class KnownTypes
        {
            // ReSharper disable InconsistentNaming
            public static MagicOnionTypeInfo System_Void { get; } = new MagicOnionTypeInfo("System", "Void");
            public static MagicOnionTypeInfo System_String { get; } = new MagicOnionTypeInfo("System", "String");
            public static MagicOnionTypeInfo System_Boolean { get; } = new MagicOnionTypeInfo("System", "Boolean");
            public static MagicOnionTypeInfo MessagePack_Nil { get; } = new MagicOnionTypeInfo("MessagePack", "Nil");
            public static MagicOnionTypeInfo System_Threading_Tasks_Task { get; } = new MagicOnionTypeInfo("System.Threading.Tasks", "Task");
            // ReSharper restore InconsistentNaming
        }

        public string Namespace { get; }
        public string Name { get; }
        public IReadOnlyList<MagicOnionTypeInfo> GenericArguments { get; }
        public bool HasGenericArguments => GenericArguments.Any();

        public string FullName
            => ToDisplayName(DisplayNameFormat.FullyQualified);
        public string FullNameOpenType
            => ToDisplayName(DisplayNameFormat.FullyQualified | DisplayNameFormat.OpenGenerics);

        [Flags]
        public enum DisplayNameFormat
        {
            Short = 0,
            Global = 1,
            Namespace = 1 << 1,
            OpenGenerics = 1 << 2,
            FullyQualified = Namespace | Global,
        }

        public string ToDisplayName(DisplayNameFormat format = DisplayNameFormat.Short)
            => $"{(format.HasFlag(DisplayNameFormat.Global) ? "global::" : "")}{(format.HasFlag(DisplayNameFormat.Namespace) && !string.IsNullOrWhiteSpace(Namespace) ? Namespace + "." : "")}{Name}{(GenericArguments.Any() ? "<" + (format.HasFlag(DisplayNameFormat.OpenGenerics) ? new string(',', GenericArguments.Count - 1) : string.Join(", ", GenericArguments.Select(x => x.ToDisplayName(format)))) + ">" : "")}";

        public static MagicOnionTypeInfo Create(string @namespace, string name, params MagicOnionTypeInfo[] genericArguments)
        {
            if (@namespace == "MessagePack" && name == "Nil") return KnownTypes.MessagePack_Nil;
            if (@namespace == "System" && name == "String") return KnownTypes.System_String;
            if (@namespace == "System" && name == "Boolean") return KnownTypes.System_Boolean;
            if (@namespace == "System.Threading.Tasks" && name == "Task" && genericArguments.Length == 0) return KnownTypes.System_Threading_Tasks_Task;

            return new MagicOnionTypeInfo(@namespace, name, genericArguments);
        }

        public static MagicOnionTypeInfo CreateFromSymbol(ITypeSymbol symbol)
        {
            if (symbol is INamedTypeSymbol namedTypeSymbol)
            {
                var @namespace = symbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : symbol.ContainingNamespace.ToDisplayString();
                var name = symbol.Name;
                var typeArguments = namedTypeSymbol.TypeArguments.OfType<INamedTypeSymbol>().Select(MagicOnionTypeInfo.CreateFromSymbol).ToArray();

                return Create(@namespace, name, typeArguments);
            }
            else
            {
                throw new InvalidOperationException("The specified type symbol is unnamed type symbol. Generator cannot handle it.");
            }
        }

        protected MagicOnionTypeInfo(string @namespace, string name, params MagicOnionTypeInfo[] genericArguments)
        {
            Namespace = @namespace;
            Name = name;
            GenericArguments = genericArguments ?? Array.Empty<MagicOnionTypeInfo>();
        }

        public bool Equals(MagicOnionTypeInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FullName == other.FullName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MagicOnionTypeInfo)obj);
        }

        public override int GetHashCode() => FullName.GetHashCode();
    }
}