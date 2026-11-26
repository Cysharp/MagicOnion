using MessagePack;
using SerializerFoundation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicOnion.Serialization.MessagePack
{
    /// <summary>Supplies formatters that serialize registered unmanaged types using their in-memory representation.</summary>
    [Obsolete("Do not use UnsafeDirectBlit because it poses a high security risk.")]
    public class UnsafeDirectBlitResolver : MessagePackFormatterFactory
    {
        /// <summary>Gets the shared direct-blit formatter factory.</summary>
        public static readonly UnsafeDirectBlitResolver Instance = new();

        readonly Dictionary<Type, MessagePackFormatterFactory> formatters = new();
        bool frozen;

        UnsafeDirectBlitResolver()
        {
        }

        /// <summary>Registers an unmanaged value type and its array type before the first formatter lookup.</summary>
        public void Register<T>() where T : unmanaged
        {
            lock (formatters)
            {
                if (frozen)
                {
                    throw new InvalidOperationException("Register must be called before the first formatter lookup.");
                }

                var factory = new UnsafeDirectBlitFormatterFactory<T>();
                formatters.Add(typeof(T), factory);
                formatters.Add(typeof(T[]), factory);
            }
        }

#if NET9_0_OR_GREATER
        /// <inheritdoc />
        public override object? CreateFormatter<TWriteBuffer, TReadBuffer>(Type type)
            => FindFactory(type)?.CreateFormatter<TWriteBuffer, TReadBuffer>(type);
#endif

        /// <inheritdoc />
        public override object? CreateFormatter(Type writeBufferType, Type readBufferType, Type valueType)
            => FindFactory(valueType)?.CreateFormatter(writeBufferType, readBufferType, valueType);

        MessagePackFormatterFactory? FindFactory(Type type)
        {
            lock (formatters)
            {
                frozen = true;
                return formatters.TryGetValue(type, out var factory) ? factory : null;
            }
        }
    }

    [Obsolete("Do not use UnsafeDirectBlit because it poses a high security risk.")]
    sealed partial class UnsafeDirectBlitFormatterFactory<T> : MessagePackFormatterFactory where T : unmanaged
    {
#if NET9_0_OR_GREATER
        public override object? CreateFormatter<TWriteBuffer, TReadBuffer>(Type type)
#else
        public object? CreateFormatter<TWriteBuffer, TReadBuffer>(Type type)
            where TWriteBuffer : struct, IWriteBuffer
            where TReadBuffer : struct, IReadBuffer
#endif
        {
            if (type == typeof(T))
            {
                return new UnsafeDirectBlitFormatter<TWriteBuffer, TReadBuffer, T>();
            }

            return type == typeof(T[]) ? new UnsafeDirectBlitArrayFormatter<TWriteBuffer, TReadBuffer, T>() : null;
        }
    }

    /// <summary>Serializes an unmanaged array as a MessagePack extension containing its native bytes.</summary>
    [Obsolete("Do not use UnsafeDirectBlit because it poses a high security risk.")]
    public sealed class UnsafeDirectBlitArrayFormatter<TWriteBuffer, TReadBuffer, T> : IMessagePackFormatter<TWriteBuffer, TReadBuffer, T[]?>
        where TWriteBuffer : struct, IWriteBuffer
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        where TReadBuffer : struct, IReadBuffer
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        where T : unmanaged
    {
        const sbyte TypeCode = 45;

        /// <inheritdoc />
        public void Initialize(MessagePackFormatterResolver resolver)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref TWriteBuffer buffer, ref SerializeState state, T[]? value)
        {
            if (value is null)
            {
                buffer.WriteNil();
                return;
            }

            var bytes = MemoryMarshal.AsBytes(value.AsSpan());
            buffer.WriteExtHeader(TypeCode, checked(bytes.Length + 1));
            buffer.WriteBoolean(BitConverter.IsLittleEndian);
            buffer.WriteRaw(bytes);
        }

        /// <inheritdoc />
        public void Deserialize(ref TReadBuffer buffer, ref DeserializeState state, ref T[]? value)
        {
            if (buffer.TryReadNil())
            {
                value = null;
                return;
            }

            var (typeCode, length) = buffer.ReadExtHeader();
            var size = Unsafe.SizeOf<T>();
            if (typeCode != TypeCode || length < 1 || (length - 1) % size != 0)
            {
                throw new MessagePackSerializationException("Invalid direct-blit array extension.");
            }

            if (buffer.ReadBoolean() != BitConverter.IsLittleEndian)
            {
                throw new MessagePackSerializationException("The direct-blit array uses a different byte order.");
            }

            var byteCount = length - 1;
            if (byteCount == 0)
            {
                value = Array.Empty<T>();
                return;
            }

            if (!buffer.TryGetSpan(byteCount, out var bytes))
            {
                throw new MessagePackSerializationException("Incomplete direct-blit array data.");
            }

            value = MemoryMarshal.Cast<byte, T>(bytes.Slice(0, byteCount)).ToArray();
            buffer.Advance(byteCount);
        }
    }

    /// <summary>Serializes an unmanaged value as a MessagePack binary value containing its native bytes.</summary>
    [Obsolete("Do not use UnsafeDirectBlit because it poses a high security risk.")]
    public sealed class UnsafeDirectBlitFormatter<TWriteBuffer, TReadBuffer, T> : IMessagePackFormatter<TWriteBuffer, TReadBuffer, T>
        where TWriteBuffer : struct, IWriteBuffer
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        where TReadBuffer : struct, IReadBuffer
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        where T : unmanaged
    {
        /// <inheritdoc />
        public void Initialize(MessagePackFormatterResolver resolver)
        {
        }

        /// <inheritdoc />
        public void Serialize(ref TWriteBuffer buffer, ref SerializeState state, T value)
        {
            var size = Unsafe.SizeOf<T>();
            buffer.WriteBinHeader(size);
            var bytes = buffer.GetSpan(size);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(bytes), value);
            buffer.Advance(size);
        }

        /// <inheritdoc />
        public void Deserialize(ref TReadBuffer buffer, ref DeserializeState state, ref T value)
        {
            var length = buffer.ReadBinHeader();
            if (length < Unsafe.SizeOf<T>() || !buffer.TryGetSpan(length, out var bytes))
            {
                throw new MessagePackSerializationException("Incomplete direct-blit value data.");
            }

            value = MemoryMarshal.Read<T>(bytes);
            buffer.Advance(length);
        }
    }
}
