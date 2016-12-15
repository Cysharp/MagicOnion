using Grpc.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    internal class MethodHandler
    {
        static readonly Type[] dynamicArgumentTupleTypes = typeof(DynamicArgumentTuple<,>).Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTuple") && !x.Name.Contains("Formatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        static readonly Type[] dynamicArgumentTupleFormatterTypes = typeof(DynamicArgumentTupleFormatter<,,>).Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTupleFormatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        static readonly byte[] emptyBytes = new byte[0];

        public string ServiceName { get; private set; }
        public Type ServiceType { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public MethodType MethodType { get; private set; }

        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        readonly MagicOnionFilterAttribute[] filters;

        // options

        readonly bool isReturnExceptionStackTraceInErrorDetail;
        readonly IMagicOnionLogger logger;

        // use for request handling.

        readonly Type requestType;
        readonly Type unwrapResponseType;

        readonly object requestMarshaller;
        readonly object responseMarshaller;
        readonly bool responseIsTask;

        readonly Func<ServiceContext, Task> methodBody;

        public MethodHandler(MagicOnionOptions options, Type classType, MethodInfo methodInfo)
        {
            this.ServiceType = classType;
            this.ServiceName = classType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>)).GetGenericArguments()[0].Name;
            this.MethodInfo = methodInfo;
            MethodType mt;
            this.unwrapResponseType = UnwrapResponseType(methodInfo, out mt, out responseIsTask, out this.requestType);
            this.MethodType = mt;

            var parameters = methodInfo.GetParameters();
            if (requestType == null)
            {
                this.requestType = MagicOnionMarshallers.CreateRequestTypeAndMarshaller(options.ZeroFormatterTypeResolverType, classType.Name + "/" + methodInfo.Name, parameters, out requestMarshaller);
            }
            else
            {
                this.requestMarshaller = MagicOnionMarshallers.CreateZeroFormattertMarshallerReflection(options.ZeroFormatterTypeResolverType, requestType);
            }

            this.responseMarshaller = MagicOnionMarshallers.CreateZeroFormattertMarshallerReflection(options.ZeroFormatterTypeResolverType, unwrapResponseType);

            this.AttributeLookup = classType.GetCustomAttributes(true)
                .Concat(methodInfo.GetCustomAttributes(true))
                .Cast<Attribute>()
                .ToLookup(x => x.GetType());

            this.filters = options.GlobalFilters
                .Concat(classType.GetCustomAttributes<MagicOnionFilterAttribute>(true))
                .Concat(methodInfo.GetCustomAttributes<MagicOnionFilterAttribute>(true))
                .OrderBy(x => x.Order)
                .ToArray();

            // options
            this.isReturnExceptionStackTraceInErrorDetail = options.IsReturnExceptionStackTraceInErrorDetail;
            this.logger = options.MagicOnionLogger;

            // prepare lambda parameters
            var contextArg = Expression.Parameter(typeof(ServiceContext), "context");
            var contextBind = Expression.Bind(classType.GetProperty("Context"), contextArg);
            var instance = Expression.MemberInit(Expression.New(classType), contextBind);

            switch (MethodType)
            {
                case MethodType.Unary:
                case MethodType.ServerStreaming:
                    // (ServiceContext context) =>
                    // {
                    //      var request = ((Marshaller<TRequest>)context.RequestMarshaller).Deserializer.Invoke(context.Request);
                    //      return new FooService() { Context = context }.Bar(request.Item1, request.Item2);
                    // };
                    {
                        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        var marshallerType = typeof(Marshaller<>).MakeGenericType(requestType);

                        var requestArg = Expression.Parameter(requestType, "request");

                        var requestMarshalleExpr = Expression.Convert(Expression.Property(contextArg, typeof(ServiceContext).GetProperty("RequestMarshaller", flags)), marshallerType);
                        var deserializer = Expression.Property(requestMarshalleExpr, "Deserializer");
                        var callDeserialize = Expression.Call(deserializer, typeof(Func<,>).MakeGenericType(typeof(byte[]), requestType).GetMethod("Invoke"), Expression.Property(contextArg, typeof(ServiceContext).GetProperty("Request", flags)));

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
                        if (!responseIsTask)
                        {
                            callBody = Expression.Call(typeof(Task).GetMethod("FromResult").MakeGenericMethod(MethodInfo.ReturnType), callBody);
                        }

                        var body = Expression.Block(new[] { requestArg }, assignRequest, callBody);
                        var compiledBody = Expression.Lambda(body, contextArg).Compile();

                        this.methodBody = BuildMethodBodyWithFilter((Func<ServiceContext, Task>)compiledBody);
                    }
                    break;
                case MethodType.ClientStreaming:
                case MethodType.DuplexStreaming:
                    // (ServiceContext context) => new FooService() { Context = context }.Bar();
                    {
                        var body = Expression.Call(instance, methodInfo);
                        if (!responseIsTask)
                        {
                            body = Expression.Call(typeof(Task).GetMethod("FromResult").MakeGenericMethod(MethodInfo.ReturnType), body);
                        }

                        var compiledBody = Expression.Lambda(body, contextArg).Compile();

                        this.methodBody = BuildMethodBodyWithFilter((Func<ServiceContext, Task>)compiledBody);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unknown MethodType:" + MethodType);
            }
        }

        static Type UnwrapResponseType(MethodInfo methodInfo, out MethodType methodType, out bool responseIsTask, out Type requestTypeIfExists)
        {
            var t = methodInfo.ReturnType;
            if (!t.IsGenericType) throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");

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

        Func<ServiceContext, Task> BuildMethodBodyWithFilter(Func<ServiceContext, Task> methodBody)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Func<ServiceContext, Task> next = methodBody;

            foreach (var filter in this.filters.Reverse())
            {
                var fields = filter.GetType().GetFields(flags);

                var newFilter = (MagicOnionFilterAttribute)Activator.CreateInstance(filter.GetType(), new object[] { next });
                // copy all data.
                foreach (var item in fields)
                {
                    item.SetValue(newFilter, item.GetValue(filter));
                }

                next = newFilter.Invoke;
            }

            return next;
        }

        internal void RegisterHandler(ServerServiceDefinition.Builder builder)
        {
            var method = new Method<byte[], byte[]>(this.MethodType, this.ServiceName, this.MethodInfo.Name, MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);

            switch (this.MethodType)
            {
                case MethodType.Unary:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(UnaryServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(requestType, unwrapResponseType);
                        var handler = (UnaryServerMethod<byte[], byte[]>)Delegate.CreateDelegate(typeof(UnaryServerMethod<byte[], byte[]>), this, genericMethod);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.ClientStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(ClientStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(requestType, unwrapResponseType);
                        var handler = (ClientStreamingServerMethod<byte[], byte[]>)Delegate.CreateDelegate(typeof(ClientStreamingServerMethod<byte[], byte[]>), this, genericMethod);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.ServerStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(ServerStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(requestType, unwrapResponseType);
                        var handler = (ServerStreamingServerMethod<byte[], byte[]>)Delegate.CreateDelegate(typeof(ServerStreamingServerMethod<byte[], byte[]>), this, genericMethod);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.DuplexStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(DuplexStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(requestType, unwrapResponseType);
                        var handler = (DuplexStreamingServerMethod<byte[], byte[]>)Delegate.CreateDelegate(typeof(DuplexStreamingServerMethod<byte[], byte[]>), this, genericMethod);
                        builder.AddMethod(method, handler);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unknown RegisterType:" + this.MethodType);
            }
        }

        async Task<byte[]> UnaryServerMethod<TRequest, TResponse>(byte[] request, ServerCallContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isErrorOrInterrupted = false;
            try
            {
                logger.BeginInvokeMethod(this.MethodType, context.Method);

                var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context)
                {
                    RequestMarshaller = requestMarshaller,
                    ResponseMarshaller = responseMarshaller,
                    Request = request
                };

                await this.methodBody(serviceContext).ConfigureAwait(false);

                return serviceContext.Result ?? emptyBytes;
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
                sw.Stop();
                logger.EndInvokeMethod(this.MethodType, context.Method, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        async Task<byte[]> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<byte[]> requestStream, ServerCallContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isErrorOrInterrupted = false;
            try
            {
                logger.BeginInvokeMethod(this.MethodType, context.Method);
                using (requestStream)
                {
                    var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context)
                    {
                        RequestMarshaller = requestMarshaller,
                        ResponseMarshaller = responseMarshaller,
                        RequestStream = requestStream
                    };

                    await this.methodBody(serviceContext).ConfigureAwait(false);

                    return serviceContext.Result ?? emptyBytes;
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
                sw.Stop();
                logger.EndInvokeMethod(this.MethodType, context.Method, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        async Task<byte[]> ServerStreamingServerMethod<TRequest, TResponse>(byte[] request, IServerStreamWriter<byte[]> responseStream, ServerCallContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isErrorOrInterrupted = false;
            try
            {
                logger.BeginInvokeMethod(this.MethodType, context.Method);
                var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context)
                {
                    RequestMarshaller = requestMarshaller,
                    ResponseMarshaller = responseMarshaller,
                    ResponseStream = responseStream,
                    Request = request
                };

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
                sw.Stop();
                logger.EndInvokeMethod(this.MethodType, context.Method, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        async Task<byte[]> DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<byte[]> requestStream, IServerStreamWriter<byte[]> responseStream, ServerCallContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isErrorOrInterrupted = false;
            try
            {
                logger.BeginInvokeMethod(this.MethodType, context.Method);
                using (requestStream)
                {
                    var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context)
                    {
                        RequestMarshaller = requestMarshaller,
                        ResponseMarshaller = responseMarshaller,
                        RequestStream = requestStream,
                        ResponseStream = responseStream
                    };

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
                sw.Stop();
                logger.EndInvokeMethod(this.MethodType, context.Method, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        static void LogError(Exception ex, ServerCallContext context)
        {
            GrpcEnvironment.Logger.Error(ex, "MagicOnionHandler throws exception occured in " + context.Method);
        }
    }
}