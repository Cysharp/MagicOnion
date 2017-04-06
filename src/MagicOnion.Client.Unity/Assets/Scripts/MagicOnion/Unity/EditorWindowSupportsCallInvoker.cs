#if UNITY_EDITOR

using Grpc.Core;
using Grpc.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicOnion
{
    public class EditorWindowSupportsCallInvoker : CallInvoker
    {
        readonly CallInvoker unaryInvoker;
        readonly Channel channel;

        public EditorWindowSupportsCallInvoker(Channel channel)
        {
            this.channel = channel;
            this.unaryInvoker = null;
        }

        public EditorWindowSupportsCallInvoker(CallInvoker unaryInvoker, Channel channel)
        {
            this.channel = channel;
            this.unaryInvoker = unaryInvoker;
        }

        /// <summary>
        /// Invokes a simple remote call asynchronously.
        /// </summary>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            if (unaryInvoker == null)
            {
                var call = CreateCall(method, host, options);
                return Calls.AsyncUnaryCall(call, request);
            }
            else
            {
                return unaryInvoker.AsyncUnaryCall(method, host, options, request);
            }
        }

        /// <summary>
        /// Invokes a server streaming call asynchronously.
        /// In server streaming scenario, client sends on request and server responds with a stream of responses.
        /// </summary>
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var id = MagicOnionWindow.AddSubscription(channel, method.FullName);

            var call = CreateCall(method, host, options);
            return CustomCalls.AsyncServerStreamingCall(call, request, () => MagicOnionWindow.RemoveSubscription(id), id);
        }

        /// <summary>
        /// Invokes a client streaming call asynchronously.
        /// In client streaming scenario, client sends a stream of requests and server responds with a single response.
        /// </summary>
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            var id = MagicOnionWindow.AddSubscription(channel, method.FullName);

            var call = CreateCall(method, host, options);
            return CustomCalls.AsyncClientStreamingCall(call, () => MagicOnionWindow.RemoveSubscription(id));
        }

        /// <summary>
        /// Invokes a duplex streaming call asynchronously.
        /// In duplex streaming scenario, client sends a stream of requests and server responds with a stream of responses.
        /// The response stream is completely independent and both side can be sending messages at the same time.
        /// </summary>
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            var id = MagicOnionWindow.AddSubscription(channel, method.FullName);

            var call = CreateCall(method, host, options);
            return CustomCalls.AsyncDuplexStreamingCall(call, () => MagicOnionWindow.RemoveSubscription(id));
        }

        protected virtual CallInvocationDetails<TRequest, TResponse> CreateCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
                where TRequest : class
                where TResponse : class
        {
            return new CallInvocationDetails<TRequest, TResponse>(channel, method, host, options);
        }
    }

    public static class CustomCalls
    {
        public static AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(CallInvocationDetails<TRequest, TResponse> call, TRequest req, Action customDisposeAction)
            where TRequest : class
            where TResponse : class
        {
            var asyncCall = new AsyncCall<TRequest, TResponse>(call);
            var asyncResult = asyncCall.UnaryCallAsync(req);

            var token = asyncCall.Details.Options.CancellationToken;
            if (token.CanBeCanceled)
            {
                token.Register(() => customDisposeAction());
                return new AsyncUnaryCall<TResponse>(asyncResult, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, asyncCall.Cancel);
            }
            else
            {
                return new AsyncUnaryCall<TResponse>(asyncResult, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, () =>
                {
                    customDisposeAction();
                    asyncCall.Cancel();
                });
            }
        }

        public static AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(CallInvocationDetails<TRequest, TResponse> call, TRequest req, Action customDisposeAction, string id)
            where TRequest : class
            where TResponse : class
        {
            var asyncCall = new AsyncCall<TRequest, TResponse>(call);
            asyncCall.StartServerStreamingCall(req);
            var responseStream = new ClientResponseStream<TRequest, TResponse>(asyncCall);

            var token = asyncCall.Details.Options.CancellationToken;
            if (token.CanBeCanceled)
            {
                token.Register(() =>
                {
                    customDisposeAction();
                });

                return new AsyncServerStreamingCall<TResponse>(responseStream, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, asyncCall.Cancel);
            }
            else
            {
                return new AsyncServerStreamingCall<TResponse>(responseStream, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, () => { asyncCall.Cancel(); customDisposeAction(); });
            }
        }

        public static AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(CallInvocationDetails<TRequest, TResponse> call, Action customDisposeAction)
            where TRequest : class
            where TResponse : class
        {
            var asyncCall = new AsyncCall<TRequest, TResponse>(call);
            var resultTask = asyncCall.ClientStreamingCallAsync();
            var requestStream = new ClientRequestStream<TRequest, TResponse>(asyncCall);

            var token = asyncCall.Details.Options.CancellationToken;
            if (token.CanBeCanceled)
            {
                token.Register(() => customDisposeAction());
                return new AsyncClientStreamingCall<TRequest, TResponse>(requestStream, resultTask, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, asyncCall.Cancel);
            }
            else
            {
                return new AsyncClientStreamingCall<TRequest, TResponse>(requestStream, resultTask, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, () => { asyncCall.Cancel(); customDisposeAction(); });
            }

        }

        public static AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(CallInvocationDetails<TRequest, TResponse> call, Action customDisposeAction)
            where TRequest : class
            where TResponse : class
        {
            var asyncCall = new AsyncCall<TRequest, TResponse>(call);
            asyncCall.StartDuplexStreamingCall();
            var requestStream = new ClientRequestStream<TRequest, TResponse>(asyncCall);
            var responseStream = new ClientResponseStream<TRequest, TResponse>(asyncCall);

            var token = asyncCall.Details.Options.CancellationToken;
            if (token.CanBeCanceled)
            {
                token.Register(() => customDisposeAction());
                return new AsyncDuplexStreamingCall<TRequest, TResponse>(requestStream, responseStream, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, asyncCall.Cancel);
            }
            else
            {
                return new AsyncDuplexStreamingCall<TRequest, TResponse>(requestStream, responseStream, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, () => { asyncCall.Cancel(); customDisposeAction(); });
            }
        }
    }
}

#endif