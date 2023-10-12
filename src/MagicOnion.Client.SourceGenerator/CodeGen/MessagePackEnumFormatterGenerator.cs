using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen;

internal class MessagePackEnumFormatterGenerator
{
    public static string Build(string @namespace, IEnumerable<EnumSerializationInfo> enumSerializationInfoSet)
    {
        var writer = new StringWriter();
        writer.WriteLine($$"""
            // <auto-generated />
            #pragma warning disable CS0618 // 'member' is obsolete: 'text'
            #pragma warning disable CS0612 // 'member' is obsolete

            """);

        writer.WriteLine($$"""
            namespace {{@namespace}}
            """);
        writer.WriteLine($$"""
            {
                using global::System;
                using global::MessagePack;

            """);
        foreach (var info in enumSerializationInfoSet)
        {
            writer.WriteLine($$"""
                public sealed class {{info.FormatterName}} : global::MessagePack.Formatters.IMessagePackFormatter<{{info.FullName}}>
                {
                    public void Serialize(ref global::MessagePack.MessagePackWriter writer, {{info.FullName}} value, global::MessagePack.MessagePackSerializerOptions options)
                    {
                        writer.Write(({{info.UnderlyingType}})value);
                    }
                    
                    public {{info.FullName}} Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
                    {
                        return ({{info.FullName}})reader.Read{{info.UnderlyingType}}();
                    }
                }
            """);
        }
        writer.WriteLine($$"""
            }
            """);

        return writer.ToString();
    }
}