using Grpc.Core;
using MessagePack;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using MagicOnion.Internal;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server;

public class MethodHandler : IEquatable<MethodHandler>
{
    // reflection cache
    static readonly MethodInfo createService = typeof(ServiceProviderHelper).GetMethod(nameof(ServiceProviderHelper.CreateService), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;

    static int methodHandlerIdBuild = 0;

    readonly int methodHandlerId = 0;

    readonly bool isStreamingHub;
    readonly MessagePackSerializerOptions serializerOptions;
    readonly bool responseIsTask;

    readonly Func<ServiceContext, ValueTask> methodBody;
    
    // options

    internal readonly bool isReturnExceptionStackTraceInErrorDetail;
    internal readonly IMagicOnionLogger logger;
    readonly bool enableCurrentContext;

    public string ServiceName { get; }
    public string MethodName { get; }
    public Type ServiceType { get; }
    public MethodInfo MethodInfo { get; }
    public MethodType MethodType { get; }

    public ILookup<Type, Attribute> AttributeLookup { get; }

    // use for request handling.

    public readonly Type RequestType;
    public readonly Type UnwrappedResponseType;

    public MethodHandler(Type classType, MethodInfo methodInfo, string methodName, MethodHandlerOptions handlerOptions, IServiceProvider serviceProvider, IMagicOnionLogger logger, bool isStreamingHub)
    {
        this.methodHandlerId = Interlocked.Increment(ref methodHandlerIdBuild);

        var serviceInterfaceType = classType.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>)).GetGenericArguments()[0];

        this.ServiceType = classType;
        this.ServiceName = serviceInterfaceType.Name;
        this.MethodInfo = methodInfo;
        this.MethodName = methodName;
        MethodType mt;
        this.UnwrappedResponseType = UnwrapResponseType(methodInfo, out mt, out responseIsTask, out var requestType);
        this.MethodType = mt;
        this.serializerOptions = handlerOptions.SerializerOptions;
        this.isStreamingHub = isStreamingHub;

        var parameters = methodInfo.GetParameters();
        if (requestType == null)
        {
            var resolver = this.serializerOptions.Resolver;
            requestType = MagicOnionMarshallers.CreateRequestTypeAndSetResolver(classType.Name + "/" + methodInfo.Name, parameters, ref resolver);
            this.serializerOptions = this.serializerOptions.WithResolver(resolver);
        }

        this.RequestType = requestType;

        this.AttributeLookup = classType.GetCustomAttributes(true)
            .Concat(methodInfo.GetCustomAttributes(true))
            .Cast<Attribute>()
            .ToLookup(x => x.GetType());

        var filters = FilterHelper.GetFilters(handlerOptions.GlobalFilters, classType, methodInfo);

        // options
        this.isReturnExceptionStackTraceInErrorDetail = handlerOptions.IsReturnExceptionStackTraceInErrorDetail;
        this.logger = logger;
        this.enableCurrentContext = handlerOptions.EnableCurrentContext;

        // prepare lambda parameters
        var createServiceMethodInfo = createService.MakeGenericMethod(classType, serviceInterfaceType);
        var contextArg = Expression.Parameter(typeof(ServiceContext), "context");
        var instance = Expression.Call(createServiceMethodInfo, contextArg);

        switch (MethodType)
        {
            case MethodType.Unary:
            case MethodType.ServerStreaming:
                // (ServiceContext context) =>
                // {
                //      var request = (TRequest)context.Request;
                //      var result = new FooService() { Context = context }.Bar(request.Item1, request.Item2);
                //      return MethodHandlerResultHelper.SetUnaryResult(result, context);
                // };
                try
                {
                    var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    var staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

                    var requestArg = Expression.Parameter(RequestType, "request");
                    var contextRequest = Expression.Property(contextArg, typeof(ServiceContext).GetProperty("Request", flags)!);
                    var assignRequest = Expression.Assign(requestArg, Expression.Convert(contextRequest, RequestType));

                    Expression[] arguments = new Expression[parameters.Length];
                    if (parameters.Length == 1)
                    {
                        arguments[0] = requestArg;
                    }
                    else
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            arguments[i] = Expression.Field(requestArg, "Item" + (i + 1));
                        }
                    }

                    var callBody = Expression.Call(instance, methodInfo, arguments);

