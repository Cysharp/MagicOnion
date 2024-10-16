using System.Reflection;
using Grpc.Core;

namespace MagicOnion.Server.Binder;

public class MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, ServiceContext, Task> invoker;

    public MethodType MethodType => MethodType.DuplexStreaming;
    public Type ServiceType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodInfo MethodInfo { get; }

    public MagicOnionDuplexStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, Task<DuplexStreamingResult<TRequest, TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = invoker;
    }

    public MagicOnionDuplexStreamingMethod(MagicOnionStreamingHubConnectMethod<TService> hubConnectMethod, Func<TService, ServiceContext, Task<DuplexStreamingResult<TRequest, TResponse>>> invoker)
    {
        ServiceName = hubConnectMethod.ServiceName;
        MethodName = hubConnectMethod.MethodName;
        MethodInfo = hubConnectMethod.MethodInfo;

        this.invoker = invoker;
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindDuplexStreaming(this);

    public ValueTask InvokeAsync(TService service, ServiceContext context)
        => new(invoker(service, context));
}
