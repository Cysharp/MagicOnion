using Grpc.Core;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace MagicOnion
{
    /// <summary>
    /// Wrapped AsyncDuplexStreamingCall.
    /// </summary>
    public struct DuplexStreamingResult<TRequest, TResponse> : IDisposable
    {
        readonly AsyncDuplexStreamingCall<byte[], byte[]> inner;
        readonly IClientStreamWriter<TRequest> requestStream;
        readonly IAsyncStreamReader<TResponse> responseStream;

        public DuplexStreamingResult(AsyncDuplexStreamingCall<byte[], byte[]> inner, IClientStreamWriter<TRequest> requestStream, IAsyncStreamReader<TResponse> responseStream, MessagePackSerializerOptions serializerOptions)
        {
            this.inner = inner;
            this.requestStream = requestStream;
            this.responseStream = responseStream;
        }

        public AsyncDuplexStreamingCall<byte[], byte[]> RawStreamingCall => inner;

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
        /// Async stream to send streaming requests.
        /// </summary>
        public IClientStreamWriter<TRequest> RequestStream
        {
            get
            {
                return requestStream;
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
        /// If the call has already finished normally (request stream has been completed and response stream has been fully read), doesn't do anything.
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