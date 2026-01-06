using System.Buffers;
using System.Diagnostics;
using System.Reflection;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionStreamingHubMethod
{
    string ServiceName { get; }
    string MethodName { get; }
    StreamingHubMethodHandlerMetadata Metadata { get; }

    ValueTask InvokeAsync(StreamingHubContext context);
}

public class MagicOnionStreamingHubMethod<TService, TRequest, TResponse> : IMagicOnionStreamingHubMethod
{
    public string ServiceName { get; }
    public string MethodName { get; }
    public StreamingHubMethodHandlerMetadata Metadata { get; }

    readonly Func<TService, StreamingHubContext, TRequest, ValueTask<TResponse>> invoker;

    // for Dynamic
    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Delegate invoker)
    {
        Debug.Assert(invoker is Func<TService, StreamingHubContext, TRequest, Task<TResponse>> or Func<TService, StreamingHubContext, TRequest, ValueTask<TResponse>>);

        this.ServiceName = serviceName;
        this.MethodName  = methodName;
        this.Metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata<TService>(MethodName);

        if (invoker is Func<TService, StreamingHubContext, TRequest, Task<TResponse>> invokerTask)
        {
            this.invoker = InvokeTask;
            ValueTask<TResponse> InvokeTask(TService instance, StreamingHubContext context, TRequest request)
                => new(invokerTask(instance, context, request));
        }
        else
        {
            this.invoker = (Func<TService, StreamingHubContext, TRequest, ValueTask<TResponse>>)invoker;
        }
    }

    // for Static (AOT)
    public MagicOnionStreamingHubMethod(string serviceName, string methodName, StreamingHubMethodHandlerMetadata metadata, Delegate invoker)
    {
        Debug.Assert(invoker is Func<TService, StreamingHubContext, TRequest, Task<TResponse>> or Func<TService, StreamingHubContext, TRequest, ValueTask<TResponse>>);

        this.ServiceName = serviceName;
        this.MethodName  = methodName;
        this.Metadata = metadata;

        if (invoker is Func<TService, StreamingHubContext, TRequest, Task<TResponse>> invokerTask)
        {
            this.invoker = InvokeTask;
            ValueTask<TResponse> InvokeTask(TService instance, StreamingHubContext context, TRequest request)
                => new(invokerTask(instance, context, request));
        }
        else
        {
            this.invoker = (Func<TService, StreamingHubContext, TRequest, ValueTask<TResponse>>)invoker;
        }
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, StreamingHubContext, TRequest, ValueTask<TResponse>> invoker) : this(serviceName, methodName, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, StreamingHubContext, TRequest, Task<TResponse>> invoker) : this(serviceName, methodName, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, StreamingHubMethodHandlerMetadata metadata, Func<TService, StreamingHubContext, TRequest, ValueTask<TResponse>> invoker) : this(serviceName, methodName, metadata, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, StreamingHubMethodHandlerMetadata metadata, Func<TService, StreamingHubContext, TRequest, Task<TResponse>> invoker) : this(serviceName, methodName, metadata, (Delegate)invoker)
    {
    }

    public ValueTask InvokeAsync(StreamingHubContext context)
    {
        var seq = new ReadOnlySequence<byte>(context.Request);
        TRequest request = context.ServiceContext.MessageSerializer.Deserialize<TRequest>(seq);
        var result = invoker((TService)context.HubInstance, context, request);
        return context.WriteResponseMessage(result);
    }
}

public class MagicOnionStreamingHubMethod<TService, TRequest> : IMagicOnionStreamingHubMethod
{
    public string ServiceName { get; }
    public string MethodName { get; }
    public StreamingHubMethodHandlerMetadata Metadata { get; }

    readonly Func<TService, StreamingHubContext, TRequest, ValueTask> invoker;

    // for Dynamic
    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Delegate invoker)
    {
        Debug.Assert(invoker is Func<TService, StreamingHubContext, TRequest, Task> or Func<TService, StreamingHubContext, TRequest, ValueTask> or Action<TService, StreamingHubContext, TRequest>);

        this.ServiceName = serviceName;
        this.MethodName = methodName;
        this.Metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata<TService>(MethodName);

        if (invoker is Func<TService, StreamingHubContext, TRequest, Task> invokerTask)
        {
            this.invoker = InvokeTask;
            ValueTask InvokeTask(TService instance, StreamingHubContext context, TRequest request)
                => new(invokerTask(instance, context, request));
        }
        else if (invoker is Action<TService, StreamingHubContext, TRequest> invokerVoid)
        {
            this.invoker = InvokeVoid;
            ValueTask InvokeVoid(TService instance, StreamingHubContext context, TRequest request)
            {
                invokerVoid(instance, context, request);
                return default;
            }
        }
        else
        {
            this.invoker = (Func<TService, StreamingHubContext, TRequest, ValueTask>)invoker;
        }
    }

    // for Static (AOT)
    public MagicOnionStreamingHubMethod(string serviceName, string methodName, StreamingHubMethodHandlerMetadata metadata, Delegate invoker)
    {
        Debug.Assert(invoker is Func<TService, StreamingHubContext, TRequest, Task> or Func<TService, StreamingHubContext, TRequest, ValueTask> or Action<TService, StreamingHubContext, TRequest>);

        this.ServiceName = serviceName;
        this.MethodName = methodName;
        this.Metadata = metadata;

        if (invoker is Func<TService, StreamingHubContext, TRequest, Task> invokerTask)
        {
            this.invoker = InvokeTask;
            ValueTask InvokeTask(TService instance, StreamingHubContext context, TRequest request)
                => new(invokerTask(instance, context, request));
        }
        else if (invoker is Action<TService, StreamingHubContext, TRequest> invokerVoid)
        {
            this.invoker = InvokeVoid;
            ValueTask InvokeVoid(TService instance, StreamingHubContext context, TRequest request)
            {
                invokerVoid(instance, context, request);
                return default;
            }
        }
        else
        {
            this.invoker = (Func<TService, StreamingHubContext, TRequest, ValueTask>)invoker;
        }
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, StreamingHubContext, TRequest, ValueTask> invoker) : this(serviceName, methodName, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, StreamingHubContext, TRequest, Task> invoker) : this(serviceName, methodName, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Action<TService, StreamingHubContext, TRequest> invoker) : this(serviceName, methodName, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, StreamingHubMethodHandlerMetadata metadata, Func<TService, StreamingHubContext, TRequest, ValueTask> invoker) : this(serviceName, methodName, metadata, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, StreamingHubMethodHandlerMetadata metadata, Func<TService, StreamingHubContext, TRequest, Task> invoker) : this(serviceName, methodName, metadata, (Delegate)invoker)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, StreamingHubMethodHandlerMetadata metadata, Action<TService, StreamingHubContext, TRequest> invoker) : this(serviceName, methodName, metadata, (Delegate)invoker)
    {
    }

    public ValueTask InvokeAsync(StreamingHubContext context)
    {
        var seq = new ReadOnlySequence<byte>(context.Request);
        TRequest request = context.ServiceContext.MessageSerializer.Deserialize<TRequest>(seq);
        var response = invoker((TService)context.HubInstance, context, request);
        return context.WriteResponseMessageNil(response);
    }
}

