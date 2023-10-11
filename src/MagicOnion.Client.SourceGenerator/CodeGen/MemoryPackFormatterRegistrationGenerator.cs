using MagicOnion.Client.SourceGenerator.Internal;

namespace MagicOnion.Client.SourceGenerator.CodeGen;

internal class MemoryPackFormatterRegistrationGenerator : ISerializerFormatterGenerator
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
            using global::MemoryPack;
        """);

        using (ctx.TextWriter.BeginIndent())
        {
            EmitRegister(ctx);
        }

        ctx.TextWriter.WriteLines($$"""
        }
        """);
    }

    static void EmitRegister(SerializationFormatterCodeGenContext ctx)
    {
        ctx.TextWriter.WriteLines($$"""
        public class {{ctx.InitializerName}}
        {
            public static void RegisterFormatters()
            {
        """);
        using (ctx.TextWriter.BeginIndent(2))
        {
            foreach (var (resolverInfo, index) in ctx.FormatterRegistrations.Select((x, i) => (x, i)))
            {
                ctx.TextWriter.WriteLines($$"""
                global::MemoryPack.MemoryPackFormatterProvider.Register(new {{(resolverInfo.FormatterName.StartsWith("global::") || string.IsNullOrWhiteSpace(ctx.FormatterNamespace) ? "" : ctx.FormatterNamespace + ".") + resolverInfo.FormatterName}}{{resolverInfo.FormatterConstructorArgs}});
                """);
            }
        }
        ctx.TextWriter.WriteLines($$"""
            }
        }
        """);
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
