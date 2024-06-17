using System;
using System.Buffers;

namespace MagicOnion.Internal.Buffers
{
    public class MemoryPoolBufferWriter : IBufferWriter<byte>
    {
        readonly MemoryPool<byte> memoryPool;
        IMemoryOwner<byte>? buffer;
        int written;

        [ThreadStatic]
        static MemoryPoolBufferWriter? shared;
        public static MemoryPoolBufferWriter RentThreadStaticWriter() => shared ??= new MemoryPoolBufferWriter(MemoryPool<byte>.Shared);

        public MemoryPoolBufferWriter(MemoryPool<byte> memoryPool)
        {
            this.memoryPool = memoryPool;
            this.buffer = null;
            this.written = 0;
        }

        public void Advance(int count)
        {
            written += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (buffer != null && (buffer.Memory.Length - written) > sizeHint)
            {
                return buffer.Memory.Slice(written);
            }
            else
            {
                if (buffer == null)
                {
                    // New
                    buffer = memoryPool.Rent(sizeHint > 0 ? sizeHint : 32767);
                }
                else
                {
                    // Grow
                    var oldBuffer = buffer;
                    var newBuffer = memoryPool.Rent(buffer.Memory.Length * 2);

                    oldBuffer.Memory.Slice(0, written).CopyTo(newBuffer.Memory);
                    oldBuffer.Dispose();

                    buffer = newBuffer;
                }
                return buffer.Memory.Slice(written);
            }
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetMemory(sizeHint).Span;
        }

        public (IMemoryOwner<byte> Owner, int Written) ToMemoryOwnerAndReturn()
        {
            var result = (buffer ?? memoryPool.Rent(0), written);
            written = 0;
            buffer = null;
            return result;
        }
    }
}
