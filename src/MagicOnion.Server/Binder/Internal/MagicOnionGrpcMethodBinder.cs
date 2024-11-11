using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Features.Internal;
using MagicOnion.Server.Hubs.Internal;
using MagicOnion.Server.Internal;
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
        var messageSerializer = messageSerializerProvider.Create(MethodType.Unary, method.Metadata.ServiceImplementationMethod);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.Unary, method.ServiceName, method.MethodName, messageSerializer);

        providerContext.AddUnaryMethod(grpcMethod, method.Metadata.Metadata.ToArray(), handlerBuilder.BuildUnaryMethod(method, messageSerializer));
    }

    public void BindClientStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.ClientStreaming, method.Metadata.ServiceImplementationMethod);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ClientStreaming, method.ServiceName, method.MethodName, messageSerializer);

        providerContext.AddClientStreamingMethod(grpcMethod, method.Metadata.Metadata.ToArray(), handlerBuilder.BuildClientStreamingMethod(method, messageSerializer));
    }

    public void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.ServerStreaming, method.Metadata.ServiceImplementationMethod);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ServerStreaming, method.ServiceName, method.MethodName, messageSerializer);

        providerContext.AddServerStreamingMethod(grpcMethod, method.Metadata.Metadata.ToArray(), handlerBuilder.BuildServerStreamingMethod(method, messageSerializer));
    }

    public void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.DuplexStreaming, method.Metadata.ServiceImplementationMethod);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.DuplexStreaming, method.ServiceName, method.MethodName, messageSerializer);

        providerContext.AddDuplexStreamingMethod(grpcMethod, method.Metadata.Metadata.ToArray(), handlerBuilder.BuildDuplexStreamingMethod(method, messageSerializer));
    }

    public void BindStreamingHub(MagicOnionStreamingHubConnectMethod<TService> method)
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.DuplexStreaming, method.Metadata.ServiceImplementationMethod);
        // StreamingHub uses the special marshallers for streaming messages serialization.
        // TODO: Currently, MagicOnion expects TRawRequest/TRawResponse to be raw-byte array (`StreamingHubPayload`).
        var grpcMethod = new Method<StreamingHubPayload, StreamingHubPayload>(
            MethodType.DuplexStreaming,
            method.ServiceName,
            method.MethodName,
            MagicOnionMarshallers.StreamingHubMarshaller,
            MagicOnionMarshallers.StreamingHubMarshaller
        );

        var duplexMethod = new MagicOnionDuplexStreamingMethod<TService, StreamingHubPayload, StreamingHubPayload, StreamingHubPayload, StreamingHubPayload>(
            method,
            static (instance, context) =>
            {
                context.CallContext.GetHttpContext().Features.Set<IStreamingHubFeature>(context.ServiceProvider.GetRequiredService<StreamingHubRegistry<TService>>());
                return ((IStreamingHubBase)instance).Connect();
            });
        providerContext.AddDuplexStreamingMethod(grpcMethod, method.Metadata.Metadata.ToArray(), handlerBuilder.BuildDuplexStreamingMethod(duplexMethod, messageSerializer));
    }
}
