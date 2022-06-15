using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Client.Internal
{
    // Pubternal API: This class is used from generated clients and is therefore `public` but internal API.
    public class UnaryMethodRawInvoker<TRequest, TResponse>
    {
        readonly Func<RequestContext, ResponseContext> createResponseContext;

        public UnaryMethodRawInvoker(IMethod method, Func<RequestContext, IMethod, AsyncUnaryCall<TResponse>> asyncUnaryCall)
        {
            createResponseContext = context => new ResponseContext<TResponse>(asyncUnaryCall(context, method));
        }
        public UnaryMethodRawInvoker(IMethod method, Func<RequestContext, IMethod, AsyncUnaryCall<GrpcMethodHelper.Box<TResponse>>> asyncUnaryCall)
        {
            createResponseContext = context => new ResponseContext<TResponse>(asyncUnaryCall(context, method));
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
                (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<TRequest, GrpcMethodHelper.Box<TResponse>>)method, context.Client.Options.Host, context.CallOptions, ((RequestContext<TRequest>)context).Request));

        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_ValueType_RefType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            where TResponse : class
            => new UnaryMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions),
                (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<GrpcMethodHelper.Box<TRequest>, TResponse>)method, context.Client.Options.Host, context.CallOptions, new GrpcMethodHelper.Box<TRequest>(((RequestContext<TRequest>)context).Request)));
        public static UnaryMethodRawInvoker<TRequest, TResponse> Create_ValueType_ValueType<TRequest, TResponse>(string serviceName, string name, MessagePackSerializerOptions serializerOptions)
            => new UnaryMethodRawInvoker<TRequest, TResponse>(
                GrpcMethodHelper.CreateMethod<TRequest, TResponse>(MethodType.Unary, serviceName, name, serializerOptions),
                (context, method) => context.Client.Options.CallInvoker.AsyncUnaryCall((Method<GrpcMethodHelper.Box<TRequest>, GrpcMethodHelper.Box<TResponse>>)method, context.Client.Options.Host, context.CallOptions, new GrpcMethodHelper.Box<TRequest>(((RequestContext<TRequest>)context).Request)));
    }
}