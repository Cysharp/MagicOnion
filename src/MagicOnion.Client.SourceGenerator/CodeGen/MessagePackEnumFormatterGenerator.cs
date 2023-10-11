using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen;

internal class MessagePackEnumFormatterGenerator
{
    public static string Build(string @namespace, IEnumerable<EnumSerializationInfo> enumSerializationInfoSet)
    {
        var writer = new StringWriter();
        writer.WriteLine($$"""
            #pragma warning disable 618
            #pragma warning disable 612
            #pragma warning disable 414
            #pragma warning disable 219
            #pragma warning disable 168

            """);

        writer.WriteLine($$"""
            namespace {{@namespace}}
            """);
        writer.WriteLine($$"""
            {
                using System;
                using MessagePack;

            """);
        foreach (var info in enumSerializationInfoSet)
        {
            writer.WriteLine($$"""
                public sealed class {{info.FormatterName}} : global::MessagePack.Formatters.IMessagePackFormatter<{{info.FullName}}>
                {
                    public void Serialize(ref MessagePackWriter writer, {{info.FullName}} value, MessagePackSerializerOptions options)
                    {
                        writer.Write(({{info.UnderlyingType}})value);
                    }
                    
                    public {{info.FullName}} Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
                    {
                        return ({{info.FullName}})reader.Read{{info.UnderlyingType}}();
                    }
                }
            """);
        }
        writer.WriteLine($$"""

            }

            #pragma warning restore 168
            #pragma warning restore 219
            #pragma warning restore 414
            #pragma warning restore 612
            #pragma warning restore 618
            """);

        return writer.ToString();
    }
}
