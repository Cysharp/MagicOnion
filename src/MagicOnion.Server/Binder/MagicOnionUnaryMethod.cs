using System.Reflection;
using Grpc.Core;
using MagicOnion.Internal;
using MessagePack;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{
    ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request);
}

public abstract class MagicOnionUnaryMethodBase<TService, TRequest, TResponse, TRawRequest, TRawResponse>(string serviceName, string methodName)
    : IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{
    public MethodType MethodType => MethodType.Unary;
    public Type ServiceType => typeof(TService);
    public string ServiceName => serviceName;
    public string MethodName => methodName;

    public MethodInfo MethodInfo { get; } = typeof(TService).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindUnary(this);

    public abstract ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request);
}

public sealed class MagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse>(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, UnaryResult<TResponse>> invoker)
    : MagicOnionUnaryMethodBase<TService, TRequest, TResponse, TRawRequest, TRawResponse>(serviceName, methodName)
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{
    public override ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => MethodHandlerResultHelper.SetUnaryResult(invoker(service, context, request), context);
}

public sealed class MagicOnionUnaryMethod<TService, TRequest, TRawRequest>(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, UnaryResult> invoker)
    : MagicOnionUnaryMethodBase<TService, TRequest, MessagePack.Nil, TRawRequest, Box<MessagePack.Nil>>(serviceName, methodName)
    where TService : class
    where TRawRequest : class
{
    public override ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => MethodHandlerResultHelper.SetUnaryResultNonGeneric(invoker(service, context, request), context);
}

internal class MethodHandlerResultHelper
{
    static readonly object BoxedNil = Nil.Default;

    public static ValueTask NewEmptyValueTask<T>(T result)
        => default;

    public static ValueTask TaskToEmptyValueTask<T>(Task<T> result)
        => new(result);

    public static ValueTask SetUnaryResultNonGeneric(UnaryResult result, ServiceContext context)
    {
        if (result.hasRawValue)
        {
            if (result.rawTaskValue is { IsCompletedSuccessfully: true })
            {
                return Await(result.rawTaskValue, context);
            }
            context.Result = BoxedNil;
        }

        return default;

        static async ValueTask Await(Task task, ServiceContext context)
        {
            await task.ConfigureAwait(false);
            context.Result = BoxedNil;
        }
    }

    public static ValueTask SetUnaryResult<T>(UnaryResult<T> result, ServiceContext context)
    {
        if (result.hasRawValue)
        {
            if (result.rawTaskValue is { } task)
            {
                if (task.IsCompletedSuccessfully)
                {
                    context.Result = task.Result;
                }
                else
                {
                    return Await(task, context);
                }
            }
            else
            {
                context.Result = result.rawValue;
            }
        }

        return default;

        static async ValueTask Await(Task<T> task, ServiceContext context)
        {
            context.Result = await task.ConfigureAwait(false);
        }
    }

    public static async ValueTask SetTaskUnaryResult<T>(Task<UnaryResult<T>> taskResult, ServiceContext context)
    {
        var result = await taskResult.ConfigureAwait(false);
        if (result.hasRawValue)
        {
            context.Result = (result.rawTaskValue != null) ? await result.rawTaskValue.ConfigureAwait(false) : result.rawValue;
        }
    }

    public static ValueTask SerializeClientStreamingResult<TRequest, TResponse>(ClientStreamingResult<TRequest, TResponse> result, ServiceContext context)
        => SerializeValueTaskClientStreamingResult(new ValueTask<ClientStreamingResult<TRequest, TResponse>>(result), context);

    public static ValueTask SerializeTaskClientStreamingResult<TRequest, TResponse>(Task<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
        => SerializeValueTaskClientStreamingResult(new ValueTask<ClientStreamingResult<TRequest, TResponse>>(taskResult), context);

    public static ValueTask SerializeValueTaskClientStreamingResult<TRequest, TResponse>(ValueTask<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
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

        static async ValueTask Await(ValueTask<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
        {
            var result = await taskResult.ConfigureAwait(false);
            if (result.hasRawValue)
            {
                context.Result = result.rawValue;
            }
        }
    }
}
