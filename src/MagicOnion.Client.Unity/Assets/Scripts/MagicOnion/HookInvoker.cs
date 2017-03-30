using Grpc.Core;
using Grpc.Core.Internal;
using UniRx;

namespace MagicOnion
{
    public abstract class UnaryHookCallInvoker : DefaultCallInvoker
    {
        public UnaryHookCallInvoker(Channel channel) : base(channel)
        {
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = CreateCall(method, host, options);
            var asyncCall = new AsyncCall<TRequest, TResponse>(call);
            var asyncResult = HandleUnaryCall(asyncCall.UnaryCallAsync(request), call);

            return new AsyncUnaryCall<TResponse>(asyncResult, asyncCall.ResponseHeadersAsync, asyncCall.GetStatus, asyncCall.GetTrailers, asyncCall.Cancel);
        }

        protected abstract IObservable<TResponse> HandleUnaryCall<TRequest, TResponse>(IObservable<TResponse> source, CallInvocationDetails<TRequest, TResponse> details);
    }
}
