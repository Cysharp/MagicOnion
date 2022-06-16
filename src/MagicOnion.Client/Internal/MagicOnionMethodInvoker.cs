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

        public UnaryMethodRawInvoker(IMethod method, Func<RequestContext, IMethod, AsyncUnaryCall<TResponse>> grpcCall)
        {
            createResponseContext = context => new ResponseContext<TResponse>(grpcCall(context, method));
        }
        public UnaryMethodRawInvoker(IMethod method, Func<RequestContext, IMethod, AsyncUnaryCall<Box<TResponse>>> grpcCall)
        {
            createResponseContext = context => new ResponseContext<TResponse>(grpcCall(context, method));
        }

        public UnaryResult<TResponse> Invoke(MagicOnionClientBase client, string path, TRequest request)
            => client.InvokeAsync<TRequest, TResponse>(path, request, createResponseContext);
    }

    public static class UnaryMethodRawInvoker
    {
        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new UnaryMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions),
                (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<TRequest, TResponse>)method, context.Client.Options.Host, context.CallOptions, ((RequestContext<TRequest>)context).Request));

        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new UnaryMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions),
                (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<TRequest, Box<TResponse>>)method, context.Client.Options.Host, context.CallOptions, ((RequestContext<TRequest>)context).Request));

        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new UnaryMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions),
                (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<Box<TRequest>, TResponse>)method, context.Client.Options.Host, context.CallOptions, new Box<TRequest>(((RequestContext<TRequest>)context).Request)));

        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new UnaryMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions),
                (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<Box<TRequest>, Box<TResponse>>)method, context.Client.Options.Host, context.CallOptions, new Box<TRequest>(((RequestContext<TRequest>)context).Request)));
    }

    public class ServerStreamingMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<MagicOnionClientBase, TRequest, ServerStreamingResult<TResponse>> createResult;

        public ServerStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, TRequest, IMethod, AsyncServerStreamingCall<TResponse>> grpcCall)
        {
            createResult = (client, request) => new ServerStreamingResult<TResponse>(grpcCall(client, request, method));
        }
        public ServerStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, TRequest, IMethod, AsyncServerStreamingCall<Box<TResponse>>> grpcCall)
        {
            createResult = (client, request) => new ServerStreamingResult<TResponse>(grpcCall(client, request, method));
        }

        public Task<ServerStreamingResult<TResponse>> Invoke(MagicOnionClientBase client, string path, TRequest request)
        {
            return Task.FromResult(createResult(client, request));
        }
    }

    public static class ServerStreamingMethodRawInvoker
    {
        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ServerStreaming, serviceName, name, serializerOptions),
                (client, request, method) => client.Options.CallInvoker.AsyncServerStreamingCall((Method<TRequest, TResponse>)method, client.Options.Host, client.Options.CallOptions, request));

        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ServerStreaming, serviceName, name, serializerOptions),
                (client, request, method) => client.Options.CallInvoker.AsyncServerStreamingCall((Method<TRequest, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions, request));

        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ServerStreaming, serviceName, name, serializerOptions),
                (client, request, method) => client.Options.CallInvoker.AsyncServerStreamingCall((Method<Box<TRequest>, TResponse>)method, client.Options.Host, client.Options.CallOptions, new Box<TRequest>(request)));

        public static ServerStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new ServerStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ServerStreaming, serviceName, name, serializerOptions),
                (client, request, method) => client.Options.CallInvoker.AsyncServerStreamingCall((Method<Box<TRequest>, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions, new Box<TRequest>(request)));
    }

    public class ClientStreamingMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<MagicOnionClientBase, ClientStreamingResult<TRequest, TResponse>> createResult;

        public ClientStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncClientStreamingCall<TRequest, TResponse>> grpcCall)
        {
            createResult = client => new ClientStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }
        public ClientStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncClientStreamingCall<TRequest, Box<TResponse>>> grpcCall)
        {
            createResult = client => new ClientStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }
        public ClientStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncClientStreamingCall<Box<TRequest>, Box<TResponse>>> grpcCall)
        {
            createResult = client => new ClientStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }
        public ClientStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncClientStreamingCall<Box<TRequest>, TResponse>> grpcCall)
        {
            createResult = client => new ClientStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }

        public Task<ClientStreamingResult<TRequest, TResponse>> Invoke(MagicOnionClientBase client, string path)
        {
            return Task.FromResult(createResult(client));
        }
    }

    public static class ClientStreamingMethodRawInvoker
    {
        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncClientStreamingCall((Method<TRequest, TResponse>)method, client.Options.Host, client.Options.CallOptions));

        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncClientStreamingCall((Method<TRequest, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions));

        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncClientStreamingCall((Method<Box<TRequest>, TResponse>)method, client.Options.Host, client.Options.CallOptions));

        public static ClientStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new ClientStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncClientStreamingCall((Method<Box<TRequest>, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions));
    }

    
    public class DuplexStreamingMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<MagicOnionClientBase, DuplexStreamingResult<TRequest, TResponse>> createResult;

        public DuplexStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncDuplexStreamingCall<TRequest, TResponse>> grpcCall)
        {
            createResult = client => new DuplexStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }
        public DuplexStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncDuplexStreamingCall<TRequest, Box<TResponse>>> grpcCall)
        {
            createResult = client => new DuplexStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }
        public DuplexStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncDuplexStreamingCall<Box<TRequest>, Box<TResponse>>> grpcCall)
        {
            createResult = client => new DuplexStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }
        public DuplexStreamingMethodRawInvoker(IMethod method, Func<MagicOnionClientBase, IMethod, AsyncDuplexStreamingCall<Box<TRequest>, TResponse>> grpcCall)
        {
            createResult = client => new DuplexStreamingResult<TRequest, TResponse>(grpcCall(client, method));
        }

        public Task<DuplexStreamingResult<TRequest, TResponse>> Invoke(MagicOnionClientBase client, string path)
        {
            return Task.FromResult(createResult(client));
        }
    }

    public static class DuplexStreamingMethodRawInvoker
    {
        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            where TResponse : class
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.DuplexStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<TRequest, TResponse>)method, client.Options.Host, client.Options.CallOptions));

        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_RefType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TRequest : class
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.DuplexStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<TRequest, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions));

        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.DuplexStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<Box<TRequest>, TResponse>)method, client.Options.Host, client.Options.CallOptions));

        public static DuplexStreamingMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new DuplexStreamingMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.DuplexStreaming, serviceName, name, serializerOptions),
                (client, method) => client.Options.CallInvoker.AsyncDuplexStreamingCall((Method<Box<TRequest>, Box<TResponse>>)method, client.Options.Host, client.Options.CallOptions));
    }
}