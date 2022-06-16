using Grpc.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicOnion.Internal;

namespace MagicOnion
{
    /// <summary>
    /// Wrapped AsyncServerStreamingCall.
    /// </summary>
    public struct ServerStreamingResult<TResponse> : IDisposable
    {
        readonly IDisposable inner; // AsyncServerStreamingCall<TResponse> or AsyncServerStreamingCall<Box<TResponse>>

        public ServerStreamingResult(AsyncServerStreamingCall<TResponse> inner)
        {
            this.inner = inner;
        }
        public ServerStreamingResult(AsyncServerStreamingCall<Box<TResponse>> inner)
        {
            this.inner = inner;
        }

        /// <summary>
        /// Async stream to read streaming responses.
        /// </summary>
        public IAsyncStreamReader<TResponse> ResponseStream
            => inner is AsyncServerStreamingCall<Box<TResponse>> boxedStreamingCall
                ? new UnboxAsyncStreamReader<TResponse>(boxedStreamingCall.ResponseStream)
                : ((AsyncServerStreamingCall<TResponse>)inner).ResponseStream;

        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        public Task<Metadata> ResponseHeadersAsync
            => inner is AsyncServerStreamingCall<Box<TResponse>> boxedStreamingCall
                ? boxedStreamingCall.ResponseHeadersAsync
                : ((AsyncServerStreamingCall<TResponse>)inner).ResponseHeadersAsync;

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Status GetStatus()
            => inner is AsyncServerStreamingCall<Box<TResponse>> boxedStreamingCall
                ? boxedStreamingCall.GetStatus()
                : ((AsyncServerStreamingCall<TResponse>)inner).GetStatus();

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Metadata GetTrailers()
            => inner is AsyncServerStreamingCall<Box<TResponse>> boxedStreamingCall
                ? boxedStreamingCall.GetTrailers()
                : ((AsyncServerStreamingCall<TResponse>)inner).GetTrailers();

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