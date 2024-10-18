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

    public MethodHandlerMetadata Metadata { get; } = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata(typeof(TService), typeof(TService).GetMethod("MagicOnion.Server.Internal.IStreamingHubBase.Connect")!);

    public MagicOnionStreamingHubConnectMethod(string serviceName)
    {
        ServiceName = serviceName;
        MethodName = nameof(IStreamingHubBase.Connect);
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindStreamingHub(this);
}
