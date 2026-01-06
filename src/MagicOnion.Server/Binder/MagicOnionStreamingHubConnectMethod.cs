using System.Diagnostics;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Binder;

[DebuggerDisplay("MagicOnionStreamingHubConnectMethod: Service={ServiceName,nq}.{MethodName,nq}")]
public class MagicOnionStreamingHubConnectMethod<TService> : IMagicOnionGrpcMethod<TService> where TService : class
{
    public MethodType MethodType => MethodType.DuplexStreaming;
    public Type ServiceImplementationType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodHandlerMetadata Metadata { get; }

    public MagicOnionStreamingHubConnectMethod(string serviceName)
    {
        ServiceName = serviceName;
        MethodName = nameof(IStreamingHubBase.Connect);
        Metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata<TService>("MagicOnion.Server.Internal.IStreamingHubBase.Connect");
    }

    public MagicOnionStreamingHubConnectMethod(string serviceName, MethodHandlerMetadata metadata)
    {
        ServiceName = serviceName;
        MethodName = nameof(IStreamingHubBase.Connect);
        Metadata = metadata;
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindStreamingHub(this);
}
