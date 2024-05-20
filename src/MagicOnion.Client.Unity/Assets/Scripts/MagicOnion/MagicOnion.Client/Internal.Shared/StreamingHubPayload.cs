#nullable enable
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MagicOnion.Internal
{
    internal class StreamingHubPayload : IStreamingHubPayload
    {
        byte[]? buffer;
        ReadOnlyMemory<byte>? memory;

        public int Length => memory!.Value.Length;
        public ReadOnlySpan<byte> Span => memory!.Value.Span;
        public ReadOnlyMemory<byte> Memory => memory!.Value;

        void IStreamingHubPayload.Initialize(ReadOnlySpan<byte> data)
        {
            ThrowIfUsing();

            buffer = ArrayPool<byte>.Shared.Rent(data.Length);
            data.CopyTo(buffer);
            memory = buffer.AsMemory(0, (int)data.Length);
        }

        void IStreamingHubPayload.Initialize(ReadOnlySequence<byte> data)
        {
            ThrowIfUsing();
            if (data.Length > int.MaxValue) throw new InvalidOperationException("A body size of StreamingHubPayload must be less than int.MaxValue");

            buffer = ArrayPool<byte>.Shared.Rent((int)data.Length);
            data.CopyTo(buffer);
            memory = buffer.AsMemory(0, (int)data.Length);
        }

        void IStreamingHubPayload.Initialize(ReadOnlyMemory<byte> data)
        {
            ThrowIfUsing();

            buffer = null;
            memory = data;
        }

        void IStreamingHubPayload.Uninitialize()
        {
            ThrowIfDisposed();

            if (buffer != null)
            {
#if DEBUG && NET6_0_OR_GREATER
                Array.Fill<byte>(buffer, 0xff);
#endif
                ArrayPool<byte>.Shared.Return(buffer);
            }

            memory = null;
            buffer = null;
        }

#if NON_UNITY && !NETSTANDARD2_0 && !NETSTANDARD2_1
        [MemberNotNull(nameof(memory))]
#endif
        void ThrowIfDisposed()
        {
            if (memory is null) throw new ObjectDisposedException(nameof(StreamingHubPayload));
        }

        void ThrowIfUsing()
        {
            if (memory is not null) throw new InvalidOperationException(nameof(StreamingHubPayload));
        }
    }

    internal interface IStreamingHubPayload
    {
        void Initialize(ReadOnlySpan<byte> data);
        void Initialize(ReadOnlySequence<byte> data);
        void Initialize(ReadOnlyMemory<byte> data);
        void Uninitialize();
    }
}
