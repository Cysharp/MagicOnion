using System.Reflection;
using MagicOnion.Internal;

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
