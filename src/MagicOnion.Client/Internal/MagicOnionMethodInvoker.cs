using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Internal;
using MessagePack;

namespace MagicOnion.Client.Internal
{
    // Pubternal API: This class is used from generated clients and is therefore `public` but internal API.
    public class UnaryMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<RequestContext, ResponseContext> createResponseContext;

        public UnaryMethodRawInvoker(string serviceName, string name, MessagePackSerializerOptions serializerOptions, Func<RequestContext, IMethod, AsyncUnaryCall<TResponse>> grpcCall)
        {
            var method = GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions);
            createResponseContext = context => new ResponseContext<TResponse>(grpcCall(context, method));
        }
        public UnaryMethodRawInvoker(string serviceName, string name, MessagePackSerializerOptions serializerOptions, Func<RequestContext, IMethod, AsyncUnaryCall<Box<TResponse>>> grpcCall)
        {
            var method = GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions);
            createResponseContext = context => new ResponseContext<TResponse>(grpcCall(context, method));
        }

        public UnaryResult<TResponse> Invoke(MagicOnionClientBase client, string path, TRequest request)
        {
            var future = InvokeAsyncCore(client, path, request, createResponseContext);
            return new UnaryResult<TResponse>(future);

        }
        private async Task<IResponseContext<TResponse>> InvokeAsyncCore(MagicOnionClientBase client, string path, TRequest request, Func<RequestContext, ResponseContext> requestMethod)
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
    }

    public static class UnaryMethodRawInvoker
    {
        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new UnaryMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions, (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<TRequest, TResponse>)method, context.Client.Options.Host, context.CallOptions, ((RequestContext<TRequest>)context).Request));

        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new UnaryMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions, (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<TRequest, Box<TResponse>>)method, context.Client.Options.Host, context.CallOptions, ((RequestContext<TRequest>)context).Request));

        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new UnaryMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions, (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<Box<TRequest>, TResponse>)method, context.Client.Options.Host, context.CallOptions, Box.Create(((RequestContext<TRequest>)context).Request)));

        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new UnaryMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions, (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<Box<TRequest>, Box<TResponse>>)method, context.Client.Options.Host, context.CallOptions, Box.Create(((RequestContext<TRequest>)context).Request)));
    }

    public class ServerStreamingMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<MagicOnionClientBase, TRequest, IMethod, IAsyncServerStreamingCallWrapper<TResponse>> grpcCall;
        readonly IMethod method;

        public ServerStreamingMethodRawInvoker(string serviceName, string name, MessagePackSerializerOptions serializerOptions, Func<MagicOnionClientBase, TRequest, IMethod, IAsyncServerStreamingCallWrapper<TResponse>> grpcCall)
        {
            this.method = GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ServerStreaming, serviceName, name, serializerOptions);
            this.grpcCall = grpcCall;
        }

        public Task<ServerStreamingResult<TResponse>> Invoke(MagicOnionClientBase client, string path, TRequest request)
            => Task.FromResult(new ServerStreamingResult<TResponse>(grpcCall(client, request, method)));
    }

    public static class ServerStreamingMethodRawInvoker
    {
        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, request, method) => new AsyncServerStreamingCallWrapper<TResponse, TResponse>(client.Options.CallInvoker.AsyncServerStreamingCall((Method<TRequest, TResponse>)method, client.Options.Host, client.Options.CallOptions, request)));

        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, request, method) => new AsyncServerStreamingCallWrapper<Box<TResponse>, TResponse>(client.Options.CallInvoker.AsyncServerStreamingCall((Method<TRequest, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions, request)));

        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, request, method) => new AsyncServerStreamingCallWrapper<TResponse, TResponse>(client.Options.CallInvoker.AsyncServerStreamingCall((Method<Box<TRequest>, TResponse>)method, client.Options.Host, client.Options.CallOptions, Box.Create(request))));

        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, request, method) => new AsyncServerStreamingCallWrapper<Box<TResponse>, TResponse>(client.Options.CallInvoker.AsyncServerStreamingCall((Method<Box<TRequest>, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions, Box.Create(request))));

        class AsyncServerStreamingCallWrapper<TResponseRaw, TResponse> : IAsyncServerStreamingCallWrapper<TResponse>
        {
            readonly AsyncServerStreamingCall<TResponseRaw> inner;
            IAsyncStreamReader<TResponse> responseStream;

            public AsyncServerStreamingCallWrapper(AsyncServerStreamingCall<TResponseRaw> inner)
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
                => responseStream ?? (responseStream = (typeof(TResponseRaw) == typeof(Box<TResponse>)) ? new UnboxAsyncStreamReader<TResponse>((IAsyncStreamReader<Box<TResponse>>)inner.ResponseStream) : (IAsyncStreamReader<TResponse>)inner.ResponseStream);

            public void Dispose()
                => inner.Dispose();
        }
    }

    public class ClientStreamingMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<MagicOnionClientBase, IMethod, IAsyncClientStreamingCallWrapper<TRequest, TResponse>> grpcCall;
        readonly IMethod method;

        public ClientStreamingMethodRawInvoker(string serviceName, string name, MessagePackSerializerOptions serializerOptions, Func<MagicOnionClientBase, IMethod, IAsyncClientStreamingCallWrapper<TRequest, TResponse>> grpcCall)
        {
            this.method = GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, name, serializerOptions);
            this.grpcCall = grpcCall;
        }

        public Task<ClientStreamingResult<TRequest, TResponse>> Invoke(MagicOnionClientBase client, string path)
            => Task.FromResult(new ClientStreamingResult<TRequest, TResponse>(grpcCall(client, method)));
    }

    public static class ClientStreamingMethodRawInvoker
    {
        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, method) => new AsyncClientStreamingCallWrapper<TRequest, TResponse, TRequest, TResponse>(client.Options.CallInvoker.AsyncClientStreamingCall((Method<TRequest, TResponse>)method, client.Options.Host, client.Options.CallOptions)));

        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, method) => new AsyncClientStreamingCallWrapper<TRequest, Box<TResponse>, TRequest, TResponse>(client.Options.CallInvoker.AsyncClientStreamingCall((Method<TRequest, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions)));

        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, method) => new AsyncClientStreamingCallWrapper<Box<TRequest>, TResponse, TRequest, TResponse>(client.Options.CallInvoker.AsyncClientStreamingCall((Method<Box<TRequest>, TResponse>)method, client.Options.Host, client.Options.CallOptions)));

        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, method) => new AsyncClientStreamingCallWrapper<Box<TRequest>, Box<TResponse>, TRequest, TResponse>(client.Options.CallInvoker.AsyncClientStreamingCall((Method<Box<TRequest>, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions)));

        class AsyncClientStreamingCallWrapper<TRequestRaw, TResponseRaw, TRequest, TResponse> : IAsyncClientStreamingCallWrapper<TRequest, TResponse>
        {
            readonly AsyncClientStreamingCall<TRequestRaw, TResponseRaw> inner;
            IClientStreamWriter<TRequest> requestStream;

            public AsyncClientStreamingCallWrapper(AsyncClientStreamingCall<TRequestRaw, TResponseRaw> inner)
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
                => requestStream ?? (requestStream = (typeof(TRequestRaw) == typeof(Box<TRequest>)) ? new BoxClientStreamWriter<TRequest>((IClientStreamWriter<Box<TRequest>>)inner.RequestStream) : (IClientStreamWriter<TRequest>)inner.RequestStream);
            public Task<TResponse> ResponseAsync
                => (typeof(TResponseRaw) == typeof(Box<TResponse>))
                    ? inner.ResponseAsync.ContinueWith(x => ((Box<TResponse>)(object)x.Result).Value)
                    : inner.ResponseAsync.ContinueWith(x => (TResponse)(object)x.Result);

            public void Dispose()
                => inner.Dispose();
        }
    }
    
    public class DuplexStreamingMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<MagicOnionClientBase, IMethod, IAsyncDuplexStreamingCallWrapper<TRequest, TResponse>> grpcCall;
        readonly IMethod method;

        public DuplexStreamingMethodRawInvoker(string serviceName, string name, MessagePackSerializerOptions serializerOptions, Func<MagicOnionClientBase, IMethod, IAsyncDuplexStreamingCallWrapper<TRequest, TResponse>> grpcCall)
        {
            this.method = GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.DuplexStreaming, serviceName, name, serializerOptions);
            this.grpcCall = grpcCall;
        }

        public Task<DuplexStreamingResult<TRequest, TResponse>> Invoke(MagicOnionClientBase client, string path)
            => Task.FromResult(new DuplexStreamingResult<TRequest, TResponse>(grpcCall(client, method)));
    }

    public static class DuplexStreamingMethodRawInvoker
    {
        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions, 
                (client, method) => new AsyncDuplexStreamingCallWrapper<TRequest, TResponse, TRequest, TResponse>(client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<TRequest, TResponse>)method, client.Options.Host, client.Options.CallOptions)));

        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, method) => new AsyncDuplexStreamingCallWrapper<TRequest, Box<TResponse>, TRequest, TResponse>(client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<TRequest, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions)));

        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, method) => new AsyncDuplexStreamingCallWrapper<Box<TRequest>, TResponse, TRequest, TResponse>(client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<Box<TRequest>, TResponse>)method, client.Options.Host, client.Options.CallOptions)));

        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(serviceName, name, serializerOptions,
                (client, method) => new AsyncDuplexStreamingCallWrapper<Box<TRequest>, Box<TResponse>, TRequest, TResponse>(client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<Box<TRequest>, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions)));

        class AsyncDuplexStreamingCallWrapper<TRequestRaw, TResponseRaw, TRequest, TResponse> : IAsyncDuplexStreamingCallWrapper<TRequest, TResponse>
        {
            readonly AsyncDuplexStreamingCall<TRequestRaw, TResponseRaw> inner;
            IClientStreamWriter<TRequest> requestStream;
            IAsyncStreamReader<TResponse> responseStream;

            public AsyncDuplexStreamingCallWrapper(AsyncDuplexStreamingCall<TRequestRaw, TResponseRaw> inner)
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
                => requestStream ?? (requestStream = (typeof(TRequestRaw) == typeof(Box<TRequest>)) ? new BoxClientStreamWriter<TRequest>((IClientStreamWriter<Box<TRequest>>)inner.RequestStream) : (IClientStreamWriter<TRequest>)inner.RequestStream);
            public IAsyncStreamReader<TResponse> ResponseStream
                => responseStream ?? (responseStream = (typeof(TResponseRaw) == typeof(Box<TResponse>)) ? new UnboxAsyncStreamReader<TResponse>((IAsyncStreamReader<Box<TResponse>>)inner.ResponseStream) : (IAsyncStreamReader<TResponse>)inner.ResponseStream);

            public void Dispose()
                => inner.Dispose();
        }
    }
}