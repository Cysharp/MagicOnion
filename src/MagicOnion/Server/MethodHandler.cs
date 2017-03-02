using Grpc.Core;
using MessagePack;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public class MethodHandler : IEquatable<MethodHandler>
    {
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

        public readonly Type RequestType;
        public readonly Type UnwrappedResponseType;

        readonly IFormatterResolver resolver;
        readonly bool responseIsTask;

        readonly Func<ServiceContext, Task> methodBody;

        public Func<object, byte> serialize;

        // helper
        readonly Func<object, object, byte[]> boxedRequestSerialize;
        readonly Func<object, byte[], object> boxedResponseDeserialize;

        public MethodHandler(MagicOnionOptions options, Type classType, MethodInfo methodInfo)
        {
            this.ServiceType = classType;
            this.ServiceName = classType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>)).GetGenericArguments()[0].Name;
            this.MethodInfo = methodInfo;
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
                    //      var request = context.Resolver.GetFormatterWithVerify<TRequest>().Deserialize(context.Request);
                    //      return new FooService() { Context = context }.Bar(request.Item1, request.Item2);
                    // };
                    {
                        // TODO:change expression

                        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        var marshallerType = typeof(Marshaller<>).MakeGenericType(RequestType);

                        var requestArg = Expression.Parameter(RequestType, "request");

                        var requestMarshalleExpr = Expression.Convert(Expression.Property(contextArg, typeof(ServiceContext).GetProperty("RequestMarshaller", flags)), marshallerType);
                        var deserializer = Expression.Property(requestMarshalleExpr, "Deserializer");
                        var callDeserialize = Expression.Call(deserializer, typeof(Func<,>).MakeGenericType(typeof(byte[]), RequestType).GetMethod("Invoke"), Expression.Property(contextArg, typeof(ServiceContext).GetProperty("Request", flags)));

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
                    if (parameters.Length != 0)
                    {
                        throw new InvalidOperationException($"{MethodType} does not support method parameters. If you need to send initial parameter, use header instead.");
                    }

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

            // Utility
            {
                // TODO:Unknown Utilities

                // (object requestMarshaller, object value) => ((Marshaller<TRequest>)requestMarshaller).Serializer.Invoke((TRequest)value);
                var marshallerType = typeof(Marshaller<>).MakeGenericType(RequestType);
                var requestMarshallerArg = Expression.Parameter(typeof(object), "requestMarshaller");
                var valueArg = Expression.Parameter(typeof(object), "value");
                var serializer = Expression.Property(Expression.Convert(requestMarshallerArg, marshallerType), "Serializer");
                var callSerialize = Expression.Call(serializer, typeof(Func<,>).MakeGenericType(RequestType, typeof(byte[])).GetMethod("Invoke"),
                    Expression.Convert(valueArg, RequestType));

                boxedRequestSerialize = Expression.Lambda<Func<object, object, byte[]>>(callSerialize, requestMarshallerArg, valueArg).Compile();
            }
            {
                // (object responseMarshaller, byte[] value) => ((Marshaller<TResponse>)requestMarshaller).Deserializer.Invoke(value);
                var marshallerType = typeof(Marshaller<>).MakeGenericType(UnwrappedResponseType);
                var responseMarshallerArg = Expression.Parameter(typeof(object), "responseMarshaller");
                var valueArg = Expression.Parameter(typeof(byte[]), "value");
                var deserializer = Expression.Property(Expression.Convert(responseMarshallerArg, marshallerType), "Deserializer");
                var callDeserialize = Expression.Convert(Expression.Call(deserializer, typeof(Func<,>).MakeGenericType(typeof(byte[]), UnwrappedResponseType).GetMethod("Invoke"), valueArg), typeof(object));
                boxedResponseDeserialize = Expression.Lambda<Func<object, byte[], object>>(callDeserialize, responseMarshallerArg, valueArg).Compile();
            }
        }

        public byte[] BoxedSerialize(object requestValue)
        {
            return boxedRequestSerialize(this.requestMarshaller, requestValue);
        }

        public object BoxedDeserialize(byte[] responseValue)
        {
            return boxedResponseDeserialize(this.responseMarshaller, responseValue);
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
            var method = new Method<byte[], byte[]>(this.MethodType, this.ServiceName, this.MethodInfo.Name, MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

            switch (this.MethodType)
            {
                case MethodType.Unary:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(UnaryServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);
                        var handler = (UnaryServerMethod<byte[], byte[]>)Delegate.CreateDelegate(typeof(UnaryServerMethod<byte[], byte[]>), this, genericMethod);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.ClientStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(ClientStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);
                        var handler = (ClientStreamingServerMethod<byte[], byte[]>)Delegate.CreateDelegate(typeof(ClientStreamingServerMethod<byte[], byte[]>), this, genericMethod);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.ServerStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(ServerStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);
                        var handler = (ServerStreamingServerMethod<byte[], byte[]>)Delegate.CreateDelegate(typeof(ServerStreamingServerMethod<byte[], byte[]>), this, genericMethod);
                        builder.AddMethod(method, handler);
                    }
                    break;
                case MethodType.DuplexStreaming:
                    {
                        var genericMethod = this.GetType()
                            .GetMethod(nameof(DuplexStreamingServerMethod), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(RequestType, UnwrappedResponseType);
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
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger)
            {
                Request = request
            };
            try
            {
                logger.BeginInvokeMethod(serviceContext);
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
                logger.EndInvokeMethod(serviceContext, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        async Task<byte[]> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<byte[]> requestStream, ServerCallContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isErrorOrInterrupted = false;
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger)
            {
                RequestStream = requestStream
            };
            try
            {
                using (requestStream)
                {
                    logger.BeginInvokeMethod(serviceContext);
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
                logger.EndInvokeMethod(serviceContext, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        async Task<byte[]> ServerStreamingServerMethod<TRequest, TResponse>(byte[] request, IServerStreamWriter<byte[]> responseStream, ServerCallContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isErrorOrInterrupted = false;
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger)
            {
                ResponseStream = responseStream,
                Request = request
            };
            try
            {
                logger.BeginInvokeMethod(serviceContext);
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
                logger.EndInvokeMethod(serviceContext, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
            }
        }

        async Task<byte[]> DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<byte[]> requestStream, IServerStreamWriter<byte[]> responseStream, ServerCallContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isErrorOrInterrupted = false;
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, resolver, logger)
            {
                RequestStream = requestStream,
                ResponseStream = responseStream
            };
            try
            {
                logger.BeginInvokeMethod(serviceContext);
                using (requestStream)
                {

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
                logger.EndInvokeMethod(serviceContext, sw.Elapsed.TotalMilliseconds, isErrorOrInterrupted);
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
    }
}