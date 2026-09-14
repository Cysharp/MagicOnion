using System.Reflection;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    volatile bool isDisconnected;

    public IAsyncStreamReader<TRequest>? RequestStream { get; }
    public IServerStreamWriter<TResponse>? ResponseStream { get; }

    /// <summary>
    /// Gets the response queue consumer's completion task, or a completed task if the writer has not been created.
    /// </summary>
    internal Task ResponseWriterCompletion => streamingResponseWriter is { IsValueCreated: true }
        ? streamingResponseWriter.Value.Completion
        : Task.CompletedTask;

    // used in StreamingHub
    public bool IsDisconnected => isDisconnected;

    public StreamingServiceContext(
        object instance,
        IMagicOnionGrpcMethod method,
        ServerCallContext context,
        IMagicOnionSerializer messageSerializer,
        MagicOnionMetrics metrics,
        ILogger logger,
        IServiceProvider serviceProvider,
        IAsyncStreamReader<TRequest>? requestStream,
        IServerStreamWriter<TResponse>? responseStream
    ) : base(instance, method, context, messageSerializer, metrics, logger, serviceProvider)
    {
        RequestStream = requestStream;
        ResponseStream = responseStream;

        // streaming hub
        if (MethodType == MethodType.DuplexStreaming)
        {
            this.streamingResponseWriter = new Lazy<QueuedResponseWriter<TResponse>>(() =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<MagicOnionOptions>>().Value;
                // Capture the request lifetime before producers can run outside the request flow.
                var requestLifetime = context.GetHttpContext().Features.GetRequiredFeature<IHttpRequestLifetimeFeature>();
                return new QueuedResponseWriter<TResponse>(ResponseStream!, () => IsDisconnected, MagicOnionServerInternalLogger.Current,
                    static value =>
                    {
                        if (value is StreamingHubPayload payload)
                        {
                            StreamingHubPayloadPool.Shared.Return(payload);
                        }
                    },
                    static value => value is StreamingHubPayload payload ? payload.Length : 0,
                    () =>
                    {
                        CompleteStreamingHub();
                        // Queue closure alone cannot unblock an in-flight transport write.
                        requestLifetime.Abort();
                    },
                    options.StreamingHubResponseQueueMaxLength,
                    options.StreamingHubResponseQueueMaxSize);
            });
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
        isDisconnected = true;
        streamingResponseWriter.Value.Dispose();
    }
}
