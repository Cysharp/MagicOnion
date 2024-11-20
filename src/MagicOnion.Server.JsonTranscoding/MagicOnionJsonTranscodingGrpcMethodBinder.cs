using System.Buffers;
using System.Text.Json;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Binder.Internal;
using MessagePack;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingGrpcMethodBinder<TService>(
    ServiceMethodProviderContext<TService> context,
    IGrpcServiceActivator<TService> serviceActivator,
    MagicOnionJsonTranscodingOptions options,
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory
) : IMagicOnionGrpcMethodBinder<TService>
    where TService : class
{
    public void BindUnary<TRequest, TResponse, TRawRequest, TRawResponse>(IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method) where TRawRequest : class where TRawResponse : class
    {
        var messageSerializer = new MessagePackJsonMessageSerializer(options.MessagePackSerializerOptions ?? MessagePackSerializer.DefaultOptions);

        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.Unary, method.ServiceName, method.MethodName, messageSerializer);

        var handlerBuilder = new MagicOnionGrpcMethodHandler<TService>(enableCurrentContext: false, isReturnExceptionStackTraceInErrorDetail: false, serviceProvider, [], loggerFactory.CreateLogger<MagicOnionGrpcMethodHandler<TService>>());
        var unaryMethodHandler = handlerBuilder.BuildUnaryMethod(method, messageSerializer);

        context.AddMethod(grpcMethod, RoutePatternFactory.Parse($"/_/{method.ServiceName}/{method.MethodName}"), method.Metadata.Metadata.ToArray(), async (context) =>
        {
            var serverCallContext = new MagicOnionJsonTranscodingServerCallContext(method);
            serverCallContext.UserState["__HttpContext"] = context;
            context.Features.Set<IServerCallContextFeature>(serverCallContext);

            GrpcActivatorHandle<TService> handle = default;
            try
            {
                handle = serviceActivator.Create(context.RequestServices);

                var memStream = new MemoryStream();
                await context.Request.BodyReader.CopyToAsync(memStream);

                // If the request type is `Nil` (parameter-less method), we always ignore the request body.
                TRawRequest request = (typeof(TRequest) == typeof(Nil))
                    ? (TRawRequest)(object)Box.Create(Nil.Default)
                    : grpcMethod.RequestMarshaller.ContextualDeserializer(new DeserializationContextImpl(memStream.ToArray()));

                var response = await unaryMethodHandler(handle.Instance, request, serverCallContext);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;

                grpcMethod.ResponseMarshaller.ContextualSerializer(response, new SerializationContextImpl(context.Response.BodyWriter));
            }
            catch (RpcException ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                var status = ex.Status;
                await context.Response.BodyWriter.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(new { Code = status.StatusCode, Detail = status.Detail }));
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                var status = new Status(StatusCode.Internal, $"{ex.GetType().FullName}: {ex.Message}");
                await context.Response.BodyWriter.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(new { Code = status.StatusCode, Detail = status.Detail }));
            }
            finally
            {
                if (handle.Instance != null)
                {
                    await serviceActivator.ReleaseAsync(handle);
                }
            }

        });
    }

    class SerializationContextImpl(IBufferWriter<byte> writer) : SerializationContext
    {
        public override IBufferWriter<byte> GetBufferWriter() => writer;
        public override void Complete() {}
        public override void SetPayloadLength(int payloadLength) => throw new NotSupportedException();
        public override void Complete(byte[] payload) => throw new NotSupportedException();
    }


    class DeserializationContextImpl(ReadOnlyMemory<byte> bytes) : DeserializationContext
    {
        public override int PayloadLength => bytes.Length;
        public override ReadOnlySequence<byte> PayloadAsReadOnlySequence() => new(bytes);
    }

    public void BindClientStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method) where TRawRequest : class where TRawResponse : class
    {
        // Ignore (Currently, not supported)
        throw new NotSupportedException("JsonTranscoding does not support ClientStreaming, ServerStreaming and DuplexStreaming.");
    }

    public void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method) where TRawRequest : class where TRawResponse : class
    {
        // Ignore (Currently, not supported)
        throw new NotSupportedException("JsonTranscoding does not support ClientStreaming, ServerStreaming and DuplexStreaming.");
    }

    public void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method) where TRawRequest : class where TRawResponse : class
    {
        // Ignore (Currently, not supported)
        throw new NotSupportedException("JsonTranscoding does not support ClientStreaming, ServerStreaming and DuplexStreaming.");
    }

    public void BindStreamingHub(MagicOnionStreamingHubConnectMethod<TService> method)
    {
        // Ignore (Currently, not supported)
        throw new NotSupportedException("JsonTranscoding does not support ClientStreaming, ServerStreaming and DuplexStreaming.");
    }
}
