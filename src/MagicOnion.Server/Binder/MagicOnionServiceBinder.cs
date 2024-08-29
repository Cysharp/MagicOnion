using System.Diagnostics;
using System.Runtime.CompilerServices;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Internal;
using MessagePack;
using Microsoft.AspNetCore.Routing;

namespace MagicOnion.Server.Binder;

internal record MagicOnionMethodBindingContext(IMagicOnionServiceBinder Binder, MethodHandler MethodHandler);

internal interface IMagicOnionServiceBinder
{
    void BindUnary<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<TRequest, ServerCallContext, ValueTask<object?>> serverMethod)
        where TRawRequest : class
        where TRawResponse : class;

    void BindUnaryParameterless<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<Nil, ServerCallContext, ValueTask<object?>> serverMethod)
        where TRawRequest : class
        where TRawResponse : class;

    void BindStreamingHub<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, ValueTask> serverMethod)
        where TRawRequest : class
        where TRawResponse : class;

    void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, ValueTask> serverMethod)
        where TRawRequest : class
        where TRawResponse : class;

    void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, ValueTask> serverMethod)
        where TRawRequest : class
        where TRawResponse : class;

    void BindClientStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<IAsyncStreamReader<TRequest>, ServerCallContext, ValueTask<TResponse?>> serverMethod)
        where TRawRequest : class
        where TRawResponse : class;
}

