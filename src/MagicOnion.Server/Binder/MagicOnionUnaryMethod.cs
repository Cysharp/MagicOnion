using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Server.Internal;
using MessagePack;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> : IMagicOnionGrpcMethod<TService>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{
    ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request);
}

public abstract class MagicOnionUnaryMethodBase<TService, TRequest, TResponse, TRawRequest, TRawResponse>
    : IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{
    static readonly object BoxedNil = Nil.Default;

    public MethodType MethodType => MethodType.Unary;
    public Type ServiceImplementationType => typeof(TService);
    public string ServiceName { get; }
    public string MethodName { get; }
    public MethodHandlerMetadata Metadata { get; }

    protected MagicOnionUnaryMethodBase(string serviceName, string methodName)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        Metadata = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata<TService>(methodName);
    }

    protected MagicOnionUnaryMethodBase(string serviceName, string methodName, MethodHandlerMetadata metadata)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        Metadata = metadata;
    }

    public void Bind(IMagicOnionGrpcMethodBinder<TService> binder)
        => binder.BindUnary(this);

    public abstract ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request);

    protected static ValueTask SetUnaryResultNonGeneric(UnaryResult result, ServiceContext context)
    {
        if (result.HasRawValue)
        {
            context.Result = BoxedNil;
        }
        else
        {
            return Await(result, context);
        }

        return default;

        static async ValueTask Await(UnaryResult task, ServiceContext context)
        {
            await task.ConfigureAwait(false);
            context.Result = BoxedNil;
        }
    }

    protected static ValueTask SetUnaryResult(UnaryResult<TResponse> result, ServiceContext context)
    {
        if (result.HasRawValue)
        {
            context.Result = result.GetAwaiter().GetResult();
        }
        else
        {
            return Await(result, context);
        }

        return default;

        static async ValueTask Await(UnaryResult<TResponse> task, ServiceContext context)
        {
            context.Result = await task.ConfigureAwait(false);
        }
    }
}

[DebuggerDisplay("MagicOnionUnaryMethod: {ServiceName,nq}.{MethodName,nq}; Implementation={typeof(TService).ToString(),nq}; Request={typeof(TRequest).ToString(),nq}; RawRequest={typeof(TRawRequest).ToString(),nq}; Response={typeof(TResponse).ToString(),nq}; RawResponse={typeof(TRawResponse).ToString(),nq}")]
public sealed class MagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse>
    : MagicOnionUnaryMethodBase<TService, TRequest, TResponse, TRawRequest, TRawResponse>
    where TService : class
    where TRawRequest : class
    where TRawResponse : class
{
    readonly Func<TService, ServiceContext, TRequest, UnaryResult<TResponse>> invoker;

    public MagicOnionUnaryMethod(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, UnaryResult<TResponse>> invoker)
        : base(serviceName, methodName)
    {
        this.invoker = invoker;
    }

    public MagicOnionUnaryMethod(string serviceName, string methodName, MethodHandlerMetadata metadata, Func<TService, ServiceContext, TRequest, UnaryResult<TResponse>> invoker)
        : base(serviceName, methodName, metadata)
    {
        this.invoker = invoker;
    }

    public override ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => SetUnaryResult(invoker(service, context, request), context);
}

[DebuggerDisplay("MagicOnionUnaryMethod: {ServiceName,nq}.{MethodName,nq}; Implementation={typeof(TService).ToString(),nq}; Request={typeof(TRequest).ToString(),nq}; RawRequest={typeof(TRawRequest).ToString(),nq}")]
public sealed class MagicOnionUnaryMethod<TService, TRequest, TRawRequest>
    : MagicOnionUnaryMethodBase<TService, TRequest, MessagePack.Nil, TRawRequest, Box<MessagePack.Nil>>
    where TService : class
    where TRawRequest : class
{
    readonly Func<TService, ServiceContext, TRequest, UnaryResult> invoker;

    public MagicOnionUnaryMethod(string serviceName, string methodName, Func<TService, ServiceContext, TRequest, UnaryResult> invoker)
        : base(serviceName, methodName)
    {
        this.invoker = invoker;
    }

    public MagicOnionUnaryMethod(string serviceName, string methodName, MethodHandlerMetadata metadata, Func<TService, ServiceContext, TRequest, UnaryResult> invoker)
        : base(serviceName, methodName, metadata)
    {
        this.invoker = invoker;
    }

    public override ValueTask InvokeAsync(TService service, ServiceContext context, TRequest request)
        => SetUnaryResultNonGeneric(invoker(service, context, request), context);
}
