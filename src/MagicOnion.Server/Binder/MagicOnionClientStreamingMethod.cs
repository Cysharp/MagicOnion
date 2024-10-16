using System.Reflection;
using Grpc.Core;

namespace MagicOnion.Server.Binder;

public class MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, ServiceContext, Task<ClientStreamingResult<TRequest, TResponse>>> invoker;

    public MethodType MethodType => MethodType.ClientStreaming;
    public Type ServiceType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }

    public MethodInfo MethodInfo { get; }

    public MagicOnionClientStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, Task<ClientStreamingResult<TRequest, TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        MethodInfo = typeof(TService).GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        this.invoker = invoker;
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindClientStreaming(this);

    public ValueTask InvokeAsync(TService service, ServiceContext context)
        => SerializeValueTaskClientStreamingResult(invoker(service, context), context);

    static ValueTask SerializeValueTaskClientStreamingResult(Task<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
    {
        if (taskResult.IsCompletedSuccessfully)
        {
            if (taskResult.Result.hasRawValue)
            {
                context.Result = taskResult.Result.rawValue;
                return default;
            }
        }

        return Await(taskResult, context);

        static async ValueTask Await(Task<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
        {
            var result = await taskResult.ConfigureAwait(false);
            if (result.hasRawValue)
            {
                context.Result = result.rawValue;
            }
        }
    }
}
