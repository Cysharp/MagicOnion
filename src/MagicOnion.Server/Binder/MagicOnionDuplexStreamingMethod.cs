using System.Reflection;
using Grpc.Core;

namespace MagicOnion.Server.Binder;

public class MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, ServiceContext, ValueTask> invoker;

    public MethodType MethodType => MethodType.DuplexStreaming;
    public Type ServiceType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodInfo MethodInfo { get; }

    public MagicOnionDuplexStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, DuplexStreamingResult<TRequest, TResponse>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = (service, context) =>
        {
            invoker(service, context);
            return default;
        };
    }

    public MagicOnionDuplexStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, Task<DuplexStreamingResult<TRequest, TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = (service, context) => new ValueTask(invoker(service, context));
    }

    public MagicOnionDuplexStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, ValueTask<DuplexStreamingResult<TRequest, TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = async (service, context) => await invoker(service, context);
    }

    public MagicOnionDuplexStreamingMethod(MagicOnionStreamingHubConnectMethod<TService> hubConnectMethod, Func<TService, ServiceContext, Task<DuplexStreamingResult<TRequest, TResponse>>> invoker)
    {
        ServiceName = hubConnectMethod.ServiceName;
        MethodName = hubConnectMethod.MethodName;
        MethodInfo = hubConnectMethod.MethodInfo;

        this.invoker = (service, context) => new ValueTask(invoker(service, context));
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindDuplexStreaming(this);

    public ValueTask InvokeAsync(TService service, ServiceContext context)
        => invoker(service, context);
}
