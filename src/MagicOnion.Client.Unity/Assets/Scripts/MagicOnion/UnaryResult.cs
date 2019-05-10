using Grpc.Core;
using MagicOnion.CompilerServices; // require this using in AsyncMethodBuilder
using MessagePack;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MagicOnion
{
    /// <summary>
    /// Wrapped AsyncUnaryCall.
    /// </summary>
#if NON_UNITY || (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6)))
    [AsyncMethodBuilder(typeof(AsyncUnaryResultMethodBuilder<>))]
#endif
    public struct UnaryResult<TResponse>
    {
        internal readonly bool hasRawValue; // internal
        internal readonly TResponse rawValue; // internal
        internal readonly Task<TResponse> rawTaskValue; // internal

        readonly AsyncUnaryCall<byte[]> inner;
        readonly IFormatterResolver resolver;

        public UnaryResult(TResponse rawValue)
        {
            this.hasRawValue = true;
            this.rawValue = rawValue;
            this.rawTaskValue = null;
            this.inner = null;
            this.resolver = null;
        }

        public UnaryResult(Task<TResponse> rawTaskValue)
        {
            this.hasRawValue = true;
            this.rawValue = default(TResponse);
            this.rawTaskValue = rawTaskValue;
            this.inner = null;
            this.resolver = null;
        }

        public UnaryResult(AsyncUnaryCall<byte[]> inner, IFormatterResolver resolver)
        {
            this.hasRawValue = false;
            this.rawValue = default(TResponse);
            this.rawTaskValue = null;
            this.inner = inner;
            this.resolver = resolver;
        }

        async Task<TResponse> Deserialize()
        {
            var bytes = await inner.ResponseAsync.ConfigureAwait(false);
            return LZ4MessagePackSerializer.Deserialize<TResponse>(bytes, resolver);
        }

        /// <summary>
        /// Asynchronous call result.
        /// </summary>
        public Task<TResponse> ResponseAsync
        {
            get
            {
                if (!hasRawValue)
                {
                    return Deserialize();
                }
                else if (rawTaskValue != null)
                {
                    return rawTaskValue;
                }
                else
                {
                    return Task.FromResult(rawValue);
                }
            }
        }

        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        public Task<Metadata> ResponseHeadersAsync
        {
            get
            {
                return inner.ResponseHeadersAsync;
            }
        }

        /// <summary>
        /// Allows awaiting this object directly.
        /// </summary>
        public TaskAwaiter<TResponse> GetAwaiter()
        {
            return ResponseAsync.GetAwaiter();
        }

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Status GetStatus()
        {
            return inner.GetStatus();
        }

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Metadata GetTrailers()
        {
            return inner.GetTrailers();
        }

        /// <summary>
        /// Provides means to cleanup after the call.
        /// If the call has already finished normally (request stream has been completed and call result has been received), doesn't do anything.
        /// Otherwise, requests cancellation of the call which should terminate all pending async operations associated with the call.
        /// As a result, all resources being used by the call should be released eventually.
        /// </summary>
        /// <remarks>
        /// Normally, there is no need for you to dispose the call unless you want to utilize the
        /// "Cancel" semantics of invoking <c>Dispose</c>.
        /// </remarks>
        public void Dispose()
        {
            inner.Dispose();
        }
    }
}