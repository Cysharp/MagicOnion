using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MagicOnion.Internal.Buffers
{
    internal sealed class ArrayPoolBufferWriter : IBufferWriter<byte>, IDisposable
    {
        [ThreadStatic]
        static ArrayPoolBufferWriter? staticInstance;

        public static ArrayPoolBufferWriter RentThreadStaticWriter()
        {
            (staticInstance ??= new ArrayPoolBufferWriter()).Prepare();

#if DEBUG
            var currentInstance = staticInstance;
            staticInstance = null;
            return currentInstance;
#else
            return staticInstance;
#endif
        }

        const int PreAllocatedBufferSize = 8192; // use 8k buffer.
        const int MinimumBufferSize = PreAllocatedBufferSize / 2;

        readonly byte[] preAllocatedBuffer = new byte[PreAllocatedBufferSize];
        byte[]? buffer;
        int index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Prepare()
        {
            buffer = preAllocatedBuffer;
            index = 0;
        }

        public ReadOnlyMemory<byte> WrittenMemory => buffer.AsMemory(0, index);
        public ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, index);

        public int WrittenCount => index;

        public int Capacity => buffer?.Length ?? throw new ObjectDisposedException(nameof(ArrayPoolBufferWriter));

        public int FreeCapacity => Capacity - index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (count < 0) throw new ArgumentException(nameof(count));
            index += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            return buffer.AsMemory(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            return buffer.AsSpan(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckAndResizeBuffer(int sizeHint)
        {
            if (buffer == null) throw new ObjectDisposedException(nameof(ArrayPoolBufferWriter));
            if (sizeHint < 0) throw new ArgumentException(nameof(sizeHint));

            if (sizeHint == 0)
            {
                sizeHint = MinimumBufferSize;
            }

            int availableSpace = buffer.Length - index;

            if (sizeHint > availableSpace)
            {
                int growBy = Math.Max(sizeHint, buffer.Length);
                int newSize = checked(buffer.Length + growBy);

                byte[] oldBuffer = buffer;

                buffer = ArrayPool<byte>.Shared.Rent(newSize);
                oldBuffer.AsSpan(0, index).CopyTo(buffer);

                if (oldBuffer != preAllocatedBuffer)
                {
                    ArrayPool<byte>.Shared.Return(oldBuffer);
                }
            }
        }

        public void Dispose()
        {
            if (buffer != preAllocatedBuffer && buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            buffer = null;

#if DEBUG
            Debug.Assert(staticInstance is null);
            staticInstance = this;
#if NETSTANDARD2_1 || NET6_0_OR_GREATER
            Array.Fill<byte>(preAllocatedBuffer, 0xff);
#endif
#endif
        }
    }
}
