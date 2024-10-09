using System.Reflection;
using Grpc.Core;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server;

public interface IStreamingServiceContext : IServiceContext
{
    bool IsDisconnected { get; }
    void CompleteStreamingHub();
}

public interface IServiceContextWithRequestStream<T> : IStreamingServiceContext
{
    IAsyncStreamReader<T>? RequestStream { get; }
}

public interface IServiceContextWithResponseStream<T> : IStreamingServiceContext
{
    IServerStreamWriter<T>? ResponseStream { get; }
    void QueueResponseStreamWrite(in T value);
}

public interface IStreamingServiceContext<TRequest, TResponse> : IServiceContextWithRequestStream<TRequest>, IServiceContextWithResponseStream<TResponse>
{}

internal class StreamingServiceContext<TRequest, TResponse> : ServiceContext, IStreamingServiceContext<TRequest, TResponse>
{
    readonly Lazy<QueuedResponseWriter<TResponse>> streamingResponseWriter;

    public IAsyncStreamReader<TRequest>? RequestStream { get; }
    public IServerStreamWriter<TResponse>? ResponseStream { get; }

    // used in StreamingHub
    public bool IsDisconnected { get; private set; }

    public StreamingServiceContext(
        object instance,
        Type serviceType,
        string serviceName,
        MethodInfo methodInfo,
        ILookup<Type, Attribute> attributeLookup,
        MethodType methodType,
        ServerCallContext context,
        IMagicOnionSerializer messageSerializer,
        ILogger logger,
        MethodHandler methodHandler,
        IServiceProvider serviceProvider,
        IAsyncStreamReader<TRequest>? requestStream,
        IServerStreamWriter<TResponse>? responseStream
    ) : base(instance, serviceType, serviceName, methodInfo, attributeLookup, methodType, context, messageSerializer, logger, methodHandler, serviceProvider)
    {
        RequestStream = requestStream;
        ResponseStream = responseStream;

        // streaming hub
        if (methodType == MethodType.DuplexStreaming)
        {
            this.streamingResponseWriter = new Lazy<QueuedResponseWriter<TResponse>>(() => new QueuedResponseWriter<TResponse>(this));
        }
        else
        {
            this.streamingResponseWriter = null!;
        }
    }

    // used in StreamingHub
    public void QueueResponseStreamWrite(in TResponse value)
    {
        streamingResponseWriter.Value.Write(value);
    }

    public void CompleteStreamingHub()
    {
        IsDisconnected = true;
        streamingResponseWriter.Value.Dispose();
    }
}
