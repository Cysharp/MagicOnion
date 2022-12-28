using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Server.Internal;
using Microsoft.AspNetCore.Routing;

namespace MagicOnion.Server.Glue;

internal class MagicOnionGlueServiceBinder<TService> : ServiceBinderBase
    where TService : class
{
    readonly ServiceMethodProviderContext<TService> context;

    public MagicOnionGlueServiceBinder(ServiceMethodProviderContext<TService> context)
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

        metadata.Add(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));
        return metadata;
    }

    public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
    {
        context.AddUnaryMethod(method, GetMetadataFromHandler(((MagicOnionServerMethod<TRequest, TResponse>)method).MagicOnionMethodHandler), (_, request, context) => handler(request, context));
    }

    public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ClientStreamingServerMethod<TRequest, TResponse> handler)
    {
        context.AddClientStreamingMethod(method, GetMetadataFromHandler(((MagicOnionServerMethod<TRequest, TResponse>)method).MagicOnionMethodHandler), (_, request, context) => handler(request, context));
    }

    public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServerStreamingServerMethod<TRequest, TResponse> handler)
    {
        context.AddServerStreamingMethod(method, GetMetadataFromHandler(((MagicOnionServerMethod<TRequest, TResponse>)method).MagicOnionMethodHandler), (_, request, stream, context) => handler(request, stream, context));
    }

    public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, DuplexStreamingServerMethod<TRequest, TResponse> handler)
    {
        context.AddDuplexStreamingMethod(method, GetMetadataFromHandler(((MagicOnionServerMethod<TRequest, TResponse>)method).MagicOnionMethodHandler), (_, request, stream, context) => handler(request, stream, context));
    }
}
