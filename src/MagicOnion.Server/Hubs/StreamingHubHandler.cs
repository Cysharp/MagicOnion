using System.Buffers;
using MessagePack;
using System.Linq.Expressions;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Hubs;

public class StreamingHubHandler : IEquatable<StreamingHubHandler>
{
    readonly StreamingHubMethodHandlerMetadata metadata;
    readonly string toStringCache;
    readonly int getHashCodeCache;

    public string HubName => metadata.StreamingHubInterfaceType.Name;
    public Type HubType => metadata.StreamingHubImplementationType;
    public MethodInfo MethodInfo => metadata.ImplementationMethod;
    public int MethodId => metadata.MethodId;

    public ILookup<Type, Attribute> AttributeLookup => metadata.AttributeLookup;

    internal Type RequestType => metadata.RequestType;
    internal IMagicOnionMessageSerializer MessageSerializer { get; }
    internal Func<StreamingHubContext, ValueTask> MethodBody { get; }

    public StreamingHubHandler(Type classType, MethodInfo methodInfo, StreamingHubHandlerOptions handlerOptions, IServiceProvider serviceProvider)
    {
        this.metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(classType, methodInfo);
        this.MessageSerializer = handlerOptions.MessageSerializer;
        this.toStringCache = HubName + "/" + MethodInfo.Name;
        this.getHashCodeCache = HashCode.Combine(HubName, MethodInfo.Name);

        var parameters = metadata.Parameters;
        try
        {
            // var invokeHubMethodFunc = (context, request) => ((HubType)context.HubInstance).Foo(request);
            // or
            // var invokeHubMethodFunc = (context, request) => ((HubType)context.HubInstance).Foo(request.Item1, request.Item2 ...);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var contextArg = Expression.Parameter(typeof(StreamingHubContext), "context");
            var requestArg = Expression.Parameter(RequestType, "request");
            var getInstanceCast = Expression.Convert(Expression.Property(contextArg, typeof(StreamingHubContext).GetProperty(nameof(StreamingHubContext.HubInstance), flags)!), HubType);
            Expression[] arguments = new Expression[parameters.Count];
            if (parameters.Count == 1)
            {
                arguments[0] = requestArg;
            }
            else
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    arguments[i] = Expression.Field(requestArg, "Item" + (i + 1));
                }
            }
            var callHubMethod = Expression.Call(getInstanceCast, methodInfo, arguments);
            var invokeHubMethodFunc = Expression.Lambda(callHubMethod, contextArg, requestArg).Compile();

            // Create a StreamingHub method invoker and a wrapped-invoke method.
            Type invokerType = metadata.ResponseType is null
                ? typeof(StreamingHubMethodInvoker<>).MakeGenericType(metadata.RequestType)
                : typeof(StreamingHubMethodInvoker<,>).MakeGenericType(metadata.RequestType, metadata.ResponseType);
            StreamingHubMethodInvoker invoker = (StreamingHubMethodInvoker)Activator.CreateInstance(invokerType, MessageSerializer, invokeHubMethodFunc)!;

            var filters = FilterHelper.GetFilters(handlerOptions.GlobalStreamingHubFilters, classType, methodInfo);
            this.MethodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, invoker.InvokeAsync);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
        }
    }

    abstract class StreamingHubMethodInvoker
    {
        protected IMagicOnionMessageSerializer MessageSerializer { get; }

        public StreamingHubMethodInvoker(IMagicOnionMessageSerializer messageSerializer)
        {
            MessageSerializer = messageSerializer;
        }

        public abstract ValueTask InvokeAsync(StreamingHubContext context);
    }

    sealed class StreamingHubMethodInvoker<TRequest, TResponse> : StreamingHubMethodInvoker
    {
        readonly Func<StreamingHubContext, TRequest, Task<TResponse>> hubMethodFunc;

        public StreamingHubMethodInvoker(IMagicOnionMessageSerializer messageSerializer, Delegate hubMethodFunc) : base(messageSerializer)
        {
            this.hubMethodFunc = (Func<StreamingHubContext, TRequest, Task<TResponse>>)hubMethodFunc;
        }

        public override ValueTask InvokeAsync(StreamingHubContext context)
        {
            var seq = new ReadOnlySequence<byte>(context.Request);
            TRequest request = MessageSerializer.Deserialize<TRequest>(seq);
            Task<TResponse> response = hubMethodFunc(context, request);
            return context.WriteResponseMessage(response);
        }
    }

    sealed class StreamingHubMethodInvoker<TRequest> : StreamingHubMethodInvoker
    {
        readonly Func<StreamingHubContext, TRequest, Task> hubMethodFunc;

        public StreamingHubMethodInvoker(IMagicOnionMessageSerializer messageSerializer, Delegate hubMethodFunc) : base(messageSerializer)
        {
            this.hubMethodFunc = (Func<StreamingHubContext, TRequest, Task>)hubMethodFunc;
        }

        public override ValueTask InvokeAsync(StreamingHubContext context)
        {
            var seq = new ReadOnlySequence<byte>(context.Request);
            TRequest request = MessageSerializer.Deserialize<TRequest>(seq);
            Task response = hubMethodFunc(context, request);
            return context.WriteResponseMessageNil(response);
        }
    }

    public override string ToString()
        => toStringCache;

    public override int GetHashCode()
        => getHashCodeCache;

    public bool Equals(StreamingHubHandler? other)
        => other != null && HubName.Equals(other.HubName) && MethodInfo.Name.Equals(other.MethodInfo.Name);
}

/// <summary>
/// Options for StreamingHubHandler construction.
/// </summary>
public class StreamingHubHandlerOptions
{
    public IList<StreamingHubFilterDescriptor> GlobalStreamingHubFilters { get; }

    public IMagicOnionMessageSerializer MessageSerializer { get; }

    public StreamingHubHandlerOptions(MagicOnionOptions options)
    {
        GlobalStreamingHubFilters = options.GlobalStreamingHubFilters;
        MessageSerializer = options.MessageSerializer;
    }
}
