using System;
using System.Buffers;

namespace MagicOnion.Utils
{
    internal sealed class ArrayPoolBufferWriter : IBufferWriter<byte>, IDisposable
    {
        [ThreadStatic]
        static ArrayPoolBufferWriter staticInstance;

        public static ArrayPoolBufferWriter RentThreadStaticWriter()
        {
            if (staticInstance == null)
            {
                staticInstance = new ArrayPoolBufferWriter();
            }
            staticInstance.Prepare();
            return staticInstance;
        }

        const int MinimumBufferSize = 32767; // use 32k buffer.

        byte[] buffer;
        int index;

        void Prepare()
        {
            if (buffer == null)
            {
                buffer = ArrayPool<byte>.Shared.Rent(MinimumBufferSize);
            }
            index = 0;
        }

        public ReadOnlyMemory<byte> WrittenMemory => buffer.AsMemory(0, index);
        public ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, index);

        public int WrittenCount => index;

        public int Capacity => buffer.Length;

        public int FreeCapacity => buffer.Length - index;

        public void Advance(int count)
        {
            if (count < 0) throw new ArgumentException(nameof(count));
            index += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            return buffer.AsMemory(index);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            return buffer.AsSpan(index);
        }

        void CheckAndResizeBuffer(int sizeHint)
        {
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

                Span<byte> previousBuffer = oldBuffer.AsSpan(0, index);
                previousBuffer.CopyTo(buffer);
                ArrayPool<byte>.Shared.Return(oldBuffer);
            }
        }

        public void Dispose()
        {
            if (buffer == null)
            {
                return;
            }

            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;
        }
    }
}