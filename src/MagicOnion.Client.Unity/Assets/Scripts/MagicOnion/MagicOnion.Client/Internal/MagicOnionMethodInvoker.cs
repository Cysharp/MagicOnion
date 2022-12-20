using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion.Client.Internal
{
    // Pubternal API: This class is used from generated clients and is therefore `public` but internal API.
    public static class RawMethodInvoker
    {
        public static RawMethodInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializerProvider messageSerializerProvider)
            where TRequest : class
            where TResponse : class
            => new RawMethodInvoker<TRequest, TResponse, TRequest, TResponse>(methodType, serviceName, name, messageSerializerProvider.Create(methodType, null));

        public static RawMethodInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializerProvider messageSerializerProvider)
            where TRequest : class
            => new RawMethodInvoker<TRequest, TResponse, TRequest, Box<TResponse>>(methodType, serviceName, name, messageSerializerProvider.Create(methodType, null));

        public static RawMethodInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializerProvider messageSerializerProvider)
            where TResponse : class
            => new RawMethodInvoker<TRequest, TResponse, Box<TRequest>, TResponse>(methodType, serviceName, name, messageSerializerProvider.Create(methodType, null));

        public static RawMethodInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(MethodType methodType, string serviceName, string name, IMagicOnionSerializerProvider messageSerializerProvider)
            => new RawMethodInvoker<TRequest, TResponse, Box<TRequest>, Box<TResponse>>(methodType, serviceName, name, messageSerializerProvider.Create(methodType, null));
    }

    public abstract class RawMethodInvoker<TRequest, TResponse>
    {
        public abstract UnaryResult<TResponse> InvokeUnary(MagicOnionClientBase client, string path, TRequest request);
        public abstract UnaryResult InvokeUnaryNonGeneric(MagicOnionClientBase client, string path, TRequest request);
        public abstract Task<ServerStreamingResult<TResponse>> InvokeServerStreaming(MagicOnionClientBase client, string path, TRequest request);
        public abstract Task<ClientStreamingResult<TRequest, TResponse>> InvokeClientStreaming(MagicOnionClientBase client, string path);
        public abstract Task<DuplexStreamingResult<TRequest, TResponse>> InvokeDuplexStreaming(MagicOnionClientBase client, string path);
    }

    public sealed class RawMethodInvoker<TRequest, TResponse, TRawRequest, TRawResponse> : RawMethodInvoker<TRequest, TResponse>
        where TRawRequest : class
        where TRawResponse : class
    {
        readonly GrpcMethodHelper.MagicOnionMethod<TRequest, TResponse, TRawRequest, TRawResponse> method;
        readonly Func<RequestContext, ResponseContext> createUnaryResponseContext;

        public RawMethodInvoker(MethodType methodType, string serviceName, string name, IMagicOnionSerializer messageSerializer)
        {
            this.method = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(methodType, serviceName, name, messageSerializer);
            this.createUnaryResponseContext = context => ResponseContext<TResponse>.Create<TRawResponse>(
                context.Client.Options.CallInvoker.AsyncUnaryCall(method.Method, context.Client.Options.Host, context.CallOptions, method.ToRawRequest(((RequestContext<TRequest>)context).Request)));
        }

        public override UnaryResult<TResponse> InvokeUnary(MagicOnionClientBase client, string path, TRequest request)
        {
            var future = InvokeUnaryCore(client, path, request, createUnaryResponseContext);
            return new UnaryResult<TResponse>(future);
        }

        public override UnaryResult InvokeUnaryNonGeneric(MagicOnionClientBase client, string path, TRequest request)
        {
            var future = (Task<IResponseContext<Nil>>)(object)InvokeUnaryCore(client, path, request, createUnaryResponseContext);
            return new UnaryResult(future);
        }

        async Task<IResponseContext<TResponse>> InvokeUnaryCore(MagicOnionClientBase client, string path, TRequest request, Func<RequestContext, ResponseContext> requestMethod)
        {
            var requestContext = new RequestContext<TRequest>(request, client, path, client.Options.CallOptions, typeof(TResponse), client.Options.Filters, requestMethod);
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

        public override Task<ServerStreamingResult<TResponse>> InvokeServerStreaming(MagicOnionClientBase client, string path, TRequest request)
            => Task.FromResult(
                new ServerStreamingResult<TResponse>(
                    new AsyncServerStreamingCallWrapper(
                        client.Options.CallInvoker.AsyncServerStreamingCall(method.Method, client.Options.Host, client.Options.CallOptions, method.ToRawRequest(request)))));

        public override Task<ClientStreamingResult<TRequest, TResponse>> InvokeClientStreaming(MagicOnionClientBase client, string path)
            => Task.FromResult(
                new ClientStreamingResult<TRequest, TResponse>(
                    new AsyncClientStreamingCallWrapper(
                        client.Options.CallInvoker.AsyncClientStreamingCall(method.Method, client.Options.Host, client.Options.CallOptions))));

        public override Task<DuplexStreamingResult<TRequest, TResponse>> InvokeDuplexStreaming(MagicOnionClientBase client, string path)
            => Task.FromResult(
                new DuplexStreamingResult<TRequest, TResponse>(
                    new AsyncDuplexStreamingCallWrapper(
                        client.Options.CallInvoker.AsyncDuplexStreamingCall(method.Method, client.Options.Host, client.Options.CallOptions))));

        class AsyncServerStreamingCallWrapper : IAsyncServerStreamingCallWrapper<TResponse>
        {
            readonly AsyncServerStreamingCall<TRawResponse> inner;
            IAsyncStreamReader<TResponse> responseStream;

            public AsyncServerStreamingCallWrapper(AsyncServerStreamingCall<TRawResponse> inner)
            {
                this.inner = inner;
            }

            public Task<Metadata> ResponseHeadersAsync
                => inner.ResponseHeadersAsync;
            public Status GetStatus()
                => inner.GetStatus();
            public Metadata GetTrailers()
                => inner.GetTrailers();

            public IAsyncStreamReader<TResponse> ResponseStream
                => responseStream ?? (responseStream = (typeof(TRawResponse) == typeof(Box<TResponse>)) ? new UnboxAsyncStreamReader<TResponse>((IAsyncStreamReader<Box<TResponse>>)inner.ResponseStream) : (IAsyncStreamReader<TResponse>)inner.ResponseStream);

            public void Dispose()
                => inner.Dispose();
        }

        class AsyncClientStreamingCallWrapper : IAsyncClientStreamingCallWrapper<TRequest, TResponse>
        {
            readonly AsyncClientStreamingCall<TRawRequest, TRawResponse> inner;
            IClientStreamWriter<TRequest> requestStream;

            public AsyncClientStreamingCallWrapper(AsyncClientStreamingCall<TRawRequest, TRawResponse> inner)
            {
                this.inner = inner;
            }

            public Task<Metadata> ResponseHeadersAsync
                => inner.ResponseHeadersAsync;
            public Status GetStatus()
                => inner.GetStatus();
            public Metadata GetTrailers()
                => inner.GetTrailers();

            public IClientStreamWriter<TRequest> RequestStream
                => requestStream ?? (requestStream = (typeof(TRawRequest) == typeof(Box<TRequest>)) ? new BoxClientStreamWriter<TRequest>((IClientStreamWriter<Box<TRequest>>)inner.RequestStream) : (IClientStreamWriter<TRequest>)inner.RequestStream);
            public Task<TResponse> ResponseAsync
                => UnboxResponseAsync();

            private async Task<TResponse> UnboxResponseAsync()
                => (typeof(TRawResponse) == typeof(Box<TResponse>))
                    ? ((Box<TResponse>)(object)(await inner.ResponseAsync)).Value
                    : (TResponse)(object)await inner.ResponseAsync;

            public void Dispose()
                => inner.Dispose();
        }

        class AsyncDuplexStreamingCallWrapper : IAsyncDuplexStreamingCallWrapper<TRequest, TResponse>
        {
            readonly AsyncDuplexStreamingCall<TRawRequest, TRawResponse> inner;
            IClientStreamWriter<TRequest> requestStream;
            IAsyncStreamReader<TResponse> responseStream;

            public AsyncDuplexStreamingCallWrapper(AsyncDuplexStreamingCall<TRawRequest, TRawResponse> inner)
            {
                this.inner = inner;
            }

            public Task<Metadata> ResponseHeadersAsync
                => inner.ResponseHeadersAsync;
            public Status GetStatus()
                => inner.GetStatus();
            public Metadata GetTrailers()
                => inner.GetTrailers();

            public IClientStreamWriter<TRequest> RequestStream
                => requestStream ?? (requestStream = (typeof(TRawRequest) == typeof(Box<TRequest>)) ? new BoxClientStreamWriter<TRequest>((IClientStreamWriter<Box<TRequest>>)inner.RequestStream) : (IClientStreamWriter<TRequest>)inner.RequestStream);
            public IAsyncStreamReader<TResponse> ResponseStream
                => responseStream ?? (responseStream = (typeof(TRawResponse) == typeof(Box<TResponse>)) ? new UnboxAsyncStreamReader<TResponse>((IAsyncStreamReader<Box<TResponse>>)inner.ResponseStream) : (IAsyncStreamReader<TResponse>)inner.ResponseStream);

            public void Dispose()
                => inner.Dispose();
        }
    }
}
