using MessagePack;
using MessagePack.Formatters;
using System;
using System.Runtime.CompilerServices;

namespace MagicOnion.Redis
{
    internal static class NativeGuidArrayFormatter
    {
        static readonly IMessagePackFormatter<Guid> formatter = NativeGuidFormatter.Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(ref MessagePackWriter writer, Guid[] value)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                formatter.Serialize(ref writer, value[i], null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid[] Deserialize(ref MessagePackReader reader)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            var len = reader.ReadArrayHeader();
            var result = new Guid[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = formatter.Deserialize(ref reader, null);
            }

            return result;
        }
    }
}