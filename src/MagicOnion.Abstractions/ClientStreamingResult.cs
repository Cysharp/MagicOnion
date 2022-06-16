using Grpc.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Internal;

namespace MagicOnion
{
    /// <summary>
    /// Wrapped AsyncClientStreamingCall.
    /// </summary>
    public struct ClientStreamingResult<TRequest, TResponse> : IDisposable
    {
        internal readonly TResponse rawValue;
        internal readonly bool hasRawValue;
        readonly IDisposable inner; // AsyncClientStreamingCall<TRequest, TResponse> or AsyncClientStreamingCall<Box<TRequest>, TResponse> or AsyncClientStreamingCall<TRequest, Box<TResponse>> or AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>>

        public ClientStreamingResult(TResponse rawValue)
        {
            this.hasRawValue = true;
            this.rawValue = rawValue;
            this.inner = null;
        }

        public ClientStreamingResult(AsyncClientStreamingCall<TRequest, TResponse> inner)
            : this((IDisposable)inner)
        { }
        public ClientStreamingResult(AsyncClientStreamingCall<Box<TRequest>, TResponse> inner)
            : this((IDisposable)inner)
        { }
        public ClientStreamingResult(AsyncClientStreamingCall<TRequest, Box<TResponse>> inner)
            : this((IDisposable)inner)
        { }
        public ClientStreamingResult(AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>> inner)
            : this((IDisposable)inner)
        { }

        private ClientStreamingResult(IDisposable inner)
        {
            this.hasRawValue = false;
            this.rawValue = default(TResponse);
            this.inner = inner;
        }

        /// <summary>
        /// Asynchronous call result.
        /// </summary>
        public Task<TResponse> ResponseAsync
        {
            get
            {
                if (hasRawValue)
                {
                    return Task.FromResult(rawValue);
                }
                else
                {
                    return (inner is AsyncClientStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                        ? requestBoxed.ResponseAsync
                        : (inner is AsyncClientStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                            ? responseBoxed.ResponseAsync.ContinueWith(x => x.Result.Value)
                            : (inner is AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                                ? requestAndResponseBoxed.ResponseAsync.ContinueWith(x => x.Result.Value)
                                : ((AsyncClientStreamingCall<TRequest, TResponse>)inner).ResponseAsync;
                }
            }
        }

        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        public Task<Metadata> ResponseHeadersAsync
            => (inner is AsyncClientStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? requestBoxed.ResponseHeadersAsync
                : (inner is AsyncClientStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? responseBoxed.ResponseHeadersAsync
                    : (inner is AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? requestAndResponseBoxed.ResponseHeadersAsync
                        : ((AsyncClientStreamingCall<TRequest, TResponse>)inner).ResponseHeadersAsync;

        /// <summary>
        /// Async stream to send streaming requests.
        /// </summary>
        public IClientStreamWriter<TRequest> RequestStream
        {
            get
            {
                if (inner == null) return null;
                
                return (inner is AsyncClientStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                    ? new BoxClientStreamWriter<TRequest>(requestBoxed.RequestStream)
                    : (inner is AsyncClientStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                        ? responseBoxed.RequestStream
                        : (inner is AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                            ? new BoxClientStreamWriter<TRequest>(requestAndResponseBoxed.RequestStream)
                            : ((AsyncClientStreamingCall<TRequest, TResponse>)inner).RequestStream;
            }
        }


        /// <summary>
        /// Allows awaiting this object directly.
        /// </summary>
        /// <returns></returns>
        public TaskAwaiter<TResponse> GetAwaiter()
        {
            return ResponseAsync.GetAwaiter();
        }

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Status GetStatus()
            => (inner is AsyncClientStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? requestBoxed.GetStatus()
                : (inner is AsyncClientStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? responseBoxed.GetStatus()
                    : (inner is AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? requestAndResponseBoxed.GetStatus()
                        : ((AsyncClientStreamingCall<TRequest, TResponse>)inner).GetStatus();

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Metadata GetTrailers()
            => (inner is AsyncClientStreamingCall<Box<TRequest>, TResponse> requestBoxed)
                ? requestBoxed.GetTrailers()
                : (inner is AsyncClientStreamingCall<TRequest, Box<TResponse>> responseBoxed)
                    ? responseBoxed.GetTrailers()
                    : (inner is AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>> requestAndResponseBoxed)
                        ? requestAndResponseBoxed.GetTrailers()
                        : ((AsyncClientStreamingCall<TRequest, TResponse>)inner).GetTrailers();

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
            if (this.inner != null)
            {
                this.inner.Dispose();
            }
        }
    }
}
