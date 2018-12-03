using MessagePack;
using MessagePack.Formatters;
using System;
using System.Runtime.CompilerServices;

namespace MagicOnion.Redis
{
    internal static class NativeGuidArrayFormatter
    {
        static readonly IMessagePackFormatter<Guid> formatter = BinaryGuidFormatter.Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Serialize(ref byte[] bytes, int offset, Guid[] value)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var start = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                offset += formatter.Serialize(ref bytes, offset, value[i], null);
            }
            return offset - start;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid[] Deserialize(byte[] bytes, int offset, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var start = offset;
            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var result = new Guid[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = formatter.Deserialize(bytes, offset, null, out readSize);
                offset += readSize;
            }
            readSize = offset - start;
            return result;
        }
    }
}