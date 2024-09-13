#nullable enable
using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace MagicOnion.Internal
{
#if DEBUG
    internal class StreamingHubPayload
    {
        readonly short version;

#if STREAMINGHUBPAYLOAD_TRACK_LOCATION
        string? payloadCreatedLocation;
        string? payloadReturnLocation;
#endif

        internal StreamingHubPayloadCore Core { get; }

        public int Length
        {
            get
            {
                ThrowIfVersionHasChanged();
                return Core.Length;
            }
        }

        public ReadOnlyMemory<byte> Memory
        {
            get
            {
                ThrowIfVersionHasChanged();
                return Core.Memory;
            }
        }

        public ReadOnlySpan<byte> Span
        {
            get
            {
                ThrowIfVersionHasChanged();
                return Core.Span;
            }
        }

        public StreamingHubPayload(StreamingHubPayloadCore core)
        {
            this.Core = core;
            this.version = core.Version;
#if STREAMINGHUBPAYLOAD_TRACK_LOCATION
            this.payloadCreatedLocation = Environment.StackTrace;
#endif
        }

        void ThrowIfVersionHasChanged()
        {
            if (Core.Version != version) throw new InvalidOperationException("StreamingHubPayload version is mismatched.");
        }

        public void MarkAsReturned()
        {
#if STREAMINGHUBPAYLOAD_TRACK_LOCATION
            payloadReturnLocation = Environment.StackTrace;
#endif
        }
    }
#else
    internal class StreamingHubPayload : StreamingHubPayloadCore
    {}
#endif

    internal class StreamingHubPayloadCore
    {
        byte[]? buffer;
        int length = -1;

#if DEBUG
        public short Version { get; private set; }
#endif

        public int Length => length;
        public ReadOnlySpan<byte> Span => buffer!.AsSpan(0, length);
        public ReadOnlyMemory<byte> Memory => buffer!.AsMemory(0, length);

        public void Initialize(ReadOnlySpan<byte> data)
        {
            ThrowIfUsing();

            buffer = ArrayPool<byte>.Shared.Rent(data.Length);
            length = data.Length;
            data.CopyTo(buffer);
        }

        public void Initialize(ReadOnlySequence<byte> data)
        {
            ThrowIfUsing();
            if (data.Length > int.MaxValue) throw new InvalidOperationException("A body size of StreamingHubPayload must be less than int.MaxValue");

            buffer = ArrayPool<byte>.Shared.Rent((int)data.Length);
            length = (int)data.Length;
            data.CopyTo(buffer);
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

            length = -1;
            buffer = null;

#if DEBUG
            Version++;
#endif
        }

        void ThrowIfUninitialized()
        {
            //Debug.Assert(memory is not null);
            if (length == -1)
            {
                throw new InvalidOperationException("A StreamingHubPayload has been already uninitialized.");
            }
        }

        void ThrowIfUsing()
        {
            //Debug.Assert(memory is null);
            if (length != -1)
            {
                throw new InvalidOperationException("A StreamingHubPayload is currently used by other caller.");
            }
        }
    }
}
