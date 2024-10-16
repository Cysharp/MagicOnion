using System.Reflection;
using Grpc.Core;

namespace MagicOnion.Server.Binder;

public class MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, ServiceContext, TRequest, Task> invoker;

    public MethodType MethodType => MethodType.ServerStreaming;
    public Type ServiceType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodInfo MethodInfo { get; }

    public MagicOnionServerStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, Task<ServerStreamingResult<TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = invoker;
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindServerStreaming(this);

    public ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => new(invoker(service, context, request));
}
