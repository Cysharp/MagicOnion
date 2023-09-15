using System;

namespace MagicOnion
{
    public sealed class RawBytesBox
    {
        public ReadOnlyMemory<byte> Bytes { get; }

        public RawBytesBox(ReadOnlyMemory<byte> bytes)
        {
            Bytes = bytes;
        }
    }
}

