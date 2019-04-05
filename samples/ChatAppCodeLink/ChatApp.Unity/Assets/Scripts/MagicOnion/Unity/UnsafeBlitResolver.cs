#if ENABLE_UNSAFE_MSGPACK

using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace MagicOnion
{
    public class UnsafeDirectBlitResolver : IFormatterResolver
    {
        public static readonly UnsafeDirectBlitResolver Instance = new UnsafeDirectBlitResolver();

        static bool isFreezed = false;
        static Dictionary<Type, object> formatters = new Dictionary<Type, object>();

        UnsafeDirectBlitResolver()
        {
        }

        public static void Register<T>()
            where T : struct
        {
            if (isFreezed)
            {
                throw new InvalidOperationException("Register must call on startup(before use GetFormatter<T>).");
            }
            if (!UnsafeUtility.IsBlittable<T>())
            {
                throw new InvalidOperationException(typeof(T) + " is not blittable so can not register UnsafeBlitResolver.");
            }

            formatters.Add(typeof(T), new UnsafeDirectBlitFormatter<T>());
            formatters.Add(typeof(T[]), new UnsafeDirectBlitArrayFormatter<T>());
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                isFreezed = true;

                var t = typeof(T);

                object formatterObject;
                if (formatters.TryGetValue(t, out formatterObject))
                {
                    formatter = (IMessagePackFormatter<T>)formatterObject;
                }
            }
        }
    }

    public class UnsafeDirectBlitArrayFormatter<T> : IMessagePackFormatter<T[]>
        where T : struct
    {
        const int TypeCode = 45;

        // use ext instead of ArrayFormatter for extremely boostup performance.
        // Layout: [extHeader, byteSize(integer), isLittlEendian(bool), bytes()]

        readonly int StructLength;

        public UnsafeDirectBlitArrayFormatter()
        {
            this.StructLength = UnsafeUtility.SizeOf<T>();
        }

        public unsafe int Serialize(ref byte[] bytes, int offset, T[] value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;

            var byteLen = value.Length * StructLength;

            offset += MessagePackBinary.WriteExtensionFormatHeader(ref bytes, offset, TypeCode, byteLen);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, byteLen); // write original header(not array header)
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, BitConverter.IsLittleEndian);

            MessagePackBinary.EnsureCapacity(ref bytes, offset, byteLen);

            ulong handle2;
            var srcPointer = UnsafeUtility.PinGCArrayAndGetDataAddress(value, out handle2);
            try
            {
                fixed (void* dstPointer = &bytes[offset])
                {
                    UnsafeUtility.MemCpy(dstPointer, srcPointer, byteLen);
                }
            }
            finally
            {
                UnsafeUtility.ReleaseGCObject(handle2);
            }

            offset += byteLen;
            return offset - startOffset;
        }

        public unsafe T[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var header = MessagePackBinary.ReadExtensionFormatHeader(bytes, offset, out readSize);
            offset += readSize;

            if (header.TypeCode != TypeCode) throw new InvalidOperationException("Invalid typeCode.");

            var byteLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
            offset += readSize;

            var isLittleEndian = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
            offset += readSize;

            if (isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes, offset, byteLength);
            }

            var result = new T[byteLength / StructLength];
            ulong handle1;
            var dstPointer = UnsafeUtility.PinGCArrayAndGetDataAddress(result, out handle1);
            try
            {
                fixed (void* srcPointer = &bytes[offset])
                {
                    UnsafeUtility.MemCpy(dstPointer, srcPointer, byteLength);
                }
            }
            finally
            {
                UnsafeUtility.ReleaseGCObject(handle1);
            }

            offset += byteLength;
            readSize = offset - startOffset;
            return result;
        }
    }

    public class UnsafeDirectBlitFormatter<T> : IMessagePackFormatter<T>
        where T : struct
    {
        readonly int size;

        public UnsafeDirectBlitFormatter()
        {
            this.size = UnsafeUtility.SizeOf<T>();
        }

        public unsafe int Serialize(ref byte[] bytes, int offset, T value, IFormatterResolver formatterResolver)
        {
            var headerSize = MessagePackBinaryEx.WriteBytesHeaderWithEnsureCount(ref bytes, offset, size);

            fixed (void* p = &bytes[offset + headerSize])
            {
                UnsafeUtility.CopyStructureToPtr(ref value, p);
            }

            return headerSize + size;
        }

        public unsafe T Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            var segment = MessagePackBinary.ReadBytesSegment(bytes, offset, out readSize);
            ValidateRead(segment.Array, segment.Offset);

            T value;
            fixed (void* p = &segment.Array[segment.Offset])
            {
                UnsafeUtility.CopyPtrToStructure<T>(p, out value);
            }

            return value;
        }

        void ValidateRead(byte[] bytes, int offset)
        {
            if (bytes.Length - offset < size)
            {
                throw new InvalidOperationException("Overflow");
            }
        }
    }

    internal static class MessagePackBinaryEx
    {
        public static int WriteBytesHeaderWithEnsureCount(ref byte[] dest, int dstOffset, int count)
        {
            if (count <= byte.MaxValue)
            {
                var size = 2;
                MessagePackBinary.EnsureCapacity(ref dest, dstOffset, size + count);
                dest[dstOffset] = MessagePackCode.Bin8;
                dest[dstOffset + 1] = (byte)count;
                return size;
            }
            else if (count <= UInt16.MaxValue)
            {
                var size = 3;
                MessagePackBinary.EnsureCapacity(ref dest, dstOffset, size + count);
                unchecked
                {
                    dest[dstOffset] = MessagePackCode.Bin16;
                    dest[dstOffset + 1] = (byte)(count >> 8);
                    dest[dstOffset + 2] = (byte)(count);
                }
                return size;
            }
            else
            {
                var size = 5;
                MessagePackBinary.EnsureCapacity(ref dest, dstOffset, size + count);
                unchecked
                {
                    dest[dstOffset] = MessagePackCode.Bin32;
                    dest[dstOffset + 1] = (byte)(count >> 24);
                    dest[dstOffset + 2] = (byte)(count >> 16);
                    dest[dstOffset + 3] = (byte)(count >> 8);
                    dest[dstOffset + 4] = (byte)(count);
                }
                return size;
            }
        }
    }
}

#endif