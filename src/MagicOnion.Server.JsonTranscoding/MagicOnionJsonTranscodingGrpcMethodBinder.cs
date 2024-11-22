using System.Buffers;
using System.Net;
using System.Text.Json;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Binder.Internal;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingGrpcMethodBinder<TService>(
    ServiceMethodProviderContext<TService> context,
    IGrpcServiceActivator<TService> serviceActivator,
    JsonOptions jsonOptions,
    MagicOnionJsonTranscodingOptions transcodingOptions,
    MagicOnionOptions options,
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory
) : IMagicOnionGrpcMethodBinder<TService>
    where TService : class
{
    public void BindUnary<TRequest, TResponse, TRawRequest, TRawResponse>(IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method) where TRawRequest : class where TRawResponse : class
    {
        var parameterNames = method.Metadata.Parameters.Select(x => x.Name!).ToArray();
        var messageSerializer = new SystemTextJsonMessageSerializer(jsonOptions.SerializerOptions ?? JsonSerializerOptions.Default, parameterNames);

        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.Unary, method.ServiceName, method.MethodName, messageSerializer);

        var handlerBuilder = new MagicOnionGrpcMethodHandler<TService>(
            options.EnableCurrentContext,
            options.IsReturnExceptionStackTraceInErrorDetail,
            serviceProvider,
            options.GlobalFilters,
            loggerFactory.CreateLogger<MagicOnionGrpcMethodHandler<TService>>()
        );
        var unaryMethodHandler = handlerBuilder.BuildUnaryMethod(method, messageSerializer);

        var routePath = $"{transcodingOptions.RoutePathPrefix.TrimEnd('/')}/{method.ServiceName}/{method.MethodName}";
        var metadata = method.Metadata.Metadata.Append(new MagicOnionJsonTranscodingMetadata(routePath, typeof(TRequest), typeof(TResponse), method)).ToArray();

        context.AddMethod(grpcMethod, RoutePatternFactory.Parse(routePath), metadata, async (context) =>
        {
            var serverCallContext = new MagicOnionJsonTranscodingServerCallContext(method);

            // Grpc.AspNetCore.Server expects that UserState has the key "__HttpContext" and that HttpContext is set to it.
            // https://github.com/grpc/grpc-dotnet/blob/5a58c24efc1d0b7c5ff88e7b0582ea891b90b17f/src/Grpc.AspNetCore.Server/ServerCallContextExtensions.cs#L30
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
            catch (Exception ex)
            {
                await WriteErrorResponseAsync(context, ex);
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

    async ValueTask WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var status = (ex is RpcException rpcException)
            ? rpcException.Status
            : new Status(StatusCode.Internal, "Exception was thrown by handler." + (options.IsReturnExceptionStackTraceInErrorDetail ? $" ({ex.GetType().FullName}: {ex.Message})\n{ex.StackTrace}" : string.Empty));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = MapStatusCodeToHttpStatus(status.StatusCode);

        await JsonSerializer.SerializeAsync(context.Response.Body, new { Code = (int)status.StatusCode, Detail = status.Detail });
    }


    // from https://github.com/dotnet/aspnetcore/blob/8d0f798cc4de54a2851748be635a58eadbf79593/src/Grpc/JsonTranscoding/src/Microsoft.AspNetCore.Grpc.JsonTranscoding/Internal/JsonRequestHelpers.cs#L87
    static int MapStatusCodeToHttpStatus(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.OK:
                return StatusCodes.Status200OK;
            case StatusCode.Cancelled:
                return StatusCodes.Status408RequestTimeout;
            case StatusCode.Unknown:
                return StatusCodes.Status500InternalServerError;
            case StatusCode.InvalidArgument:
                return StatusCodes.Status400BadRequest;
            case StatusCode.DeadlineExceeded:
                return StatusCodes.Status504GatewayTimeout;
            case StatusCode.NotFound:
                return StatusCodes.Status404NotFound;
            case StatusCode.AlreadyExists:
                return StatusCodes.Status409Conflict;
            case StatusCode.PermissionDenied:
                return StatusCodes.Status403Forbidden;
            case StatusCode.Unauthenticated:
                return StatusCodes.Status401Unauthorized;
            case StatusCode.ResourceExhausted:
                return StatusCodes.Status429TooManyRequests;
            case StatusCode.FailedPrecondition:
                // Note, this deliberately doesn't translate to the similarly named '412 Precondition Failed' HTTP response status.
                return StatusCodes.Status400BadRequest;
            case StatusCode.Aborted:
                return StatusCodes.Status409Conflict;
            case StatusCode.OutOfRange:
                return StatusCodes.Status400BadRequest;
            case StatusCode.Unimplemented:
                return StatusCodes.Status501NotImplemented;
            case StatusCode.Internal:
                return StatusCodes.Status500InternalServerError;
            case StatusCode.Unavailable:
                return StatusCodes.Status503ServiceUnavailable;
            case StatusCode.DataLoss:
                return StatusCodes.Status500InternalServerError;
        }

        return StatusCodes.Status500InternalServerError;
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
        var routePath = $"{transcodingOptions.RoutePathPrefix.TrimEnd('/')}/{method.ServiceName}/{method.MethodName}";
        BindNotImplemented(routePath, MethodType.ClientStreaming, method.ServiceName, method.MethodName);
    }

    public void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method) where TRawRequest : class where TRawResponse : class
    {
        // Ignore (Currently, not supported)
        var routePath = $"{transcodingOptions.RoutePathPrefix.TrimEnd('/')}/{method.ServiceName}/{method.MethodName}";
        BindNotImplemented(routePath, MethodType.ServerStreaming, method.ServiceName, method.MethodName);
    }

    public void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method) where TRawRequest : class where TRawResponse : class
    {
        // Ignore (Currently, not supported)
        var routePath = $"{transcodingOptions.RoutePathPrefix.TrimEnd('/')}/{method.ServiceName}/{method.MethodName}";
        BindNotImplemented(routePath, MethodType.DuplexStreaming, method.ServiceName, method.MethodName);
    }

    public void BindStreamingHub(MagicOnionStreamingHubConnectMethod<TService> method)
    {
        // Ignore (Currently, not supported)
        var routePath = $"{transcodingOptions.RoutePathPrefix.TrimEnd('/')}/{method.ServiceName}/{method.MethodName}";
        BindNotImplemented(routePath, MethodType.DuplexStreaming, method.ServiceName, method.MethodName);
    }

    void BindNotImplemented(string routePath, MethodType methodType, string serviceName, string methodName)
    {
        var grpcMethod = new Method<object, object>(
            methodType,
            serviceName,
            methodName,
            new Marshaller<object>(_ => throw new NotSupportedException(), _ => throw new NotSupportedException()),
            new Marshaller<object>(_ => throw new NotSupportedException(), _ => throw new NotSupportedException())
        );
        context.AddMethod(grpcMethod, RoutePatternFactory.Parse(routePath), [], async (context) =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, new { Code = (int)StatusCode.Unimplemented, Detail = $"JsonTranscoding does not support {methodType}." });
        });
    }
}
