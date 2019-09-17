using Grpc.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public class MethodHandler : IEquatable<MethodHandler>
    {
        static int methodHandlerIdBuild = 0;
        static readonly byte[] emptyBytes = new byte[0];

        int methodHandlerId = 0;

        public string ServiceName { get; private set; }
        public string MethodName { get; private set; }
        public Type ServiceType { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public MethodType MethodType { get; private set; }

        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        readonly IMagicOnionFilterFactory<MagicOnionFilterAttribute>[] filters;

        // options

        internal readonly bool isReturnExceptionStackTraceInErrorDetail;
        internal readonly IMagicOnionLogger logger;
        readonly bool enableCurrentContext;
        readonly IServiceLocator serviceLocator;

        // use for request handling.

        public readonly Type RequestType;
        public readonly Type UnwrappedResponseType;

        readonly IFormatterResolver resolver;
        readonly bool responseIsTask;

        readonly Func<ServiceContext, ValueTask> methodBody;
        public Func<object, byte> serialize;

        // reflection cache
        static readonly MethodInfo messagePackDeserialize = typeof(LZ4MessagePackSerializer).GetMethods()
            .First(x => x.Name == "Deserialize" && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(byte[]));
        static readonly MethodInfo register = typeof(IServiceLocator).GetMethods()
            .First(x => x.Name == nameof(IServiceLocator.Register) && x.GetParameters().Length == 0);
        static readonly MethodInfo createService = typeof(ServiceLocatorHelper).GetMethod(nameof(ServiceLocatorHelper.CreateService), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        public MethodHandler(MagicOnionOptions options, Type classType, MethodInfo methodInfo, string methodName)
        {
            this.methodHandlerId = Interlocked.Increment(ref methodHandlerIdBuild);

            var serviceInterfaceType = classType.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>)).GetGenericArguments()[0];

            this.ServiceType = classType;
            this.ServiceName = serviceInterfaceType.Name;
            this.MethodInfo = methodInfo;
            this.MethodName = methodName;
            MethodType mt;
            this.UnwrappedResponseType = UnwrapResponseType(methodInfo, out mt, out responseIsTask, out this.RequestType);
            this.MethodType = mt;
            this.resolver = options.FormatterResolver;

            var parameters = methodInfo.GetParameters();
            if (RequestType == null)
            {
                this.RequestType = MagicOnionMarshallers.CreateRequestTypeAndSetResolver(classType.Name + "/" + methodInfo.Name, parameters, ref resolver);
            }

            this.AttributeLookup = classType.GetCustomAttributes(true)
                .Concat(methodInfo.GetCustomAttributes(true))
                .Cast<Attribute>()
                .ToLookup(x => x.GetType());

            this.filters = options.GlobalFilters
                .OfType<IMagicOnionFilterFactory<MagicOnionFilterAttribute>>()
                .Concat(classType.GetCustomAttributes<MagicOnionFilterAttribute>(true).Select(x => new MagicOnionServiceFilterDescriptor(x, x.Order)))
                .Concat(classType.GetCustomAttributes(true).OfType<IMagicOnionFilterFactory<MagicOnionFilterAttribute>>())
                .Concat(methodInfo.GetCustomAttributes<MagicOnionFilterAttribute>(true).Select(x => new MagicOnionServiceFilterDescriptor(x, x.Order)))
                .Concat(methodInfo.GetCustomAttributes(true).OfType<IMagicOnionFilterFactory<MagicOnionFilterAttribute>>())
                .OrderBy(x => x.Order)
                .ToArray();

            // options
            this.isReturnExceptionStackTraceInErrorDetail = options.IsReturnExceptionStackTraceInErrorDetail;
            this.logger = options.MagicOnionLogger;
            this.enableCurrentContext = options.EnableCurrentContext;
            this.serviceLocator = options.ServiceLocator;

            // register DI
            register.MakeGenericMethod(classType).Invoke(this.serviceLocator, null);

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
                    //      var request = LZ4MessagePackSerializer.Deserialize<T>(context.Request, context.Resolver);
                    //      var result = new FooService() { Context = context }.Bar(request.Item1, request.Item2);
                    //      return MethodHandlerResultHelper.SerializeUnaryResult(result, context);
                    // };
                    try
                    {
                        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        var staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

                        var requestArg = Expression.Parameter(RequestType, "request");
                        var getResolver = Expression.Property(contextArg, typeof(ServiceContext).GetProperty("FormatterResolver", flags));

                        var contextRequest = Expression.Property(contextArg, typeof(ServiceContext).GetProperty("Request", flags));

                        var callDeserialize = Expression.Call(messagePackDeserialize.MakeGenericMethod(RequestType), contextRequest, getResolver);
                        var assignRequest = Expression.Assign(requestArg, callDeserialize);

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

                        if (MethodType == MethodType.Unary)
                        {
                            var finalMethod = (responseIsTask)
                                ? typeof(MethodHandlerResultHelper).GetMethod("SerializeTaskUnaryResult", staticFlags).MakeGenericMethod(UnwrappedResponseType)
                                : typeof(MethodHandlerResultHelper).GetMethod("SerializeUnaryResult", staticFlags).MakeGenericMethod(UnwrappedResponseType);
                            callBody = Expression.Call(finalMethod, callBody, contextArg);
                        }
                        else
                        {
                            if (!responseIsTask)
                            {
                                callBody = Expression.Call(typeof(MethodHandlerResultHelper)
                                    .GetMethod(nameof(MethodHandlerResultHelper.NewEmptyValueTask), staticFlags)
                                    .MakeGenericMethod(MethodInfo.ReturnType), callBody);
                            }
                            else
                            {
                                callBody = Expression.Call(typeof(MethodHandlerResultHelper)
                                    .GetMethod(nameof(MethodHandlerResultHelper.TaskToEmptyValueTask), staticFlags)
                                    .MakeGenericMethod(MethodInfo.ReturnType.GetGenericArguments()[0]), callBody);
                            }
                        }

                        var body = Expression.Block(new[] { requestArg }, assignRequest, callBody);
                        var compiledBody = Expression.Lambda(body, contextArg).Compile();

                        this.methodBody = BuildMethodBodyWithFilter((Func<ServiceContext, ValueTask>)compiledBody);
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
                                ? typeof(MethodHandlerResultHelper).GetMethod("SerializeTaskClientStreamingResult", staticFlags).MakeGenericMethod(RequestType, UnwrappedResponseType)
                                : typeof(MethodHandlerResultHelper).GetMethod("SerializeClientStreamingResult", staticFlags).MakeGenericMethod(RequestType, UnwrappedResponseType);
                            body = Expression.Call(finalMethod, body, contextArg);
                        }
                        else
                        {
                            if (!responseIsTask)
                            {
                                body = Expression.Call(typeof(MethodHandlerResultHelper)
                                    .GetMethod(nameof(MethodHandlerResultHelper.NewEmptyValueTask), staticFlags)
                                    .MakeGenericMethod(MethodInfo.ReturnType), body);
                            }
                            else
                            {
                                body = Expression.Call(typeof(MethodHandlerResultHelper)
                                    .GetMethod(nameof(MethodHandlerResultHelper.TaskToEmptyValueTask), staticFlags)
                                    .MakeGenericMethod(MethodInfo.ReturnType.GetGenericArguments()[0]), body);
                            }
                        }

                        var compiledBody = Expression.Lambda(body, contextArg).Compile();

                        this.methodBody = BuildMethodBodyWithFilter((Func<ServiceContext, ValueTask>)compiledBody);
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
            return LZ4MessagePackSerializer.NonGeneric.Serialize(RequestType, requestValue, resolver);
        }

        public object BoxedDeserialize(byte[] responseValue)
        {
            return LZ4MessagePackSerializer.NonGeneric.Deserialize(UnwrappedResponseType, responseValue, resolver);
        }

        static Type UnwrapResponseType(MethodInfo methodInfo, out MethodType methodType, out bool responseIsTask, out Type requestTypeIfExists)
        {
            var t = methodInfo.ReturnType;
            if (!t.GetTypeInfo().IsGenericType) throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");

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
                requestTypeIfExists = null;
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
                requestTypeIfExists = null;
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
                throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");
            }
        }

        Func<ServiceContext, ValueTask> BuildMethodBodyWithFilter(Func<ServiceContext, ValueTask> methodBody)
        {
            Func<ServiceContext, ValueTask> next = methodBody;

            foreach (var filterFactory in this.filters.Reverse())
            {
                var newFilter = filterFactory.CreateInstance(serviceLocator);
                var next_ = next; // capture reference
                next = (ctx) => newFilter.Invoke(ctx, next_);
            }

            return next;
        }

        internal void RegisterHandler(ServerServiceDefinition.Builder builder)
        {
            var method = new Method<byte[], byte[]>(this.MethodType, this.ServiceName, this.MethodName, MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

            switch (this.MethodType)
            {
                case MethodType.Unary:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(UnaryServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);

                        var handler = (UnaryServerMethod<byte[], byte[]>)genericMethod.CreateDelegate(typeof(UnaryServerMethod<byte[], byte[]>), this);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.ClientStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(ClientStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);
                        var handler = (ClientStreamingServerMethod<byte[], byte[]>)genericMethod.CreateDelegate(typeof(ClientStreamingServerMethod<byte[], byte[]>), this);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.ServerStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(ServerStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);
                        var handler = (ServerStreamingServerMethod<byte[], byte[]>)genericMethod.CreateDelegate(typeof(ServerStreamingServerMethod<byte[], byte[]>), this);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.DuplexStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(DuplexStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);
                        var handler = (DuplexStreamingServerMethod<byte[], byte[]>)genericMethod.CreateDelegate(typeof(DuplexStreamingServerMethod<byte[], byte[]>), this);
                        builder.AddMethod(method, handler);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unknown RegisterType:" + this.MethodType);
            }
        }

        async Task<byte[]> UnaryServerMethod<TRequest, TResponse>(byte[] request, ServerCallContext context)
        {
            var isErrorOrInterrupted = false;
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger, this, serviceLocator)
            {
                Request = request
            };

            byte[] response = emptyBytes;
            try
            {
                logger.BeginInvokeMethod(serviceContext, request, typeof(TRequest));
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }
                await this.methodBody(serviceContext).ConfigureAwait(false);
                response = serviceContext.Result ?? emptyBytes;
            }
            catch (ReturnStatusException ex)
            {
                isErrorOrInterrupted = true;
                context.Status = ex.ToStatus();
                response = emptyBytes;
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
                    LogError(ex, context);
                    response = emptyBytes;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                logger.EndInvokeMethod(serviceContext, response, typeof(TResponse), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
            }

            return response;
        }

        async Task<byte[]> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<byte[]> requestStream, ServerCallContext context)
        {
            var isErrorOrInterrupted = false;
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger, this, serviceLocator)
            {
                RequestStream = requestStream
            };
            byte[] response = emptyBytes;
            try
            {
                using (requestStream as IDisposable)
                {
                    logger.BeginInvokeMethod(serviceContext, emptyBytes, typeof(Nil));
                    if (enableCurrentContext)
                    {
                        ServiceContext.currentServiceContext.Value = serviceContext;
                    }
                    await this.methodBody(serviceContext).ConfigureAwait(false);
                    response = serviceContext.Result ?? emptyBytes;
                }
            }
            catch (ReturnStatusException ex)
            {
                isErrorOrInterrupted = true;
                context.Status = ex.ToStatus();
                response = emptyBytes;
            }
            catch (Exception ex)
            {
                isErrorOrInterrupted = true;
                if (isReturnExceptionStackTraceInErrorDetail)
                {
                    context.Status = new Status(StatusCode.Unknown, ex.ToString());
                    LogError(ex, context);
                    response = emptyBytes;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                logger.EndInvokeMethod(serviceContext, response, typeof(TResponse), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
            }
            return response;
        }

        async Task<byte[]> ServerStreamingServerMethod<TRequest, TResponse>(byte[] request, IServerStreamWriter<byte[]> responseStream, ServerCallContext context)
        {
            var isErrorOrInterrupted = false;
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger, this, serviceLocator)
            {
                ResponseStream = responseStream,
                Request = request
            };
            try
            {
                logger.BeginInvokeMethod(serviceContext, request, typeof(TRequest));
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }
                await this.methodBody(serviceContext).ConfigureAwait(false);
                return emptyBytes;
            }
            catch (ReturnStatusException ex)
            {
                isErrorOrInterrupted = true;
                context.Status = ex.ToStatus();
                return emptyBytes;
            }
            catch (Exception ex)
            {
                isErrorOrInterrupted = true;
                if (isReturnExceptionStackTraceInErrorDetail)
                {
                    context.Status = new Status(StatusCode.Unknown, ex.ToString());
                    LogError(ex, context);
                    return emptyBytes;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                logger.EndInvokeMethod(serviceContext, emptyBytes, typeof(Nil), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        async Task<byte[]> DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<byte[]> requestStream, IServerStreamWriter<byte[]> responseStream, ServerCallContext context)
        {
            var isErrorOrInterrupted = false;
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger, this, serviceLocator)
            {
                RequestStream = requestStream,
                ResponseStream = responseStream
            };
            try
            {
                logger.BeginInvokeMethod(serviceContext, emptyBytes, typeof(Nil));
                using (requestStream as IDisposable)
                {
                    if (enableCurrentContext)
                    {
                        ServiceContext.currentServiceContext.Value = serviceContext;
                    }
                    await this.methodBody(serviceContext).ConfigureAwait(false);

                    return emptyBytes;
                }
            }
            catch (ReturnStatusException ex)
            {
                isErrorOrInterrupted = true;
                context.Status = ex.ToStatus();
                return emptyBytes;
            }
            catch (Exception ex)
            {
                isErrorOrInterrupted = true;
                if (isReturnExceptionStackTraceInErrorDetail)
                {
                    context.Status = new Status(StatusCode.Unknown, ex.ToString());
                    LogError(ex, context);
                    return emptyBytes;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                logger.EndInvokeMethod(serviceContext, emptyBytes, typeof(Nil), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        static void LogError(Exception ex, ServerCallContext context)
        {
            GrpcEnvironment.Logger.Error(ex, "MagicOnionHandler throws exception occured in " + context.Method);
        }

        public override string ToString()
        {
            return ServiceName + "/" + MethodInfo.Name;
        }

        public override int GetHashCode()
        {
            return ServiceName.GetHashCode() ^ MethodInfo.Name.GetHashCode() << 2;
        }

        public bool Equals(MethodHandler other)
        {
            return ServiceName.Equals(other.ServiceName) && MethodInfo.Name.Equals(other.MethodInfo.Name);
        }

        public class UniqueEqualityComparer : IEqualityComparer<MethodHandler>
        {
            public bool Equals(MethodHandler x, MethodHandler y)
            {
                return x.methodHandlerId.Equals(y.methodHandlerId);
            }

            public int GetHashCode(MethodHandler obj)
            {
                return obj.methodHandlerId.GetHashCode();
            }
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

        public static async ValueTask SerializeUnaryResult<T>(UnaryResult<T> result, ServiceContext context)
        {
            if (result.hasRawValue && !context.IsIgnoreSerialization)
            {
                var value = (result.rawTaskValue != null) ? await result.rawTaskValue.ConfigureAwait(false) : result.rawValue;

                var bytes = LZ4MessagePackSerializer.Serialize<T>(value, context.FormatterResolver);
                context.Result = bytes;
            }
        }

        public static async ValueTask SerializeTaskUnaryResult<T>(Task<UnaryResult<T>> taskResult, ServiceContext context)
        {
            var result = await taskResult.ConfigureAwait(false);
            if (result.hasRawValue && !context.IsIgnoreSerialization)
            {
                var value = (result.rawTaskValue != null) ? await result.rawTaskValue.ConfigureAwait(false) : result.rawValue;

                var bytes = LZ4MessagePackSerializer.Serialize<T>(value, context.FormatterResolver);
                context.Result = bytes;
            }
        }

        public static ValueTask SerializeClientStreamingResult<TRequest, TResponse>(ClientStreamingResult<TRequest, TResponse> result, ServiceContext context)
        {
            if (result.hasRawValue)
            {
                var bytes = LZ4MessagePackSerializer.Serialize<TResponse>(result.rawValue, context.FormatterResolver);
                context.Result = bytes;
            }

            return default(ValueTask);
        }

        public static async ValueTask SerializeTaskClientStreamingResult<TRequest, TResponse>(Task<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
        {
            var result = await taskResult.ConfigureAwait(false);
            if (result.hasRawValue)
            {
                var bytes = LZ4MessagePackSerializer.Serialize<TResponse>(result.rawValue, context.FormatterResolver);
                context.Result = bytes;
            }
        }
    }
}