internal class MagicOnionServiceBinder<TService> : IMagicOnionServiceBinder
    where TService : class
{
    readonly ServiceMethodProviderContext<TService> context;

    public MagicOnionServiceBinder(ServiceMethodProviderContext<TService> context)
    {
        this.context = context;
    }

    IList<object> GetMetadataFromHandler(MethodHandler methodHandler)
    {
        var serviceType = methodHandler.ServiceType;

        // NOTE: We need to collect Attributes for Endpoint metadata. ([Authorize], [AllowAnonymous] ...)
        // https://github.com/grpc/grpc-dotnet/blob/7ef184f3c4cd62fbc3cde55e4bb3e16b58258ca1/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs#L89-L98
        var metadata = new List<object>();
        metadata.AddRange(serviceType.GetCustomAttributes(inherit: true));
        metadata.AddRange(methodHandler.MethodInfo.GetCustomAttributes(inherit: true));

        metadata.Add(new HttpMethodMetadata(["POST"], acceptCorsPreflight: true));
        return metadata;
    }


    public void BindUnary<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<TRequest, ServerCallContext, ValueTask<object?>> serverMethod)
        where TRawRequest : class
        where TRawResponse : class
    {
        var method = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.Unary, ctx.MethodHandler.ServiceName, ctx.MethodHandler.MethodName, ctx.MethodHandler.MethodInfo, ctx.MethodHandler.MessageSerializer);
        UnaryServerMethod<TService, TRawRequest, TRawResponse> invoker = async (_, request, context) =>
        {
            var response = await serverMethod(GrpcMethodHelper.FromRaw<TRawRequest, TRequest>(request), context);
            if (response is RawBytesBox rawBytesResponse)
            {
                return Unsafe.As<RawBytesBox, TRawResponse>(ref rawBytesResponse); // NOTE: To disguise an object as a `TRawResponse`, `TRawResponse` must be `class`.
            }

            return GrpcMethodHelper.ToRaw<TResponse, TRawResponse>((TResponse?)response!);
        };

        context.AddUnaryMethod(method, GetMetadataFromHandler(ctx.MethodHandler), invoker);
    }

    public void BindUnaryParameterless<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<Nil, ServerCallContext, ValueTask<object?>> serverMethod)
        where TRawRequest : class
        where TRawResponse : class
    {
        // WORKAROUND: Prior to MagicOnion 5.0, the request type for the parameter-less method was byte[].
        //             DynamicClient sends byte[], but GeneratedClient sends Nil, which is incompatible,
        //             so as a special case we do not serialize/deserialize and always convert to a fixed values.
        var method = GrpcMethodHelper.CreateMethod<TResponse, TRawResponse>(MethodType.Unary, ctx.MethodHandler.ServiceName, ctx.MethodHandler.MethodName, ctx.MethodHandler.MethodInfo, ctx.MethodHandler.MessageSerializer);
        UnaryServerMethod<TService, Box<Nil>, TRawResponse> invoker = async (_, request, context) =>
        {
            var response = await serverMethod(GrpcMethodHelper.FromRaw<Box<Nil>, Nil>(request), context);
            if (response is RawBytesBox rawBytesResponse)
            {
                return Unsafe.As<RawBytesBox, TRawResponse>(ref rawBytesResponse); // NOTE: To disguise an object as a `TRawResponse`, `TRawResponse` must be `class`.
            }

            return GrpcMethodHelper.ToRaw<TResponse, TRawResponse>((TResponse?)response!);
        };

        context.AddUnaryMethod(method, GetMetadataFromHandler(ctx.MethodHandler), invoker);
    }

    public void BindStreamingHub<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, ValueTask> serverMethod)
        where TRawRequest : class
        where TRawResponse : class
    {
        Debug.Assert(typeof(TRequest) == typeof(StreamingHubPayload));
        Debug.Assert(typeof(TResponse) == typeof(StreamingHubPayload));
        // StreamingHub uses the special marshallers for streaming messages serialization.
        // TODO: Currently, MagicOnion expects TRawRequest/TRawResponse to be raw-byte array (`StreamingHubPayload`).
        var method = new Method<TRawRequest, TRawResponse>(
            MethodType.DuplexStreaming,
            ctx.MethodHandler.ServiceName,
            ctx.MethodHandler.MethodName,
            (Marshaller<TRawRequest>)(object)MagicOnionMarshallers.StreamingHubMarshaller,
            (Marshaller<TRawResponse>)(object)MagicOnionMarshallers.StreamingHubMarshaller
        );
        DuplexStreamingServerMethod<TService, TRawRequest, TRawResponse> invoker = async (_, request, response, context) => await serverMethod(
            new MagicOnionAsyncStreamReader<TRequest, TRawRequest>(request),
            new MagicOnionServerStreamWriter<TResponse, TRawResponse>(response),
            context
        );

        context.AddDuplexStreamingMethod(method, GetMetadataFromHandler(ctx.MethodHandler), invoker);
    }

    public void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, ValueTask> serverMethod)
        where TRawRequest : class
        where TRawResponse : class
    {
        var method = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.DuplexStreaming, ctx.MethodHandler.ServiceName, ctx.MethodHandler.MethodName, ctx.MethodHandler.MethodInfo, ctx.MethodHandler.MessageSerializer);
        DuplexStreamingServerMethod<TService, TRawRequest, TRawResponse> invoker = async (_, request, response, context) => await serverMethod(
            new MagicOnionAsyncStreamReader<TRequest, TRawRequest>(request),
            new MagicOnionServerStreamWriter<TResponse, TRawResponse>(response),
            context
        );

        context.AddDuplexStreamingMethod(method, GetMetadataFromHandler(ctx.MethodHandler), invoker);
    }

    public void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, ValueTask> serverMethod)
        where TRawRequest : class
        where TRawResponse : class
    {
        var method = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ServerStreaming, ctx.MethodHandler.ServiceName, ctx.MethodHandler.MethodName, ctx.MethodHandler.MethodInfo, ctx.MethodHandler.MessageSerializer);
        ServerStreamingServerMethod<TService, TRawRequest, TRawResponse> invoker = async (_, request, response, context) => await serverMethod(
            GrpcMethodHelper.FromRaw<TRawRequest, TRequest>(request),
            new MagicOnionServerStreamWriter<TResponse, TRawResponse>(response),
            context
        );

        context.AddServerStreamingMethod(method, GetMetadataFromHandler(ctx.MethodHandler), invoker);
    }

    public void BindClientStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionMethodBindingContext ctx, Func<IAsyncStreamReader<TRequest>, ServerCallContext, ValueTask<TResponse?>> serverMethod)
        where TRawRequest : class
        where TRawResponse : class
    {
        var method = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ClientStreaming, ctx.MethodHandler.ServiceName, ctx.MethodHandler.MethodName, ctx.MethodHandler.MethodInfo, ctx.MethodHandler.MessageSerializer);
        ClientStreamingServerMethod<TService, TRawRequest, TRawResponse> invoker = async (_, request, context) => GrpcMethodHelper.ToRaw<TResponse, TRawResponse>((await serverMethod(
            new MagicOnionAsyncStreamReader<TRequest, TRawRequest>(request),
            context
        ))!);

        context.AddClientStreamingMethod(method, GetMetadataFromHandler(ctx.MethodHandler), invoker);
    }
}
