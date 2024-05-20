using System.Buffers;
using MessagePack;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Grpc.Core;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Internal;
using MagicOnion.Serialization;

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
    internal Func<StreamingHubContext, ValueTask> MethodBody { get; }

    public StreamingHubHandler(Type classType, MethodInfo methodInfo, StreamingHubHandlerOptions handlerOptions, IServiceProvider serviceProvider)
    {
        this.metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(classType, methodInfo);
        this.toStringCache = HubName + "/" + MethodInfo.Name;
        this.getHashCodeCache = HashCode.Combine(HubName, MethodInfo.Name);

        var messageSerializer = handlerOptions.MessageSerializer.Create(MethodType.DuplexStreaming, methodInfo);
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
            Type invokerType = StreamingHubMethodInvoker.CreateInvokerTypeFromMetadata(metadata);
            StreamingHubMethodInvoker invoker = (StreamingHubMethodInvoker)Activator.CreateInstance(invokerType, messageSerializer, invokeHubMethodFunc)!;

            var filters = FilterHelper.GetFilters(handlerOptions.GlobalStreamingHubFilters, classType, methodInfo);
            this.MethodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, invoker.InvokeAsync);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
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

    public IMagicOnionSerializerProvider MessageSerializer { get; }

    public StreamingHubHandlerOptions(MagicOnionOptions options)
    {
        GlobalStreamingHubFilters = options.GlobalStreamingHubFilters;
        MessageSerializer = options.MessageSerializer;
    }
}

internal abstract class StreamingHubMethodInvoker
{
    protected IMagicOnionSerializer MessageSerializer { get; }

    protected StreamingHubMethodInvoker(IMagicOnionSerializer messageSerializer)
    {
        MessageSerializer = messageSerializer;
    }

    public abstract ValueTask InvokeAsync(StreamingHubContext context);

    public static Type CreateInvokerTypeFromMetadata(in StreamingHubMethodHandlerMetadata metadata)
    {
        var isVoid = metadata.InterfaceMethod.ReturnType == typeof(void);
        var isTaskOrTaskOfT = metadata.InterfaceMethod.ReturnType == typeof(Task) ||
                              (metadata.InterfaceMethod.ReturnType is { IsGenericType: true } t && t.BaseType == typeof(Task));
        return isVoid
            ? typeof(StreamingHubMethodInvokerVoid<>).MakeGenericType(metadata.RequestType)
            : isTaskOrTaskOfT
                ? (metadata.ResponseType is null
                    ? typeof(StreamingHubMethodInvokerTask<>).MakeGenericType(metadata.RequestType)
                    : typeof(StreamingHubMethodInvokerTask<,>).MakeGenericType(metadata.RequestType, metadata.ResponseType)
                )
                : (metadata.ResponseType is null
                    ? typeof(StreamingHubMethodInvokerValueTask<>).MakeGenericType(metadata.RequestType)
                    : typeof(StreamingHubMethodInvokerValueTask<,>).MakeGenericType(metadata.RequestType, metadata.ResponseType)
                );
    }

    sealed class StreamingHubMethodInvokerVoid<TRequest> : StreamingHubMethodInvoker
    {
        readonly Action<StreamingHubContext, TRequest> hubMethodFunc;

        public StreamingHubMethodInvokerVoid(IMagicOnionSerializer messageSerializer, Delegate hubMethodFunc) : base(messageSerializer)
        {
            this.hubMethodFunc = (Action<StreamingHubContext, TRequest>)hubMethodFunc;
        }

        public override ValueTask InvokeAsync(StreamingHubContext context)
        {
            var seq = new ReadOnlySequence<byte>(context.Request);
            TRequest request = MessageSerializer.Deserialize<TRequest>(seq);
            hubMethodFunc(context, request);
            return context.WriteResponseMessageNil(default);
        }
    }

    sealed class StreamingHubMethodInvokerTask<TRequest, TResponse> : StreamingHubMethodInvoker
    {
        readonly Func<StreamingHubContext, TRequest, Task<TResponse>> hubMethodFunc;

        public StreamingHubMethodInvokerTask(IMagicOnionSerializer messageSerializer, Delegate hubMethodFunc) : base(messageSerializer)
        {
            this.hubMethodFunc = (Func<StreamingHubContext, TRequest, Task<TResponse>>)hubMethodFunc;
        }

        public override ValueTask InvokeAsync(StreamingHubContext context)
        {
            var seq = new ReadOnlySequence<byte>(context.Request);
            TRequest request = MessageSerializer.Deserialize<TRequest>(seq);
            Task<TResponse> response = hubMethodFunc(context, request);
            return context.WriteResponseMessage(new ValueTask<TResponse>(response));
        }
    }

    sealed class StreamingHubMethodInvokerTask<TRequest> : StreamingHubMethodInvoker
    {
        readonly Func<StreamingHubContext, TRequest, Task> hubMethodFunc;

        public StreamingHubMethodInvokerTask(IMagicOnionSerializer messageSerializer, Delegate hubMethodFunc) : base(messageSerializer)
        {
            this.hubMethodFunc = (Func<StreamingHubContext, TRequest, Task>)hubMethodFunc;
        }

        public override ValueTask InvokeAsync(StreamingHubContext context)
        {
            var seq = new ReadOnlySequence<byte>(context.Request);
            TRequest request = MessageSerializer.Deserialize<TRequest>(seq);
            Task response = hubMethodFunc(context, request);
            return context.WriteResponseMessageNil(new ValueTask(response));
        }
    }

    sealed class StreamingHubMethodInvokerValueTask<TRequest, TResponse> : StreamingHubMethodInvoker
    {
        readonly Func<StreamingHubContext, TRequest, ValueTask<TResponse>> hubMethodFunc;

        public StreamingHubMethodInvokerValueTask(IMagicOnionSerializer messageSerializer, Delegate hubMethodFunc) : base(messageSerializer)
        {
            this.hubMethodFunc = (Func<StreamingHubContext, TRequest, ValueTask<TResponse>>)hubMethodFunc;
        }

        public override ValueTask InvokeAsync(StreamingHubContext context)
        {
            var seq = new ReadOnlySequence<byte>(context.Request);
            TRequest request = MessageSerializer.Deserialize<TRequest>(seq);
            ValueTask<TResponse> response = hubMethodFunc(context, request);
            return context.WriteResponseMessage(response);
        }
    }

    sealed class StreamingHubMethodInvokerValueTask<TRequest> : StreamingHubMethodInvoker
    {
        readonly Func<StreamingHubContext, TRequest, ValueTask> hubMethodFunc;

        public StreamingHubMethodInvokerValueTask(IMagicOnionSerializer messageSerializer, Delegate hubMethodFunc) : base(messageSerializer)
        {
            this.hubMethodFunc = (Func<StreamingHubContext, TRequest, ValueTask>)hubMethodFunc;
        }

        public override ValueTask InvokeAsync(StreamingHubContext context)
        {
            var seq = new ReadOnlySequence<byte>(context.Request);
            TRequest request = MessageSerializer.Deserialize<TRequest>(seq);
            ValueTask response = hubMethodFunc(context, request);
            return context.WriteResponseMessageNil(response);
        }
    }

}
