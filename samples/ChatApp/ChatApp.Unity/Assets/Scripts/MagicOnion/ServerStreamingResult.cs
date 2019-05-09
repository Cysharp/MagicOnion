using Grpc.Core;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion
{
    /// <summary>
    /// Wrapped AsyncServerStreamingCall.
    /// </summary>
    public struct ServerStreamingResult<TResponse> : IDisposable
    {
        readonly AsyncServerStreamingCall<byte[]> inner;
        readonly MarshallingAsyncStreamReader<TResponse> responseStream;

        public ServerStreamingResult(AsyncServerStreamingCall<byte[]> inner, IFormatterResolver resolver)
        {
            this.inner = inner;
            this.responseStream = new MarshallingAsyncStreamReader<TResponse>(inner.ResponseStream, resolver);
        }

        /// <summary>
        /// Async stream to read streaming responses.
        /// </summary>
        public IAsyncStreamReader<TResponse> ResponseStream
        {
            get
            {
                return responseStream;
            }
        }

        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        public Task<Metadata> ResponseHeadersAsync
        {
            get
            {
                return this.inner.ResponseHeadersAsync;
            }
        }

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Status GetStatus()
        {
            return this.inner.GetStatus();
        }

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Metadata GetTrailers()
        {
            return this.inner.GetTrailers();
        }

        /// <summary>
        /// Provides means to cleanup after the call.
        /// If the call has already finished normally (response stream has been fully read), doesn't do anything.
        /// Otherwise, requests cancellation of the call which should terminate all pending async operations associated with the call.
        /// As a result, all resources being used by the call should be released eventually.
        /// </summary>
        /// <remarks>
        /// Normally, there is no need for you to dispose the call unless you want to utilize the
        /// "Cancel" semantics of invoking <c>Dispose</c>.
        /// </remarks>
        public void Dispose()
        {
            if (this.inner != null)
            {
                this.inner.Dispose();
            }
        }
    }
}