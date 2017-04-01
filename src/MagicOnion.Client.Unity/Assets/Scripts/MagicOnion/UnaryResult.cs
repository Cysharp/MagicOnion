using System;
using Grpc.Core;
using UniRx;
using MessagePack;

namespace MagicOnion
{
    /// <summary>
    /// Wrapped AsyncUnaryCall.
    /// </summary>
    public struct UnaryResult<TResponse> : IObservable<TResponse>
    {
        readonly AsyncUnaryCall<byte[]> inner;
        readonly IFormatterResolver resolver;

        public UnaryResult(AsyncUnaryCall<byte[]> inner, IFormatterResolver resolver)
        {
            this.inner = inner;
            this.resolver = resolver;
        }

        /// <summary>
        /// Asynchronous call result.
        /// </summary>
        public IObservable<TResponse> ResponseAsync
        {
            get
            {
                var m = resolver; // struct can not use field value in lambda(if avoid, we needs to implement SelectWithState)
                return inner.ResponseAsync.Select(x => LZ4MessagePackSerializer.Deserialize<TResponse>(x, m));
            }
        }

        public IObservable<TResponse> ResponseAsyncOnMainThread
        {
            get
            {
                return ResponseAsync.ObserveOnMainThread();
            }
        }

        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        public IObservable<Metadata> ResponseHeadersAsync
        {
            get
            {
                return inner.ResponseHeadersAsync;
            }
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

        public IDisposable Subscribe(IObserver<TResponse> observer)
        {
            return ResponseAsyncOnMainThread.Subscribe(observer);
        }
    }
}