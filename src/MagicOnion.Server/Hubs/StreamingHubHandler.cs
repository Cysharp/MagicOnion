using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using MagicOnion.Utils;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;

namespace MagicOnion.Server.Hubs
{
    public class StreamingHubHandler : IEquatable<StreamingHubHandler>
    {
        public string HubName { get; private set; }
        public Type HubType { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public int MethodId { get; private set; }

        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        internal readonly Type RequestType;
        readonly Type? UnwrappedResponseType;
        internal readonly MessagePackSerializerOptions serializerOptions;
        internal readonly Func<StreamingHubContext, ValueTask> MethodBody;

        readonly string toStringCache;
        readonly int getHashCodeCache;

        // reflection cache
        // Deserialize<T>(ReadOnlyMemory<byte>, MessagePackSerializerOptions, CancellationToken)
        static readonly MethodInfo messagePackDeserialize = typeof(MessagePackSerializer).GetMethods()
            .First(x => x.Name == "Deserialize" && x.GetParameters().Length == 3 && x.GetParameters()[0].ParameterType == typeof(ReadOnlyMemory<byte>) && x.GetParameters()[1].ParameterType == typeof(MessagePackSerializerOptions));

        private static MethodInfo GetInterfaceMethod(Type targetType, Type interfaceType, string targetMethodName)
        {
            var mapping = targetType.GetInterfaceMap(interfaceType);
            var methodIndex = Array.FindIndex(mapping.TargetMethods, mi => mi.Name == targetMethodName);
            return mapping.InterfaceMethods[methodIndex];
        }

        public StreamingHubHandler(Type classType, MethodInfo methodInfo, StreamingHubHandlerOptions handlerOptions, IServiceProvider serviceProvider)
        {
            var hubInterface = classType.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IStreamingHub<,>)).GetGenericArguments()[0];
            var interfaceMethod = GetInterfaceMethod(classType, hubInterface, methodInfo.Name);

            this.HubType = classType;
            this.HubName = hubInterface.Name;
            this.MethodInfo = methodInfo;
            // Validation for Id
            if (methodInfo.GetCustomAttribute<MethodIdAttribute>() != null)
            {
                throw new InvalidOperationException($"Hub Implementation can not add [MethodId], you should add hub `interface`. {classType.Name}/{methodInfo.Name}");
            }
            this.MethodId = interfaceMethod.GetCustomAttribute<MethodIdAttribute>()?.MethodId ?? FNV1A32.GetHashCode(interfaceMethod.Name);

            this.UnwrappedResponseType = UnwrapResponseType(methodInfo);

            var resolver = handlerOptions.SerializerOptions.Resolver;
            var parameters = methodInfo.GetParameters();
            this.RequestType = MagicOnionMarshallers.CreateRequestTypeAndSetResolver(classType.Name + "/" + methodInfo.Name, parameters, ref resolver);

            this.serializerOptions = handlerOptions.SerializerOptions.WithResolver(resolver);

            this.AttributeLookup = classType.GetCustomAttributes(true)
                .Concat(methodInfo.GetCustomAttributes(true))
                .Cast<Attribute>()
                .ToLookup(x => x.GetType());

            this.toStringCache = HubName + "/" + MethodInfo.Name;
            this.getHashCodeCache = HubName.GetHashCode() ^ MethodInfo.Name.GetHashCode() << 2;

            // ValueTask (StreamingHubContext context) =>
            // {
            //    T request = LZ4MessagePackSerializer.Deserialize<T>(context.Request, context.FormatterResolver);
            //    Task<T> result = ((HubType)context.HubInstance).Foo(request);
            //    return WriteInAsyncLockInTaskWithMessageId(result) || return new ValueTask(result)
            // }
            try
            {
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var contextArg = Expression.Parameter(typeof(StreamingHubContext), "context");
                var requestArg = Expression.Parameter(RequestType, "request");
                var getSerializerOptions = Expression.Property(contextArg, typeof(StreamingHubContext).GetProperty("SerializerOptions", flags)!);
                var contextRequest = Expression.Property(contextArg, typeof(StreamingHubContext).GetProperty("Request", flags)!);
                var noneCancellation = Expression.Default(typeof(CancellationToken));
                var getInstanceCast = Expression.Convert(Expression.Property(contextArg, typeof(StreamingHubContext).GetProperty("HubInstance", flags)!), HubType);

                var callDeserialize = Expression.Call(messagePackDeserialize.MakeGenericMethod(RequestType), contextRequest, getSerializerOptions, noneCancellation);
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

                var callBody = Expression.Call(getInstanceCast, methodInfo, arguments);

                var finalMethod = (methodInfo.ReturnType.IsGenericType)
                    ? typeof(StreamingHubContext).GetMethod(nameof(StreamingHubContext.WriteResponseMessage), flags)!.MakeGenericMethod(UnwrappedResponseType!)
                    : typeof(StreamingHubContext).GetMethod(nameof(StreamingHubContext.WriteResponseMessageNil), flags)!;
                callBody = Expression.Call(contextArg, finalMethod, callBody);

                var body = Expression.Block(new[] { requestArg }, assignRequest, callBody);
                var compiledBody = Expression.Lambda(body, contextArg).Compile();

                var filters = FilterHelper.GetFilters(handlerOptions.GlobalStreamingHubFilters, classType, methodInfo);
                this.MethodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (Func<StreamingHubContext, ValueTask>)compiledBody);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
            }
        }

        static Type? UnwrapResponseType(MethodInfo methodInfo)
        {
            var t = methodInfo.ReturnType;
            if (!typeof(Task).IsAssignableFrom(t)) throw new Exception($"Invalid return type, Hub return type must be Task or Task<T>. path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");

            if (t.IsGenericType)
            {
                // Task<T>
                return t.GetGenericArguments()[0];
            }
            else
            {
                // Task
                return null;
            }
        }

        public override string ToString()
        {
            return toStringCache;
        }

        public override int GetHashCode()
        {
            return getHashCodeCache;
        }

        public bool Equals(StreamingHubHandler? other)
        {
            return other != null && HubName.Equals(other.HubName) && MethodInfo.Name.Equals(other.MethodInfo.Name);
        }
    }

    /// <summary>
    /// Options for StreamingHubHandler construction.
    /// </summary>
    public class StreamingHubHandlerOptions
    {
        public IList<StreamingHubFilterDescriptor> GlobalStreamingHubFilters { get; }

        public MessagePackSerializerOptions SerializerOptions { get; }

        public StreamingHubHandlerOptions(MagicOnionOptions options)
        {
            GlobalStreamingHubFilters = options.GlobalStreamingHubFilters;
            SerializerOptions = options.SerializerOptions;
        }
    }
}
