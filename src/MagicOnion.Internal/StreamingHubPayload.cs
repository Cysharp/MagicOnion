#nullable enable
using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace MagicOnion.Internal
{
    internal class StreamingHubPayload
    {
        byte[]? buffer;
        ReadOnlyMemory<byte>? memory;

#if DEBUG
        public int Length
        {
            get
            {
                ThrowIfUninitialized();
                return memory!.Value.Length;
            }
        }

        public ReadOnlySpan<byte> Span
        {
            get
            {
                ThrowIfUninitialized();
                return memory!.Value.Span;
            }
        }

        public ReadOnlyMemory<byte> Memory
        {
            get
            {
                ThrowIfUninitialized();
                return memory!.Value;
            }
        }

#else
        public int Length => memory!.Value.Length;
        public ReadOnlySpan<byte> Span => memory!.Value.Span;
        public ReadOnlyMemory<byte> Memory => memory!.Value;
#endif

        public void Initialize(ReadOnlySpan<byte> data)
        {
            ThrowIfUsing();

            buffer = ArrayPool<byte>.Shared.Rent(data.Length);
            data.CopyTo(buffer);
            memory = buffer.AsMemory(0, (int)data.Length);
        }

        public void Initialize(ReadOnlySequence<byte> data)
        {
            ThrowIfUsing();
            if (data.Length > int.MaxValue) throw new InvalidOperationException("A body size of StreamingHubPayload must be less than int.MaxValue");

            buffer = ArrayPool<byte>.Shared.Rent((int)data.Length);
            data.CopyTo(buffer);
            memory = buffer.AsMemory(0, (int)data.Length);
        }

        public void Initialize(ReadOnlyMemory<byte> data)
        {
            ThrowIfUsing();

            buffer = null;
            memory = data;
        }

        public void Uninitialize()
        {
            ThrowIfUninitialized();

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
        void ThrowIfUninitialized()
        {
            //Debug.Assert(memory is not null);
            if (memory is null)
            {
                throw new InvalidOperationException("A StreamingHubPayload has been already uninitialized.");
            }
        }

        void ThrowIfUsing()
        {
            //Debug.Assert(memory is null);
            if (memory is not null)
            {
                throw new InvalidOperationException("A StreamingHubPayload is currently used by other caller.");
            }
        }
    }
}
