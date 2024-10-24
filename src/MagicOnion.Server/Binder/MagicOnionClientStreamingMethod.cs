using System.Diagnostics;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Binder;

[DebuggerDisplay("MagicOnionClientStreamingMethod: {ServiceName,nq}.{MethodName,nq}; Implementation={typeof(TService).ToString(),nq}; Request={typeof(TRequest).ToString(),nq}; RawRequest={typeof(TRawRequest).ToString(),nq}; Response={typeof(TResponse).ToString(),nq}; RawResponse={typeof(TRawResponse).ToString(),nq}")]
public class MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{

    readonly Func<TService, ServiceContext, Task<ClientStreamingResult<TRequest, TResponse>>> invoker;

    public MethodType MethodType => MethodType.ClientStreaming;
    public Type ServiceImplementationType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }
    public MethodHandlerMetadata Metadata { get; }

    public MagicOnionClientStreamingMethod(string serviceName, string methodName, Func<TService, ServiceContext, Task<ClientStreamingResult<TRequest, TResponse>>> invoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        Metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata<TService>(methodName);

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
