using Grpc.Core;
using MessagePack;
using System;
using System.Threading.Tasks;
using MagicOnion.Internal;

namespace MagicOnion
{
    /// <summary>
    /// Wrapped AsyncDuplexStreamingCall.
    /// </summary>
    public struct DuplexStreamingResult<TRequest, TResponse> : IDisposable
    {
        readonly IDisposable inner; // AsyncDuplexStreamingCall<TRequest, TResponse> or AsyncDuplexStreamingCall<Box<TRequest>, TResponse> or AsyncDuplexStreamingCall<TRequest, Box<TResponse>> or AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>>

        public DuplexStreamingResult(AsyncDuplexStreamingCall<TRequest, TResponse> inner)
            : this((IDisposable)inner)
        { }
        public DuplexStreamingResult(AsyncDuplexStreamingCall<Box<TRequest>, TResponse> inner)
            : this((IDisposable)inner)
        { }
        public DuplexStreamingResult(AsyncDuplexStreamingCall<TRequest, Box<TResponse>> inner)
            : this((IDisposable)inner)
        { }
        public DuplexStreamingResult(AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>> inner)
            : this((IDisposable)inner)
        { }

        private DuplexStreamingResult(IDisposable inner)
        {
            this.inner = inner;
        }

        /// <summary>
        /// Async stream to read streaming responses.
        /// </summary>
        public IAsyncStreamReader<TResponse> ResponseStream
            => (inner is AsyncDuplexStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? requestBoxed.ResponseStream
                : (inner is AsyncDuplexStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? new UnboxAsyncStreamReader<TResponse>(responseBoxed.ResponseStream)
                    : (inner is AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? new UnboxAsyncStreamReader<TResponse>(requestAndResponseBoxed.ResponseStream)
                        : ((AsyncDuplexStreamingCall<TRequest, TResponse>)inner).ResponseStream;


        /// <summary>
        /// Async stream to send streaming requests.
        /// </summary>
        public IClientStreamWriter<TRequest> RequestStream
            => (inner is AsyncDuplexStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? new BoxClientStreamWriter<TRequest>(requestBoxed.RequestStream)
                : (inner is AsyncDuplexStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? responseBoxed.RequestStream
                    : (inner is AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? new BoxClientStreamWriter<TRequest>(requestAndResponseBoxed.RequestStream)
                        : ((AsyncDuplexStreamingCall<TRequest, TResponse>)inner).RequestStream;


        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        public Task<Metadata> ResponseHeadersAsync
            => (inner is AsyncDuplexStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? requestBoxed.ResponseHeadersAsync
                : (inner is AsyncDuplexStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? responseBoxed.ResponseHeadersAsync
                    : (inner is AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? requestAndResponseBoxed.ResponseHeadersAsync
                        : ((AsyncDuplexStreamingCall<TRequest, TResponse>)inner).ResponseHeadersAsync;

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Status GetStatus()
            => (inner is AsyncDuplexStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? requestBoxed.GetStatus()
                : (inner is AsyncDuplexStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? responseBoxed.GetStatus()
                    : (inner is AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? requestAndResponseBoxed.GetStatus()
                        : ((AsyncDuplexStreamingCall<TRequest, TResponse>)inner).GetStatus();

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Metadata GetTrailers()
            => (inner is AsyncDuplexStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? requestBoxed.GetTrailers()
                : (inner is AsyncDuplexStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? responseBoxed.GetTrailers()
                    : (inner is AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? requestAndResponseBoxed.GetTrailers()
                        : ((AsyncDuplexStreamingCall<TRequest, TResponse>)inner).GetTrailers();

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