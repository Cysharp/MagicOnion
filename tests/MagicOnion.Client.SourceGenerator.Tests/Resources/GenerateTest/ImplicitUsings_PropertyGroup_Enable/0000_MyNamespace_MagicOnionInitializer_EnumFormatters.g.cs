﻿// <auto-generated />
#pragma warning disable

namespace MyNamespace
{
    using global::System;
    using global::MessagePack;

    partial class MagicOnionInitializer
    {
        static partial class MessagePackEnumFormatters
        {
            public sealed class FileModeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.IO.FileMode>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::System.IO.FileMode value, global::MessagePack.MessagePackSerializerOptions options)
                {
                    writer.Write((Int32)value);
                }
                
                public global::System.IO.FileMode Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
                {
                    return (global::System.IO.FileMode)reader.ReadInt32();
                }
            }
            public sealed class ClientCertificateOptionFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.Net.Http.ClientCertificateOption>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::System.Net.Http.ClientCertificateOption value, global::MessagePack.MessagePackSerializerOptions options)
                {
                    writer.Write((Int32)value);
                }
                
                public global::System.Net.Http.ClientCertificateOption Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
                {
                    return (global::System.Net.Http.ClientCertificateOption)reader.ReadInt32();
                }
            }
            public sealed class ApartmentStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.Threading.ApartmentState>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::System.Threading.ApartmentState value, global::MessagePack.MessagePackSerializerOptions options)
                {
                    writer.Write((Int32)value);
                }
                
                public global::System.Threading.ApartmentState Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
                {
                    return (global::System.Threading.ApartmentState)reader.ReadInt32();
                }
            }
            public sealed class TaskCreationOptionsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.Threading.Tasks.TaskCreationOptions>
            {
                public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::System.Threading.Tasks.TaskCreationOptions value, global::MessagePack.MessagePackSerializerOptions options)
                {
                    writer.Write((Int32)value);
                }
                
                public global::System.Threading.Tasks.TaskCreationOptions Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
                {
                    return (global::System.Threading.Tasks.TaskCreationOptions)reader.ReadInt32();
                }
            }
        }
    }
}
