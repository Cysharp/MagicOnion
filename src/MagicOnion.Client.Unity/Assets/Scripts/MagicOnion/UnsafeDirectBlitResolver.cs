using MessagePack;
using System.Buffers;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicOnion
{
    public class UnsafeDirectBlitResolver : IFormatterResolver
    {
        public static readonly UnsafeDirectBlitResolver Instance = new UnsafeDirectBlitResolver();

        bool isFreezed = false;
        Dictionary<Type, object> formatters = new Dictionary<Type, object>();

        UnsafeDirectBlitResolver()
        {
        }

        public void Register<T>()
            where T : unmanaged
        {
            if (isFreezed)
            {
                throw new InvalidOperationException("Register must call on startup(before use GetFormatter<T>).");
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
                Instance.isFreezed = true;

                var t = typeof(T);

                object formatterObject;
                if (Instance.formatters.TryGetValue(t, out formatterObject))
                {
                    formatter = (IMessagePackFormatter<T>)formatterObject;
                }
            }
        }
    }

    public class UnsafeDirectBlitArrayFormatter<T> : IMessagePackFormatter<T[]>
        where T : unmanaged
    {
        const int TypeCode = 45;

        // use ext instead of ArrayFormatter for extremely boostup performance.
        // Layout: [extHeader, isLittlEendian(bool), bytes()]

        readonly int StructLength;

        public UnsafeDirectBlitArrayFormatter()
        {
            this.StructLength = Unsafe.SizeOf<T>();
        }

        public void Serialize(ref MessagePackWriter writer, T[] value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            var byteLen = value.Length * StructLength;
            writer.WriteExtensionFormatHeader(new ExtensionHeader(TypeCode, byteLen + 1));
            writer.Write(BitConverter.IsLittleEndian);

            /*
            Span<byte> span = writer.GetSpan();
            Unsafe.CopyBlockUnaligned(ref span[0], ref Unsafe.As<T, byte>(ref value[0]), (uint)byteLen);
            writer.Advance(byteLen);
            */

            // currently can not write directly, wait msgpack update.
            var span = System.Buffers.ArrayPool<byte>.Shared.Rent(byteLen);
            try
            {
                Unsafe.CopyBlockUnaligned(ref span[0], ref Unsafe.As<T, byte>(ref value[0]), (uint)byteLen);
                writer.WriteRaw(span.AsSpan().Slice(0, byteLen));
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(span);
            }
        }

        public T[] Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                return null;
            }

            var ext = reader.ReadExtensionFormat();
            if (ext.Header.TypeCode != TypeCode) throw new InvalidOperationException("Invalid typeCode.");

            var extReader = new MessagePackReader(ext.Data);
            var isLittleEndian = extReader.ReadBoolean();

            // extReader.read
            byte[] rentMemory = default;
            ReadOnlySpan<byte> span = default;
            {
                var seqSlice = extReader.Sequence.Slice(extReader.Position);
                if (isLittleEndian != BitConverter.IsLittleEndian)
                {
                    rentMemory = ArrayPool<byte>.Shared.Rent((int)seqSlice.Length);
                    seqSlice.CopyTo(rentMemory);
                    Array.Reverse(rentMemory);
                    span = rentMemory;
                }
                else
                {
                    if (seqSlice.IsSingleSegment)
                    {
                        span = seqSlice.First.Span;
                    }
                    else
                    {
                        rentMemory = ArrayPool<byte>.Shared.Rent((int)seqSlice.Length);
                        seqSlice.CopyTo(rentMemory);
                        span = rentMemory;
                    }
                }
            }

            var result = new T[span.Length / StructLength];
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref result[0]), ref MemoryMarshal.GetReference(span), (uint)span.Length);
            return result;
        }
    }

    public class UnsafeDirectBlitFormatter<T> : IMessagePackFormatter<T>
        where T : unmanaged
    {
        readonly int size;

        public UnsafeDirectBlitFormatter()
        {
            // Note: check is T blittable?
            this.size = Unsafe.SizeOf<T>();
        }

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            // fix to v2.0-stable
            byte[] rentMemory = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                var span = rentMemory.AsSpan().Slice(0, size);
                Unsafe.WriteUnaligned(ref span[0], value);
                writer.Write(span);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentMemory);
            }
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var segment = reader.ReadBytes();
            if (segment == null)
            {
                throw new InvalidOperationException("data is null");
            }

            ValidateRead(segment.Value.Length);

            if (segment.Value.IsSingleSegment)
            {
                var firstMemory = segment.Value.First;
                return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(firstMemory.Span));
            }
            else
            {
                var memory = segment.Value.ToArray();
                return Unsafe.ReadUnaligned<T>(ref memory[0]);
            }
        }

        void ValidateRead(long length)
        {
            if (length < size)
            {
                throw new InvalidOperationException("Overflow");
            }
        }
    }
}
