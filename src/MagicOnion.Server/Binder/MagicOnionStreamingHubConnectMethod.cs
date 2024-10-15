using System.Reflection;
using Grpc.Core;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Binder;

public class MagicOnionStreamingHubConnectMethod<TService> : IMagicOnionGrpcMethod<TService> where TService : class
{
    public MethodType MethodType => MethodType.DuplexStreaming;
    public Type ServiceType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodInfo MethodInfo { get; }

    public MagicOnionStreamingHubConnectMethod(string serviceName)
    {
        ServiceName = serviceName;
        MethodName = nameof(IStreamingHubBase.Connect);
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindStreamingHub(this);
}
