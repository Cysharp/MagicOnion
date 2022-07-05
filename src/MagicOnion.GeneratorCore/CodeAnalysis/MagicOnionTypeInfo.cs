using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator.CodeAnalysis
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
        public bool IsArray { get; }
        public int ArrayRank { get; }
        public MagicOnionTypeInfo GetElementType() => IsArray ? Create(Namespace, Name, GenericArguments.ToArray()) : throw new ArgumentException($"The type '{FullName}' is not an array.");

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
            => $"{(format.HasFlag(DisplayNameFormat.Global) ? "global::" : "")}{(format.HasFlag(DisplayNameFormat.Namespace) && !string.IsNullOrWhiteSpace(Namespace) ? Namespace + "." : "")}{Name}{(GenericArguments.Any() ? "<" + (format.HasFlag(DisplayNameFormat.OpenGenerics) ? new string(',', GenericArguments.Count - 1) : string.Join(", ", GenericArguments.Select(x => x.ToDisplayName(format)))) + ">" : "")}{(IsArray ? $"[{(ArrayRank > 1 ? new string(',', ArrayRank - 1) : "")}]" : "")}";

        public static MagicOnionTypeInfo Create(string @namespace, string name, params MagicOnionTypeInfo[] genericArguments)
        {
            if (@namespace == "MessagePack" && name == "Nil") return KnownTypes.MessagePack_Nil;
            if (@namespace == "System" && name == "String") return KnownTypes.System_String;
            if (@namespace == "System" && name == "Boolean") return KnownTypes.System_Boolean;
            if (@namespace == "System.Threading.Tasks" && name == "Task" && genericArguments.Length == 0) return KnownTypes.System_Threading_Tasks_Task;

            return new MagicOnionTypeInfo(@namespace, name, isArray: false, arrayRank:0, genericArguments);
        }

        public static MagicOnionTypeInfo CreateArray(string @namespace, string name, params MagicOnionTypeInfo[] genericArguments)
            => CreateArray(@namespace, name, genericArguments, 1);

        public static MagicOnionTypeInfo CreateArray(string @namespace, string name, MagicOnionTypeInfo[] genericArguments, int arrayRank)
        {
            if (arrayRank < 1) throw new ArgumentOutOfRangeException(nameof(arrayRank));
            return new MagicOnionTypeInfo(@namespace, name, isArray: true, arrayRank:arrayRank, genericArguments);
        }

        public static MagicOnionTypeInfo CreateFromSymbol(ITypeSymbol symbol)
        {
            var isArray = false;
            var arrayRank = 0;

            var finalSymbol = symbol;
            if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                finalSymbol = arrayTypeSymbol.ElementType;
                arrayRank = arrayTypeSymbol.Rank;
                isArray = true;
            }

            if (finalSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                var @namespace = finalSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : finalSymbol.ContainingNamespace.ToDisplayString();
                var name = finalSymbol.Name;
                var typeArguments = namedTypeSymbol.TypeArguments.Select(MagicOnionTypeInfo.CreateFromSymbol).ToArray();

                return isArray ? CreateArray(@namespace, name, typeArguments, arrayRank) : Create(@namespace, name, typeArguments);
            }
            else if (isArray && finalSymbol is IArrayTypeSymbol)
            {
                // T[][]
                // NOTE: MagicOnionTypeInfo has limited support for a jagged array.
                var jaggedCount = 0;
                while (finalSymbol is IArrayTypeSymbol jaggedArrayTypeSymbol)
                {
                    finalSymbol = jaggedArrayTypeSymbol.ElementType;
                    jaggedCount++;
                }

                var @namespace = finalSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : finalSymbol.ContainingNamespace.ToDisplayString();
                var name = finalSymbol.Name + string.Concat(Enumerable.Repeat("[]", jaggedCount));
                return CreateArray(@namespace, name, Array.Empty<MagicOnionTypeInfo>(), arrayRank);
            }
            else
            {
                throw new InvalidOperationException("The specified type symbol is unnamed type symbol. Generator cannot handle it.");
            }
        }

        protected MagicOnionTypeInfo(string @namespace, string name, bool isArray = false, int arrayRank = 0, MagicOnionTypeInfo[] genericArguments = null)
        {
            Namespace = @namespace;
            Name = name;
            IsArray = isArray;
            GenericArguments = genericArguments ?? Array.Empty<MagicOnionTypeInfo>();
            ArrayRank = arrayRank;
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

        public static bool operator ==(MagicOnionTypeInfo a, MagicOnionTypeInfo b) => a?.Equals(b) ?? (b is null);
        public static bool operator !=(MagicOnionTypeInfo a, MagicOnionTypeInfo b) => !(a == b);

        public override int GetHashCode() => FullName.GetHashCode();
    }
}