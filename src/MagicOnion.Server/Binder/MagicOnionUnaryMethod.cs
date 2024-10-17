using System.Diagnostics;
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
    static readonly object BoxedNil = Nil.Default;

    public MethodType MethodType => MethodType.Unary;
    public Type ServiceImplementationType => typeof(TService);
    public string ServiceName => serviceName;
    public string MethodName => methodName;

    public MethodInfo MethodInfo { get; } = typeof(TService).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindUnary(this);

    public abstract ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request);

    protected static ValueTask SetUnaryResultNonGeneric(UnaryResult result, ServiceContext context)
    {
        if (result is { hasRawValue: true, rawTaskValue.IsCompletedSuccessfully: true })
        {
            return Await(result.rawTaskValue, context);
        }
        context.Result = BoxedNil;

        return default;

        static async ValueTask Await(Task task, ServiceContext context)
        {
            await task.ConfigureAwait(false);
            context.Result = BoxedNil;
        }
    }

    protected static ValueTask SetUnaryResult(UnaryResult<TResponse> result, ServiceContext context)
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

        static async ValueTask Await(Task<TResponse> task, ServiceContext context)
        {
            context.Result = await task.ConfigureAwait(false);
        }
    }
}

[DebuggerDisplay("MagicOnionUnaryMethod: {ServiceName,nq}.{MethodName,nq}; Implementation={typeof(TService).ToString(),nq}; Request={typeof(TRequest).ToString(),nq}; RawRequest={typeof(TRawRequest).ToString(),nq}; Response={typeof(TResponse).ToString(),nq}; RawResponse={typeof(TRawResponse).ToString(),nq}")]
public sealed class MagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse>(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, UnaryResult<TResponse>> invoker)
    : MagicOnionUnaryMethodBase<TService, TRequest, TResponse, TRawRequest, TRawResponse>(serviceName, methodName)
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{
    public override ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => SetUnaryResult(invoker(service, context, request), context);
}

[DebuggerDisplay("MagicOnionUnaryMethod: {ServiceName,nq}.{MethodName,nq}; Implementation={typeof(TService).ToString(),nq}; Request={typeof(TRequest).ToString(),nq}; RawRequest={typeof(TRawRequest).ToString(),nq}")]
public sealed class MagicOnionUnaryMethod<TService, TRequest, TRawRequest>(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, UnaryResult> invoker)
    : MagicOnionUnaryMethodBase<TService, TRequest, MessagePack.Nil, TRawRequest, Box<MessagePack.Nil>>(serviceName, methodName)
    where TService : class
    where TRawRequest : class
{
    public override ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => SetUnaryResultNonGeneric(invoker(service, context, request), context);
}
