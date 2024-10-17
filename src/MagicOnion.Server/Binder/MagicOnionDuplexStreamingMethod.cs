using System.Diagnostics;
using System.Reflection;
using Grpc.Core;

namespace MagicOnion.Server.Binder;

[DebuggerDisplay("MagicOnionDuplexStreamingMethod: {ServiceName,nq}.{MethodName,nq}; Implementation={typeof(TService).ToString(),nq}; Request={typeof(TRequest).ToString(),nq}; RawRequest={typeof(TRawRequest).ToString(),nq}; Response={typeof(TResponse).ToString(),nq}; RawResponse={typeof(TRawResponse).ToString(),nq}")]
public class MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, ServiceContext, Task> invoker;

    public MethodType MethodType => MethodType.DuplexStreaming;
    public Type ServiceImplementationType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodInfo MethodInfo { get; }

    public MagicOnionDuplexStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, Task> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = invoker;
    }

    public MagicOnionDuplexStreamingMethod(MagicOnionStreamingHubConnectMethod<TService> hubConnectMethod, Func<TService, ServiceContext, Task> invoker)
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
