using System.Diagnostics;
using Grpc.Core;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Binder;

[DebuggerDisplay("MagicOnionServerStreamingMethod: {ServiceName,nq}.{MethodName,nq}; Implementation={typeof(TService).ToString(),nq}; Request={typeof(TRequest).ToString(),nq}; RawRequest={typeof(TRawRequest).ToString(),nq}; Response={typeof(TResponse).ToString(),nq}; RawResponse={typeof(TRawResponse).ToString(),nq}")]
public class MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, ServiceContext, TRequest, Task> invoker;

    public MethodType MethodType => MethodType.ServerStreaming;
    public Type ServiceImplementationType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }
    public MethodHandlerMetadata Metadata { get; }

    public MagicOnionServerStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, Task> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        Metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata<TService>(methodName);

        this.invoker = invoker;
    }

    public MagicOnionServerStreamingMethod(string serviceName, string methodName, MethodHandlerMetadata metadata, Func<TService, ServiceContext, TRequest, Task> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        Metadata = metadata;

        this.invoker = invoker;
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindServerStreaming(this);

    public ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => new(invoker(service, context, request));
}
