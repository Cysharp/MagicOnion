using System.Buffers;
using System.Diagnostics.CodeAnalysis;

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
        ReadOnlyMemory<byte>? memory;

#if DEBUG
        public short Version { get; private set; }
#endif

        public int Length => memory!.Value.Length;
        public ReadOnlySpan<byte> Span => memory!.Value.Span;
        public ReadOnlyMemory<byte> Memory => memory!.Value;

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

        public void Initialize(ReadOnlyMemory<byte> data, bool holdReference)
        {
            ThrowIfUsing();

            if (holdReference)
            {
                buffer = null;
                memory = data;
            }
            else
            {
                buffer = ArrayPool<byte>.Shared.Rent((int)data.Length);
                data.CopyTo(buffer);
                memory = buffer.AsMemory(0, (int)data.Length);
            }
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

#if DEBUG
            Version++;
#endif
        }

#if !UNITY_2021_1_OR_NEWER && !NETSTANDARD2_0 && !NETSTANDARD2_1
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
