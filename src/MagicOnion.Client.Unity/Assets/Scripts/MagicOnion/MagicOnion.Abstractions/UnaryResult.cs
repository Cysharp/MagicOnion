using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.CompilerServices; // require this using in AsyncMethodBuilder
using System;
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

        readonly Task<IResponseContext<TResponse>> response;

        public UnaryResult(TResponse rawValue)
        {
            this.hasRawValue = true;
            this.rawValue = rawValue;
            this.rawTaskValue = null;
            this.response = null;
        }

        public UnaryResult(Task<TResponse> rawTaskValue)
        {
            this.hasRawValue = true;
            this.rawValue = default(TResponse);
            this.rawTaskValue = rawTaskValue;
            this.response = null;
        }

        public UnaryResult(Task<IResponseContext<TResponse>> response)
        {
            this.hasRawValue = false;
            this.rawValue = default(TResponse);
            this.rawTaskValue = null;
            this.response = response;
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
                    return UnwrapResponse();
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
                return UnwrapResponseHeaders();
            }
        }

        async Task<TResponse> UnwrapResponse()
        {
            var ctx = await response.ConfigureAwait(false);
            return await ctx.ResponseAsync.ConfigureAwait(false);
        }

        async Task<Metadata> UnwrapResponseHeaders()
        {
            var ctx = await response.ConfigureAwait(false);
            return await ctx.ResponseHeadersAsync.ConfigureAwait(false);
        }

        async void UnwrapDispose()
        {
            try
            {
                var ctx = await response.ConfigureAwait(false);
                ctx.Dispose();
            }
            catch
            {
            }
        }

        IResponseContext<TResponse> TryUnwrap()
        {
            if (!response.IsCompleted)
            {
                throw new InvalidOperationException("UnaryResult request is not yet completed, please await before call this.");
            }

            return response.Result;
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
            return TryUnwrap().GetStatus();
        }

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Metadata GetTrailers()
        {
            return TryUnwrap().GetTrailers();
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
            if (!response.IsCompleted)
            {
                UnwrapDispose();
            }
            else
            {
                response.Result.Dispose();
            }
        }
    }
}