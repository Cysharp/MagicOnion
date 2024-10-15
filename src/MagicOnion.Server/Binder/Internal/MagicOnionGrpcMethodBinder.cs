using System.Reflection;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Hubs.Internal;
using MagicOnion.Server.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Binder.Internal;

internal class MagicOnionGrpcMethodBinder<TService> : IMagicOnionGrpcMethodBinder<TService>
    where TService : class
{
    readonly ServiceMethodProviderContext<TService> providerContext;
    readonly IMagicOnionSerializerProvider messageSerializerProvider;

    readonly MagicOnionGrpcMethodHandler<TService> handlerBuilder;

    public MagicOnionGrpcMethodBinder(ServiceMethodProviderContext<TService> context, MagicOnionOptions options, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        this.providerContext = context;
        this.messageSerializerProvider = options.MessageSerializer;
        this.handlerBuilder = new MagicOnionGrpcMethodHandler<TService>(
            options.EnableCurrentContext,
            options.IsReturnExceptionStackTraceInErrorDetail,
            serviceProvider,
            options.GlobalFilters,
            loggerFactory.CreateLogger<MagicOnionGrpcMethodHandler<TService>>()
        );
    }

    public void BindUnary<TRequest, TResponse, TRawRequest, TRawResponse>(IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.Unary, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.Unary, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method);

        providerContext.AddUnaryMethod(grpcMethod, attrs, handlerBuilder.BuildUnaryMethod(method, messageSerializer, attrs));
    }

    public void BindClientStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.ClientStreaming, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ClientStreaming, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method);

        providerContext.AddClientStreamingMethod(grpcMethod, attrs, handlerBuilder.BuildClientStreamingMethod(method, messageSerializer, attrs));
    }

    public void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.ServerStreaming, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ServerStreaming, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method);

        providerContext.AddServerStreamingMethod(grpcMethod, attrs, handlerBuilder.BuildServerStreamingMethod(method, messageSerializer, attrs));
    }

    public void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.DuplexStreaming, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.DuplexStreaming, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method);

        providerContext.AddDuplexStreamingMethod(grpcMethod, attrs, handlerBuilder.BuildDuplexStreamingMethod(method, messageSerializer, attrs));
    }

    public void BindStreamingHub(MagicOnionStreamingHubConnectMethod<TService> method)
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.DuplexStreaming, default);
        // StreamingHub uses the special marshallers for streaming messages serialization.
        // TODO: Currently, MagicOnion expects TRawRequest/TRawResponse to be raw-byte array (`StreamingHubPayload`).
        var grpcMethod = new Method<StreamingHubPayload, StreamingHubPayload>(
            MethodType.DuplexStreaming,
            method.ServiceName,
            method.MethodName,
            MagicOnionMarshallers.StreamingHubMarshaller,
            MagicOnionMarshallers.StreamingHubMarshaller
        );
        var attrs = GetMetadataFromHandler(method);

        var duplexMethod = new MagicOnionDuplexStreamingMethod<TService, StreamingHubPayload, StreamingHubPayload, StreamingHubPayload, StreamingHubPayload>(
            method,
            static (instance, context) =>
            {
                context.CallContext.GetHttpContext().Features.Set<IStreamingHubFeature>(context.ServiceProvider.GetRequiredService<StreamingHubRegistry<TService>>());
                return ((IStreamingHubBase)instance).Connect();
            });
        providerContext.AddDuplexStreamingMethod(grpcMethod, attrs, handlerBuilder.BuildDuplexStreamingMethod(duplexMethod, messageSerializer, attrs));
    }

    IList<object> GetMetadataFromHandler(IMagicOnionGrpcMethod magicOnionGrpcMethod)
    {
        // NOTE: We need to collect Attributes for Endpoint metadata. ([Authorize], [AllowAnonymous] ...)
        // https://github.com/grpc/grpc-dotnet/blob/7ef184f3c4cd62fbc3cde55e4bb3e16b58258ca1/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs#L89-L98
        var metadata = new List<object>();
        metadata.AddRange(magicOnionGrpcMethod.ServiceType.GetCustomAttributes(inherit: true));
        metadata.AddRange(magicOnionGrpcMethod.MethodInfo.GetCustomAttributes(inherit: true));

        metadata.Add(new HttpMethodMetadata(["POST"], acceptCorsPreflight: true));
        return metadata;
    }
}
