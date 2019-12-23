using Grpc.Core;
using MessagePack;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
    public abstract class MagicOnionClientBase
    {
        protected string host;
        protected CallOptions option;
        protected CallInvoker callInvoker;
        protected MessagePackSerializerOptions serializerOptions;
        protected IClientFilter[] filters;

        static protected ResponseContext CreateResponseContext<TResponse>(RequestContext context, Method<byte[], byte[]> method)
        {
            var self = context.Client;
            var callResult = self.callInvoker.AsyncUnaryCall(method, self.host, context.CallOptions, context.RequestMutator(MagicOnionMarshallers.UnsafeNilBytes));
            var response = new ResponseContext<TResponse>(callResult, self.serializerOptions);
            response.SetResponseMutator(context.ResponseMutator);
            return response;
        }

        static protected ResponseContext CreateResponseContext<TRequest, TResponse>(RequestContext context, Method<byte[], byte[]> method)
        {
            var self = context.Client;
            var message = MessagePackSerializer.Serialize<TRequest>(((RequestContext<TRequest>)context).Request, self.serializerOptions);
            var callResult = self.callInvoker.AsyncUnaryCall(method, self.host, context.CallOptions, context.RequestMutator(message));
            var response = new ResponseContext<TResponse>(callResult, self.serializerOptions);
            response.SetResponseMutator(context.ResponseMutator);
            return response;
        }
    }

    public abstract class MagicOnionClientBase<T> : MagicOnionClientBase
        where T : IService<T>
    {
        protected MagicOnionClientBase()
        {
        }

        protected MagicOnionClientBase(CallInvoker callInvoker, MessagePackSerializerOptions serializerOptions, IClientFilter[] filters)
        {
            this.callInvoker = callInvoker;
            this.serializerOptions = serializerOptions;
            this.filters = filters;
        }

        protected UnaryResult<TResponse> InvokeAsync<TRequest, TResponse>(string path, TRequest request, Func<RequestContext, ResponseContext> requestMethod)
        {
            var future = InvokeAsyncCore<TRequest, TResponse>(path, request, requestMethod);
            return new UnaryResult<TResponse>(future);
        }

        async Task<IResponseContext<TResponse>> InvokeAsyncCore<TRequest, TResponse>(string path, TRequest request, Func<RequestContext, ResponseContext> requestMethod)
        {
            if (this.option.Headers == null && filters.Length != 0)
            {
                // always creating new Metadata is bad manner for performance
                this.option = this.option.WithHeaders(new Metadata());
            }

            var requestContext = new RequestContext<TRequest>(request, this, path, option, typeof(TResponse), filters, requestMethod);
            var response = await InterceptInvokeHelper.InvokeWithFilter(requestContext);
            var result = response as IResponseContext<TResponse>;
            if (result != null)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException("ResponseContext is null.");
            }
        }

        protected async Task<UnaryResult<TResponse>> InvokeTaskAsync<TRequest, TResponse>(string path, TRequest request, Func<RequestContext, ResponseContext> requestMethod)
        {
            if (this.option.Headers == null && filters.Length != 0)
            {
                // always creating new Metadata is bad manner for performance
                this.option = this.option.WithHeaders(new Metadata());
            }

            var requestContext = new RequestContext<TRequest>(request, this, path, option, typeof(TResponse), filters, requestMethod);
            var response = await InterceptInvokeHelper.InvokeWithFilter(requestContext);
            var result = response as IResponseContext<TResponse>;
            if (result != null)
            {
                return new UnaryResult<TResponse>(Task.FromResult(result));
            }
            else
            {
                throw new InvalidOperationException("ResponseContext is null.");
            }
        }

        protected abstract MagicOnionClientBase<T> Clone();

        public virtual T WithCancellationToken(CancellationToken cancellationToken)
        {
            return WithOptions(this.option.WithCancellationToken(cancellationToken));
        }

        public virtual T WithDeadline(DateTime deadline)
        {
            return WithOptions(this.option.WithDeadline(deadline));
        }

        public virtual T WithHeaders(Metadata headers)
        {
            return WithOptions(this.option.WithHeaders(headers));
        }

        public virtual T WithHost(string host)
        {
            var newInstance = Clone();
            newInstance.host = host;
            return (T)(object)newInstance;
        }

        public virtual T WithOptions(CallOptions option)
        {
            var newInstance = Clone();
            newInstance.option = option;
            return (T)(object)newInstance;
        }
    }
}