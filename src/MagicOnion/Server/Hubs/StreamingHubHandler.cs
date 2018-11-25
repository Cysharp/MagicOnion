using MessagePack;
using System;
using System.Linq;
using MagicOnion.Utils;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public class StreamingHubHandler : IEquatable<StreamingHubHandler>
    {
        public string HubName { get; private set; }
        public Type HubType { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public int MethodId { get; private set; }

        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        readonly StreamingHubFilterAttribute[] filters;
        readonly Type RequestType;
        readonly Type UnwrappedResponseType;
        internal readonly IFormatterResolver resolver;
        internal readonly Func<StreamingHubContext, ValueTask> MethodBody;

        // reflection cache
        // Deserialize<T>(ArraySegment<byte> bytes, IFormatterResolver resolver)
        static readonly MethodInfo messagePackDeserialize = typeof(LZ4MessagePackSerializer).GetMethods()
            .First(x => x.Name == "Deserialize" && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(ArraySegment<byte>));

        public StreamingHubHandler(MagicOnionOptions options, Type classType, MethodInfo methodInfo)
        {
            this.HubType = classType;
            this.HubName = classType.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IStreamingHub<,>)).GetGenericArguments()[0].Name;
            this.MethodInfo = methodInfo;
            this.MethodId = FNV1A32.GetHashCode(methodInfo.Name); // TODO:GetFromAttribute or FNV1A32
            this.UnwrappedResponseType = UnwrapResponseType(methodInfo);
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

            this.filters = options.GlobalStreamingHubFilters
                .Concat(classType.GetCustomAttributes<StreamingHubFilterAttribute>(true))
                .Concat(methodInfo.GetCustomAttributes<StreamingHubFilterAttribute>(true))
                .OrderBy(x => x.Order)
                .ToArray();

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
                var getResolver = Expression.Property(contextArg, typeof(StreamingHubContext).GetProperty("FormatterResolver", flags));
                var contextRequest = Expression.Property(contextArg, typeof(StreamingHubContext).GetProperty("Request", flags));
                var getInstanceCast = Expression.Convert(Expression.Property(contextArg, typeof(StreamingHubContext).GetProperty("HubInstance", flags)), HubType);

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

                var callBody = Expression.Call(getInstanceCast, methodInfo, arguments);

                var finalMethod = (methodInfo.ReturnType.IsGenericType)
                    ? typeof(StreamingHubContext).GetMethod(nameof(StreamingHubContext.WriteResponseMessage), flags).MakeGenericMethod(UnwrappedResponseType)
                    : typeof(StreamingHubContext).GetMethod(nameof(StreamingHubContext.WrapToValueTask), flags);
                callBody = Expression.Call(contextArg, finalMethod, callBody);

                var body = Expression.Block(new[] { requestArg }, assignRequest, callBody);
                var compiledBody = Expression.Lambda(body, contextArg).Compile();

                this.MethodBody = BuildMethodBodyWithFilter((Func<StreamingHubContext, ValueTask>)compiledBody);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
            }
        }

        static Type UnwrapResponseType(MethodInfo methodInfo)
        {
            var t = methodInfo.ReturnType;
            if (!typeof(Task).IsAssignableFrom(t)) throw new Exception($"Invalid return type, Hub return type must be Task or Task<T>. path:{methodInfo.DeclaringType.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");

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

        Func<StreamingHubContext, ValueTask> BuildMethodBodyWithFilter(Func<StreamingHubContext, ValueTask> methodBody)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Func<StreamingHubContext, ValueTask> next = methodBody;

            foreach (var filter in this.filters.Reverse())
            {
                var fields = filter.GetType().GetFields(flags);

                var newFilter = (StreamingHubFilterAttribute)Activator.CreateInstance(filter.GetType(), new object[] { next });
                // copy all data.
                foreach (var item in fields)
                {
                    item.SetValue(newFilter, item.GetValue(filter));
                }

                next = newFilter.Invoke;
            }

            return next;
        }

        public override string ToString()
        {
            return HubName + "/" + MethodInfo.Name;
        }

        public override int GetHashCode()
        {
            return HubName.GetHashCode() ^ MethodInfo.Name.GetHashCode() << 2;
        }

        public bool Equals(StreamingHubHandler other)
        {
            return HubName.Equals(other.HubName) && MethodInfo.Name.Equals(other.MethodInfo.Name);
        }
    }
}
