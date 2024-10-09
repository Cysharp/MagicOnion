using System.Reflection;

namespace MagicOnion.Server.Binder;

public class MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, TRequest, ServiceContext, ValueTask> invoker;

    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodInfo MethodInfo { get; }

    public MagicOnionServerStreamingMethod(string serviceName, string methodName, Func<TService, TRequest, ServiceContext, ServerStreamingResult<TResponse>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = (service, request, context) =>
        {
            invoker(service, request, context);
            return default;
        };
    }

    public MagicOnionServerStreamingMethod(string serviceName, string methodName, Func<TService, TRequest, ServiceContext, Task<ServerStreamingResult<TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = (service, request, context) => new ValueTask(invoker(service, request, context));
    }

    public MagicOnionServerStreamingMethod(string serviceName, string methodName, Func<TService, TRequest, ServiceContext, ValueTask<ServerStreamingResult<TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = async (service, request, context) => await invoker(service, request, context);
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindServerStreaming(this);

    public ValueTask InvokeAsync(TService service, TRequest request, ServiceContext context)
        => invoker(service, request, context);
}
