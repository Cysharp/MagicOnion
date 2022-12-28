using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.Internal;

namespace MagicOnion.Generator.CodeGen;

internal class MessagePackFormatterResolverGenerator : ISerializerFormatterGenerator
{
    public string Build(SerializationFormatterCodeGenContext ctx)
    {
        EmitPreamble(ctx);
        EmitBody(ctx);
        EmitPostscript(ctx);

        return ctx.GetWrittenText();
    }

    static void EmitPreamble(SerializationFormatterCodeGenContext ctx)
    {
        ctx.TextWriter.WriteLines("""
        #pragma warning disable 618
        #pragma warning disable 612
        #pragma warning disable 414
        #pragma warning disable 219
        #pragma warning disable 168

        // NOTE: Disable warnings for nullable reference types.
        // `#nullable disable` causes compile error on old C# compilers (-7.3)
        #pragma warning disable 8603 // Possible null reference return.
        #pragma warning disable 8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
        #pragma warning disable 8625 // Cannot convert null literal to non-nullable reference type.
        """);
    }

    static void EmitBody(SerializationFormatterCodeGenContext ctx)
    {
        ctx.TextWriter.WriteLines($$"""
        namespace {{ctx.Namespace}}
        {
            using global::System;
            using global::MessagePack;
        """);

        using (ctx.TextWriter.BeginIndent())
        {
            EmitResolver(ctx);
            EmitGetFormatterHelper(ctx);
        }

        ctx.TextWriter.WriteLines($$"""
        }
        """);
    }

    static void EmitResolver(SerializationFormatterCodeGenContext ctx)
    {
        ctx.TextWriter.WriteLines($$"""
        public class {{ctx.InitializerName}} : global::MessagePack.IFormatterResolver
        {
            public static readonly global::MessagePack.IFormatterResolver Instance = new {{ctx.InitializerName}}();

            {{ctx.InitializerName}}() {}

            public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
                => FormatterCache<T>.formatter;

            static class FormatterCache<T>
            {
                public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

                static FormatterCache()
                {
                    var f = {{ctx.InitializerName}}GetFormatterHelper.GetFormatter(typeof(T));
                    if (f != null)
                    {
                        formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                    }
                }
            }
        }
        """);
    }

    static void EmitGetFormatterHelper(SerializationFormatterCodeGenContext ctx)
    {
        ctx.TextWriter.WriteLines($$"""
        internal static class {{ctx.InitializerName}}GetFormatterHelper
        {
            static readonly global::System.Collections.Generic.Dictionary<global::System.Type, int> lookup;

            static {{ctx.InitializerName}}GetFormatterHelper()
            {
                lookup = new global::System.Collections.Generic.Dictionary<global::System.Type, int>({{ctx.FormatterRegistrations.Count}})
                {
        """);
        using (ctx.TextWriter.BeginIndent())
        {
            using (ctx.TextWriter.BeginIndent())
            {
                using (ctx.TextWriter.BeginIndent())
                {
                    foreach (var (resolverInfo, index) in ctx.FormatterRegistrations.Select((x, i) => (x, i)))
                    {
                        using (ctx.TextWriter.IfDirective(string.Join(" || ", resolverInfo.IfDirectiveConditions.Select(y => $"({y})"))))
                        {
                            ctx.TextWriter.WriteLines($$"""
                        {typeof({{resolverInfo.FullName}}), {{index}} },
                        """);
                        }
                    }
                } // lookup = new ...
                ctx.TextWriter.WriteLine("};");
            } // static {{ctx.ResolverName}}GetFormatterHelper()
            ctx.TextWriter.WriteLine("}");

            ctx.TextWriter.WriteLines($$"""
            internal static object GetFormatter(Type t)
            {
                int key;
                if (!lookup.TryGetValue(t, out key))
                {
                    return null;
                }

                switch (key)
                {
            """);
            using (ctx.TextWriter.BeginIndent())
            {
                using (ctx.TextWriter.BeginIndent())
                {
                    foreach (var (resolverInfo, index) in ctx.FormatterRegistrations.Select((x, i) => (x, i)))
                    {
                        using (ctx.TextWriter.IfDirective(string.Join(" || ", resolverInfo.IfDirectiveConditions.Select(y => $"({y})"))))
                        {
                            ctx.TextWriter.WriteLines($$"""
                            case {{index}}: return new {{(resolverInfo.FormatterName.StartsWith("global::") ? resolverInfo.FormatterName : (string.IsNullOrWhiteSpace(ctx.FormatterNamespace) ? "" : ctx.FormatterNamespace + ".") + resolverInfo.FormatterName)}};
                            """);
                        }
                    }
                    ctx.TextWriter.WriteLine("default: return null;");
                } // switch (key)
                ctx.TextWriter.WriteLine("}");
            } // internal static object GetFormatter(Type t)
            ctx.TextWriter.WriteLine("}");
        }
        ctx.TextWriter.WriteLine("}");
    }

    static void EmitPostscript(SerializationFormatterCodeGenContext ctx)
    {
        ctx.TextWriter.WriteLines("""
        #pragma warning restore 168
        #pragma warning restore 219
        #pragma warning restore 414
        #pragma warning restore 612
        #pragma warning restore 618
        """);
    }
}