                    if (MethodType == MethodType.ServerStreaming)
                    {
                        var finalMethod = (responseIsTask)
                            ? typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.TaskToEmptyValueTask), staticFlags)!.MakeGenericMethod(MethodInfo.ReturnType.GetGenericArguments()[0]) // Task<ServerStreamingResult<TResponse>>
                            : typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.NewEmptyValueTask), staticFlags)!.MakeGenericMethod(MethodInfo.ReturnType); // ServerStreamingResult<TResponse>
                        callBody = Expression.Call(finalMethod, callBody);
                    }
                    else
                    {
                        var finalMethod = (responseIsTask)
                            ? typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SetTaskUnaryResult), staticFlags)!.MakeGenericMethod(UnwrappedResponseType)
                            : typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SetUnaryResult), staticFlags)!.MakeGenericMethod(UnwrappedResponseType);
                        callBody = Expression.Call(finalMethod, callBody, contextArg);
                    }

                    var body = Expression.Block(new[] { requestArg }, assignRequest, callBody);
                    var compiledBody = Expression.Lambda(body, contextArg).Compile();

                    this.methodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (Func<ServiceContext, ValueTask>)compiledBody);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
                }
                break;
            case MethodType.ClientStreaming:
            case MethodType.DuplexStreaming:
                if (parameters.Length != 0)
                {
                    throw new InvalidOperationException($"{MethodType} does not support method parameters. If you need to send initial parameter, use header instead. Path:{ToString()}");
                }

                // (ServiceContext context) => new FooService() { Context = context }.Bar();
                try
                {
                    var body = Expression.Call(instance, methodInfo);
                    var staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

                    if (MethodType == MethodType.ClientStreaming)
                    {
                        var finalMethod = (responseIsTask)
                            ? typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SerializeTaskClientStreamingResult), staticFlags)!.MakeGenericMethod(RequestType, UnwrappedResponseType)
                            : typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SerializeClientStreamingResult), staticFlags)!.MakeGenericMethod(RequestType, UnwrappedResponseType);
                        body = Expression.Call(finalMethod, body, contextArg);
                    }
                    else
                    {
                        if (!responseIsTask)
                        {
                            body = Expression.Call(typeof(MethodHandlerResultHelper)
                                .GetMethod(nameof(MethodHandlerResultHelper.NewEmptyValueTask), staticFlags)!
                                .MakeGenericMethod(MethodInfo.ReturnType), body);
                        }
                        else
                        {
                            body = Expression.Call(typeof(MethodHandlerResultHelper)
                                .GetMethod(nameof(MethodHandlerResultHelper.TaskToEmptyValueTask), staticFlags)!
                                .MakeGenericMethod(MethodInfo.ReturnType.GetGenericArguments()[0]), body);
                        }
                    }

                    var compiledBody = Expression.Lambda(body, contextArg).Compile();

                    this.methodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (Func<ServiceContext, ValueTask>)compiledBody);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
                }
                break;
            default:
                throw new InvalidOperationException("Unknown MethodType:" + MethodType + $"Path:{ToString()}");
        }
    }

    // non-filtered.
    public byte[] BoxedSerialize(object requestValue)
    {
        return MessagePackSerializer.Serialize(RequestType, requestValue, serializerOptions);
    }

    public object BoxedDeserialize(byte[] responseValue)
    {
        return MessagePackSerializer.Deserialize(UnwrappedResponseType, responseValue, serializerOptions);
    }

    static Type UnwrapResponseType(MethodInfo methodInfo, out MethodType methodType, out bool responseIsTask, out Type? requestTypeIfExists)
    {
        var t = methodInfo.ReturnType;
        if (!t.GetTypeInfo().IsGenericType) throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");

        // Task<Unary<T>>
        if (t.GetGenericTypeDefinition() == typeof(Task<>))
        {
            responseIsTask = true;
            t = t.GetGenericArguments()[0];
        }
        else
        {
            responseIsTask = false;
        }

        // Unary<T>
        var returnType = t.GetGenericTypeDefinition();
        if (returnType == typeof(UnaryResult<>))
        {
            methodType = MethodType.Unary;
            requestTypeIfExists = default;
            return t.GetGenericArguments()[0];
        }
        else if (returnType == typeof(ClientStreamingResult<,>))
        {
            methodType = MethodType.ClientStreaming;
            var genArgs = t.GetGenericArguments();
            requestTypeIfExists = genArgs[0];
            return genArgs[1];
        }
        else if (returnType == typeof(ServerStreamingResult<>))
        {
            methodType = MethodType.ServerStreaming;
            requestTypeIfExists = default;
            return t.GetGenericArguments()[0];
        }
        else if (returnType == typeof(DuplexStreamingResult<,>))
        {
            methodType = MethodType.DuplexStreaming;
            var genArgs = t.GetGenericArguments();
            requestTypeIfExists = genArgs[0];
            return genArgs[1];
        }
        else
        {
            throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");
        }
    }

    internal void BindHandler(ServiceBinderBase binder)
    {
        // NOTE: ServiceBinderBase.AddMethod has `class` generic constraint.
        //       We need to box an instance of the value type.
        var rawRequestType = RequestType.IsValueType ? typeof(Box<>).MakeGenericType(RequestType) : RequestType;
        var rawResponseType = UnwrappedResponseType.IsValueType ? typeof(Box<>).MakeGenericType(UnwrappedResponseType) : UnwrappedResponseType;

        typeof(MethodHandler)
            .GetMethod(nameof(BindHandlerTyped), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(RequestType, UnwrappedResponseType, rawRequestType, rawResponseType)
            .Invoke(this, new [] { binder });
    }

    void BindHandlerTyped<TRequest, TResponse, TRawRequest, TRawResponse>(ServiceBinderBase binder)
        where TRawRequest : class
        where TRawResponse : class
    {
        var handlerBinder = MagicOnionMethodHandlerBinder<TRequest, TResponse, TRawRequest, TRawResponse>.Instance;
        switch (this.MethodType)
        {
            case MethodType.Unary:
                if (this.MethodInfo.GetParameters().Any())
                {
                    handlerBinder.BindUnary(binder, UnaryServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                else
                {
                    handlerBinder.BindUnaryPalameterless(binder, UnaryServerMethod<Nil, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                break;
            case MethodType.ClientStreaming:
                handlerBinder.BindClientStreaming(binder, ClientStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                break;
            case MethodType.ServerStreaming:
                handlerBinder.BindServerStreaming(binder, ServerStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                break;
            case MethodType.DuplexStreaming:
                if (isStreamingHub)
                {
                    handlerBinder.BindStreamingHub(binder, DuplexStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                else
                {
                    handlerBinder.BindDuplexStreaming(binder, DuplexStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                break;
            default:
                throw new InvalidOperationException("Unknown RegisterType:" + this.MethodType);
        }
    }

    async Task<TResponse?> UnaryServerMethod<TRequest, TResponse>(TRequest request, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, serializerOptions, logger, this, context.GetHttpContext().RequestServices);
        serviceContext.SetRawRequest(request);

        TResponse? response = default;
        try
        {
            logger.BeginInvokeMethod(serviceContext, typeof(TRequest));
            if (enableCurrentContext)
            {
                ServiceContext.currentServiceContext.Value = serviceContext;
            }
            await this.methodBody(serviceContext).ConfigureAwait(false);
            if (serviceContext.Result is not null)
            {
                response = (TResponse?)serviceContext.Result;
            }
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            response = default;

            // WORKAROUND: Grpc.AspNetCore.Server throws a `Cancelled` status exception when it receives `null` response.
            //             To return the status code correctly, we needs to rethrow the exception here.
            //             https://github.com/grpc/grpc-dotnet/blob/d4ee8babcd90666fc0727163a06527ab9fd7366a/src/Grpc.AspNetCore.Server/Internal/CallHandlers/UnaryServerCallHandler.cs#L50-L56
            var rpcException = new RpcException(ex.ToStatus());
#if NET6_0_OR_GREATER
                if (ex.StackTrace is not null)
                {
                    ExceptionDispatchInfo.SetRemoteStackTrace(rpcException, ex.StackTrace);
                }
#endif
            throw rpcException;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (isReturnExceptionStackTraceInErrorDetail)
            {
                // Trim data.
                var msg = ex.ToString();
                var lineSplit = msg.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < lineSplit.Length; i++)
                {
                    if (!(lineSplit[i].Contains("System.Runtime.CompilerServices")
                          || lineSplit[i].Contains("直前に例外がスローされた場所からのスタック トレースの終わり")
                          || lineSplit[i].Contains("End of stack trace from the previous location where the exception was thrown")
                        ))
                    {
                        sb.AppendLine(lineSplit[i]);
                    }
                    if (sb.Length >= 5000)
                    {
                        sb.AppendLine("----Omit Message(message size is too long)----");
                        break;
                    }
                }
                var str = sb.ToString();

                context.Status = new Status(StatusCode.Unknown, str);
                logger.Error(ex, context);
                response = default;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            logger.EndInvokeMethod(serviceContext, typeof(TResponse), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }

        return response;
    }

    async Task<TResponse?> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new StreamingServiceContext<TRequest, Nil /* Dummy */>(
            ServiceType,
            MethodInfo,
            AttributeLookup,
            this.MethodType,
            context,
            serializerOptions,
            logger,
            this,
            context.GetHttpContext().RequestServices,
            requestStream,
            default
        );

        TResponse? response;
        try
        {
            using (requestStream as IDisposable)
            {
                logger.BeginInvokeMethod(serviceContext, typeof(Nil));
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }
                await this.methodBody(serviceContext).ConfigureAwait(false);
                response = serviceContext.Result is TResponse r ? r : default;
            }
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            response = default;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (isReturnExceptionStackTraceInErrorDetail)
            {
                context.Status = new Status(StatusCode.Unknown, ex.ToString());
                logger.Error(ex, context);
                response = default;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            logger.EndInvokeMethod(serviceContext, typeof(TResponse), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }

        return response;
    }

    async Task ServerStreamingServerMethod<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new StreamingServiceContext<Nil /* Dummy */, TResponse>(
            ServiceType,
            MethodInfo,
            AttributeLookup,
            this.MethodType,
            context,
            serializerOptions, 
            logger,
            this,
            context.GetHttpContext().RequestServices,
            default,
            responseStream
        );
        serviceContext.SetRawRequest(request);
        try
        {
            logger.BeginInvokeMethod(serviceContext, typeof(TRequest));
            if (enableCurrentContext)
            {
                ServiceContext.currentServiceContext.Value = serviceContext;
            }
            await this.methodBody(serviceContext).ConfigureAwait(false);
            return;
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            return;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (isReturnExceptionStackTraceInErrorDetail)
            {
                context.Status = new Status(StatusCode.Unknown, ex.ToString());
                logger.Error(ex, context);
                return;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            logger.EndInvokeMethod(serviceContext, typeof(Nil), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }
    }

    async Task DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new StreamingServiceContext<TRequest, TResponse>(
            ServiceType,
            MethodInfo,
            AttributeLookup,
            this.MethodType,
            context,
            serializerOptions,
            logger,
            this,
            context.GetHttpContext().RequestServices,
            requestStream,
            responseStream
        );
        try
        {
            logger.BeginInvokeMethod(serviceContext, typeof(Nil));
            using (requestStream as IDisposable)
            {
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }
                await this.methodBody(serviceContext).ConfigureAwait(false);

                return;
            }
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            return;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (isReturnExceptionStackTraceInErrorDetail)
            {
                context.Status = new Status(StatusCode.Unknown, ex.ToString());
                logger.Error(ex, context);
                return;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            logger.EndInvokeMethod(serviceContext, typeof(Nil), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }
    }

    public override string ToString()
    {
        return ServiceName + "/" + MethodName;
    }

    public override int GetHashCode()
    {
        return ServiceName.GetHashCode() ^ MethodInfo.Name.GetHashCode() << 2;
    }

    public bool Equals(MethodHandler? other)
    {
        return other != null && ServiceName.Equals(other.ServiceName) && MethodInfo.Name.Equals(other.MethodInfo.Name);
    }

    public class UniqueEqualityComparer : IEqualityComparer<MethodHandler>
    {
        public bool Equals(MethodHandler? x, MethodHandler? y)
        {
            return (x == null && y == null) || (x != null && y != null && x.methodHandlerId.Equals(y.methodHandlerId));
        }

        public int GetHashCode(MethodHandler obj)
        {
            return obj.methodHandlerId.GetHashCode();
        }
    }
}

/// <summary>
/// Options for MethodHandler construction.
/// </summary>
public class MethodHandlerOptions
{
    public IList<MagicOnionServiceFilterDescriptor> GlobalFilters { get; }

    public bool IsReturnExceptionStackTraceInErrorDetail { get; }

    public bool EnableCurrentContext { get; }

    public MessagePackSerializerOptions SerializerOptions { get; }

    public MethodHandlerOptions(MagicOnionOptions options)
    {
        GlobalFilters = options.GlobalFilters;
        IsReturnExceptionStackTraceInErrorDetail = options.IsReturnExceptionStackTraceInErrorDetail;
        EnableCurrentContext = options.EnableCurrentContext;
        SerializerOptions = options.SerializerOptions;
    }
}

internal class MethodHandlerResultHelper
{
    static readonly ValueTask CopmletedValueTask = new ValueTask();

    public static ValueTask NewEmptyValueTask<T>(T result)
    {
        // ignore result.
        return CopmletedValueTask;
    }

    public static async ValueTask TaskToEmptyValueTask<T>(Task<T> result)
    {
        // wait and ignore result.
        await result;
    }

    public static async ValueTask SetUnaryResult<T>(UnaryResult<T> result, ServiceContext context)
    {
        if (result.hasRawValue)
        {
            context.Result = (result.rawTaskValue != null) ? await result.rawTaskValue.ConfigureAwait(false) : result.rawValue;
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
    {
        if (result.hasRawValue)
        {
            context.Result = result.rawValue;
        }

        return default(ValueTask);
    }

    public static async ValueTask SerializeTaskClientStreamingResult<TRequest, TResponse>(Task<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
    {
        var result = await taskResult.ConfigureAwait(false);
        if (result.hasRawValue)
        {
            context.Result = result.rawValue;
        }
    }
}
