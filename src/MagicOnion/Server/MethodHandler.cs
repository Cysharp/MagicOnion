
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Linq.Expressions;
using Grpc.Core;
using System.Threading;
using System.Reflection.Emit;
using ZeroFormatter.Formatters;
using ZeroFormatter.Internal;
using ZeroFormatter;

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

        public string ServiceName { get; private set; }
        public Type ServiceType { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public MethodType MethodType { get; private set; }

        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        // TODO:filter
        // readonly LightNodeFilterAttribute[] filters;

        readonly Type requestType;
        readonly Type unwrapResponseType;

        readonly object requestMarshaller;
        readonly object responseMarshaller;
        readonly bool responseIsTask;

        readonly Delegate methodBody;

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

            // TODO:filters
            //this.filters = options.Filters
            //    .Concat(classType.GetCustomAttributes<LightNodeFilterAttribute>(true))
            //    .Concat(methodInfo.GetCustomAttributes<LightNodeFilterAttribute>(true))
            //    .OrderBy(x => x.Order)
            //    .ToArray();

            // prepare lambda parameters
            var contextArg = Expression.Parameter(typeof(ServiceContext), "context");
            var contextBind = Expression.Bind(classType.GetProperty("Context"), contextArg);
            var instance = Expression.MemberInit(Expression.New(classType), contextBind);

            switch (MethodType)
            {
                case MethodType.Unary:
                case MethodType.ServerStreaming:
                    // (ServiceContext context, TRequest request) => new FooService() { Context = context }.Bar(request.Item1, request.Item2);
                    {
                        var requestArg = Expression.Parameter(requestType, "request");

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

                        var body = Expression.Call(instance, methodInfo, arguments);
                        this.methodBody = Expression.Lambda(body, contextArg, requestArg).Compile();
                    }
                    break;
                case MethodType.ClientStreaming:
                    // (ServiceContext context) => new FooService() { Context = context }.Bar();
                    {
                        var body = Expression.Call(instance, methodInfo);
                        this.methodBody = Expression.Lambda(body, contextArg).Compile();
                    }
                    break;
                case MethodType.DuplexStreaming:
                    break;
                default:
                    break;
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
            else
            {
                //methodType = MethodType.Unary; // TODO:others...
                throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");
            }
        }

        // TODO:filter
        // return InvokeRecursive(-1, targetFilters, options, context, coordinator);

        //Task InvokeRecursive(int index, IReadOnlyList<LightNodeFilterAttribute> filters, ILightNodeOptions options, OperationContext context, IOperationCoordinator coordinator)
        //{
        //    index += 1;
        //    if (filters.Count != index)
        //    {
        //        // chain next filter
        //        return filters[index].Invoke(context, () => InvokeRecursive(index, filters, options, context, coordinator));
        //    }
        //    else
        //    {
        //        // execute operation
        //        return coordinator.ExecuteOperation(options, context, ExecuteOperation);
        //    }
        //}

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
                //case MethodType.DuplexStreaming:
                //    builder.AddMethod(method, DuplexStreamingServerMethod);
                //    break;
                default:
                    throw new InvalidOperationException();
            }
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        async Task<byte[]> UnaryServerMethod<TRequest, TResponse>(byte[] request, ServerCallContext context)
        {
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, MethodType.Unary, context)
            {
                RequestMarshaller = requestMarshaller,
                ResponseMarshaller = responseMarshaller
            };

            var deserializer = (Marshaller<TRequest>)requestMarshaller;
            var args = deserializer.Deserializer(request);

            if (responseIsTask)
            {
                var body = (Func<ServiceContext, TRequest, Task<UnaryResult<TResponse>>>)this.methodBody;
                await body(serviceContext, args).ConfigureAwait(false);
            }
            else
            {
                var body = (Func<ServiceContext, TRequest, UnaryResult<TResponse>>)this.methodBody;
                body(serviceContext, args);
            }

            return serviceContext.Result;
        }

        async Task<byte[]> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<byte[]> requestStream, ServerCallContext context)
        {
            using (requestStream)
            {
                var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, MethodType.ClientStreaming, context)
                {
                    RequestMarshaller = requestMarshaller,
                    ResponseMarshaller = responseMarshaller,
                    RequestStream = requestStream
                };

                if (responseIsTask)
                {
                    var body = (Func<ServiceContext, Task<ClientStreamingResult<TRequest, TResponse>>>)this.methodBody;
                    await body(serviceContext).ConfigureAwait(false);
                }
                else
                {
                    var body = (Func<ServiceContext, ClientStreamingResult<TRequest, TResponse>>)this.methodBody;
                    body(serviceContext);
                }

                return serviceContext.Result;
            }
        }

        async Task<byte[]> ServerStreamingServerMethod<TRequest, TResponse>(byte[] request, IServerStreamWriter<byte[]> responseStream, ServerCallContext context)
        {
            var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, MethodType.ClientStreaming, context)
            {
                RequestMarshaller = requestMarshaller,
                ResponseMarshaller = responseMarshaller,
                ResponseStream = responseStream
            };

            var deserializer = (Marshaller<TRequest>)requestMarshaller;
            var args = deserializer.Deserializer(request);

            if (responseIsTask)
            {
                var body = (Func<ServiceContext, TRequest, Task<ServerStreamingResult<TResponse>>>)this.methodBody;
                await body(serviceContext, args).ConfigureAwait(false);
            }
            else
            {
                var body = (Func<ServiceContext, TRequest, ServerStreamingResult<TResponse>>)this.methodBody;
                body(serviceContext, args);
            }

            return serviceContext.Result;
        }

        //Task DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
        //{
        //    var serviceContext = new ServiceContext(MethodType.DuplexStreaming, context);
        //    var body = (Func<IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServiceContext, Task<TResponse>>)this.methodBody;
        //    return body(requestStream, responseStream, serviceContext);
        //}

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }
}