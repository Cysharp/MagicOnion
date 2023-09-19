using System;
using System.ComponentModel;

namespace MagicOnion.Internal
{
    // Pubternal
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class RawBytesBox
    {
        public ReadOnlyMemory<byte> Bytes { get; }

        public RawBytesBox(ReadOnlyMemory<byte> bytes)
        {
            Bytes = bytes;
        }
    }
}
