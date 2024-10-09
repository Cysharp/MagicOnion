using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Hubs.Internal;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Binder.Internal;

internal class MagicOnionGrpcMethodBinder<TService> : IMagicOnionGrpcMethodBinder<TService>
    where TService : class
{
    readonly ServiceMethodProviderContext<TService> providerContext;
    readonly IMagicOnionSerializerProvider messageSerializerProvider;
    readonly IList<MagicOnionServiceFilterDescriptor> globalFilters;
    readonly IServiceProvider serviceProvider;
    readonly ILogger logger;

    readonly bool enableCurrentContext;
    readonly bool isReturnExceptionStackTraceInErrorDetail;

    public MagicOnionGrpcMethodBinder(ServiceMethodProviderContext<TService> context, MagicOnionOptions options, IServiceProvider serviceProvider, ILogger<MagicOnionGrpcMethodBinder<TService>> logger)
    {
        this.providerContext = context;
        this.messageSerializerProvider = options.MessageSerializer;
        this.globalFilters = options.GlobalFilters;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.enableCurrentContext = options.EnableCurrentContext;
        this.isReturnExceptionStackTraceInErrorDetail = options.IsReturnExceptionStackTraceInErrorDetail;
    }

    public void BindUnary<TRequest, TResponse, TRawRequest, TRawResponse>(IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.Unary, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.Unary, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method.MethodInfo);

        providerContext.AddUnaryMethod(grpcMethod, attrs, BuildUnaryMethodPipeline(method, messageSerializer, attrs));
    }

    public void BindClientStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.ClientStreaming, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ClientStreaming, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method.MethodInfo);

        providerContext.AddClientStreamingMethod(grpcMethod, attrs, BuildClientStreamingMethodPipeline(method, messageSerializer, attrs));
    }

    public void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.ServerStreaming, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.ServerStreaming, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method.MethodInfo);

        providerContext.AddServerStreamingMethod<TRawRequest, TRawResponse>(grpcMethod, attrs, BuildServerStreamingMethodPipeline(method, messageSerializer, attrs));
    }

    public void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.DuplexStreaming, default);
        var grpcMethod = GrpcMethodHelper.CreateMethod<TRequest, TResponse, TRawRequest, TRawResponse>(MethodType.DuplexStreaming, method.ServiceName, method.MethodName, messageSerializer);
        var attrs = GetMetadataFromHandler(method.MethodInfo);

        providerContext.AddDuplexStreamingMethod(grpcMethod, attrs, BuildDuplexStreamingMethodPipeline(method, messageSerializer, attrs));
    }

    public void BindStreamingHub(MagicOnionStreamingHubConnectMethod<TService> method)
    {
        var messageSerializer = messageSerializerProvider.Create(MethodType.DuplexStreaming, default);
        // StreamingHub uses the special marshallers for streaming messages serialization.
        // TODO: Currently, MagicOnion expects TRawRequest/TRawResponse to be raw-byte array (`StreamingHubPayload`).
        var grpcMethod = new Method<StreamingHubPayload, StreamingHubPayload>(
            MethodType.DuplexStreaming,
            method.ServiceName,
            method.MethodName,
            MagicOnionMarshallers.StreamingHubMarshaller,
            MagicOnionMarshallers.StreamingHubMarshaller
        );
        var attrs = GetMetadataFromHandler(method.MethodInfo);

        var duplexMethod = new MagicOnionDuplexStreamingMethod<TService, StreamingHubPayload, StreamingHubPayload, StreamingHubPayload, StreamingHubPayload>(
            method.ServiceName,
            method.MethodName,
            static (instance, context) =>
            {
                context.CallContext.GetHttpContext().Features.Set<IStreamingHubFeature>(context.ServiceProvider.GetRequiredService<StreamingHubRegistry<TService>>());
                return ((IStreamingHubBase)instance).Connect();
            });
        providerContext.AddDuplexStreamingMethod(grpcMethod, attrs, BuildDuplexStreamingMethodPipeline(duplexMethod, messageSerializer, attrs));
    }

    IList<object> GetMetadataFromHandler(MethodInfo methodInfo)
    {
        // NOTE: We need to collect Attributes for Endpoint metadata. ([Authorize], [AllowAnonymous] ...)
        // https://github.com/grpc/grpc-dotnet/blob/7ef184f3c4cd62fbc3cde55e4bb3e16b58258ca1/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs#L89-L98
        var metadata = new List<object>();
        metadata.AddRange(methodInfo.DeclaringType!.GetCustomAttributes(inherit: true));
        metadata.AddRange(methodInfo.GetCustomAttributes(inherit: true));

        metadata.Add(new HttpMethodMetadata(["POST"], acceptCorsPreflight: true));
        return metadata;
    }

    void InitializeServiceProperties(object instance, ServiceContext serviceContext)
    {
        var service = ((IServiceBase)instance);
        service.Context = serviceContext;
        service.Metrics = serviceProvider.GetRequiredService<MagicOnionMetrics>();
    }

    ClientStreamingServerMethod<TService, TRawRequest, TRawResponse> BuildClientStreamingMethodPipeline<TRequest, TResponse, TRawRequest, TRawResponse>(
        MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var attributeLookup = metadata.OfType<Attribute>().ToLookup(k => k.GetType());
        var filters = FilterHelper.GetFilters(globalFilters, typeof(TService), method.MethodInfo);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, serviceContext));

        return InvokeAsync;

        async Task<TRawResponse> InvokeAsync(TService instance, IAsyncStreamReader<TRawRequest> rawRequestStream, ServerCallContext context)
        {
            var isCompletedSuccessfully = false;
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();

            var requestStream = new MagicOnionAsyncStreamReader<TRequest, TRawRequest>(rawRequestStream);
            var serviceContext = new StreamingServiceContext<TRequest, Nil /* Dummy */>(
                instance,
                typeof(TService),
                method.ServiceName,
                method.MethodInfo,
                attributeLookup,
                MethodType.ClientStreaming,
                context,
                messageSerializer,
                logger,
                default!,
                context.GetHttpContext().RequestServices,
                requestStream,
                default
            );

            InitializeServiceProperties(instance, serviceContext);

            TResponse response;
            try
            {
                using (rawRequestStream as IDisposable)
                {
                    MagicOnionServerLog.BeginInvokeMethod(logger, serviceContext, typeof(Nil));
                    if (enableCurrentContext)
                    {
                        ServiceContext.currentServiceContext.Value = serviceContext;
                    }
                    await wrappedBody(serviceContext);
                    response = serviceContext.Result is TResponse r ? r : default!;
                    isCompletedSuccessfully = true;
                }
            }
            catch (ReturnStatusException ex)
            {
                context.Status = ex.ToStatus();
                response = default!;
            }
            catch (Exception ex)
            {
                if (TryResolveStatus(ex, out var status))
                {
                    context.Status = status.Value;
                    MagicOnionServerLog.Error(logger, ex, context);
                    response = default!;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                MagicOnionServerLog.EndInvokeMethod(logger, serviceContext, typeof(TResponse), TimeProvider.System.GetElapsedTime(requestBeginTimestamp).TotalMilliseconds, !isCompletedSuccessfully);
            }

            return GrpcMethodHelper.ToRaw<TResponse, TRawResponse>(response);
        }
    }

    ServerStreamingServerMethod<TService, TRawRequest, TRawResponse> BuildServerStreamingMethodPipeline<TRequest, TResponse, TRawRequest, TRawResponse>(
        MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var attributeLookup = metadata.OfType<Attribute>().ToLookup(k => k.GetType());
        var filters = FilterHelper.GetFilters(globalFilters, typeof(TService), method.MethodInfo);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, (TRequest)serviceContext.Request!, serviceContext));

        return InvokeAsync;

        async Task InvokeAsync(TService instance, TRawRequest rawRequest, IServerStreamWriter<TRawResponse> rawResponseStream, ServerCallContext context)
        {
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();
            var isCompletedSuccessfully = false;

            var request = GrpcMethodHelper.FromRaw<TRawRequest, TRequest>(rawRequest);
            var responseStream = new MagicOnionServerStreamWriter<TResponse, TRawResponse>(rawResponseStream);
            var serviceContext = new StreamingServiceContext<TRequest, TResponse>(
                instance,
                typeof(TService),
                method.ServiceName,
                method.MethodInfo,
                attributeLookup,
                MethodType.ServerStreaming,
                context,
                messageSerializer,
                logger,
                default!,
                context.GetHttpContext().RequestServices,
                default,
                responseStream
            );

            serviceContext.SetRawRequest(request);

            InitializeServiceProperties(instance, serviceContext);

            try
            {
                MagicOnionServerLog.BeginInvokeMethod(logger, serviceContext, typeof(Nil));
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }
                await wrappedBody(serviceContext);
                isCompletedSuccessfully = true;
            }
            catch (ReturnStatusException ex)
            {
                context.Status = ex.ToStatus();
            }
            catch (Exception ex)
            {
                if (TryResolveStatus(ex, out var status))
                {
                    context.Status = status.Value;
                    MagicOnionServerLog.Error(logger, ex, context);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                MagicOnionServerLog.EndInvokeMethod(logger, serviceContext, typeof(TResponse), TimeProvider.System.GetElapsedTime(requestBeginTimestamp).TotalMilliseconds, !isCompletedSuccessfully);
            }
        }
    }

    DuplexStreamingServerMethod<TService, TRawRequest, TRawResponse> BuildDuplexStreamingMethodPipeline<TRequest, TResponse, TRawRequest, TRawResponse>(
        MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var attributeLookup = metadata.OfType<Attribute>().ToLookup(k => k.GetType());
        var filters = FilterHelper.GetFilters(globalFilters, typeof(TService), method.MethodInfo);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, serviceContext));

        return InvokeAsync;

        async Task InvokeAsync(TService instance, IAsyncStreamReader<TRawRequest> rawRequestStream, IServerStreamWriter<TRawResponse> rawResponseStream, ServerCallContext context)
        {
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();
            var isCompletedSuccessfully = false;

            var requestStream = new MagicOnionAsyncStreamReader<TRequest, TRawRequest>(rawRequestStream);
            var responseStream = new MagicOnionServerStreamWriter<TResponse, TRawResponse>(rawResponseStream);
            var serviceContext = new StreamingServiceContext<TRequest, TResponse>(
                instance,
                typeof(TService),
                method.ServiceName,
                method.MethodInfo,
                attributeLookup,
                MethodType.DuplexStreaming,
                context,
                messageSerializer,
                logger,
                default!,
                context.GetHttpContext().RequestServices,
                requestStream,
                responseStream
            );

            InitializeServiceProperties(instance, serviceContext);

            try
            {
                MagicOnionServerLog.BeginInvokeMethod(logger, serviceContext, typeof(Nil));
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }

                using (rawRequestStream as IDisposable)
                {
                    await wrappedBody(serviceContext);
                }

                isCompletedSuccessfully = true;
            }
            catch (ReturnStatusException ex)
            {
                context.Status = ex.ToStatus();
            }
            catch (Exception ex)
            {
                if (TryResolveStatus(ex, out var status))
                {
                    context.Status = status.Value;
                    MagicOnionServerLog.Error(logger, ex, context);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                MagicOnionServerLog.EndInvokeMethod(logger, serviceContext, typeof(TResponse), TimeProvider.System.GetElapsedTime(requestBeginTimestamp).TotalMilliseconds, !isCompletedSuccessfully);
            }
        }
    }

    UnaryServerMethod<TService, TRawRequest, TRawResponse> BuildUnaryMethodPipeline<TRequest, TResponse, TRawRequest, TRawResponse>(
        IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var attributeLookup = metadata.OfType<Attribute>().ToLookup(k => k.GetType());
        var filters = FilterHelper.GetFilters(globalFilters, typeof(TService), method.MethodInfo);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, (TRequest)serviceContext.Request!, serviceContext));

        return InvokeAsync;

        async Task<TRawResponse> InvokeAsync(TService instance, TRawRequest requestRaw, ServerCallContext context)
        {
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();
            var isCompletedSuccessfully = false;

            var serviceContext = new ServiceContext(instance, typeof(TService), method.ServiceName, method.MethodInfo, attributeLookup, MethodType.Unary, context, messageSerializer, logger, default!, context.GetHttpContext().RequestServices);
            var request = GrpcMethodHelper.FromRaw<TRawRequest, TRequest>(requestRaw);

            serviceContext.SetRawRequest(request);

            InitializeServiceProperties(instance, serviceContext);

            TResponse response = default!;
            try
            {
                MagicOnionServerLog.BeginInvokeMethod(logger, serviceContext, typeof(TRequest));

                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }

                await wrappedBody(serviceContext);

                isCompletedSuccessfully = true;

                if (serviceContext.Result is not null)
                {
                    response = (TResponse)serviceContext.Result;
                }

                if (response is RawBytesBox rawBytesResponse)
                {
                    return Unsafe.As<RawBytesBox, TRawResponse>(ref rawBytesResponse); // NOTE: To disguise an object as a `TRawResponse`, `TRawResponse` must be `class`.
                }
            }
            catch (ReturnStatusException ex)
            {
                context.Status = ex.ToStatus();
                response = default!;

                // WORKAROUND: Grpc.AspNetCore.Server throws a `Cancelled` status exception when it receives `null` response.
                //             To return the status code correctly, we need to rethrow the exception here.
                //             https://github.com/grpc/grpc-dotnet/blob/d4ee8babcd90666fc0727163a06527ab9fd7366a/src/Grpc.AspNetCore.Server/Internal/CallHandlers/UnaryServerCallHandler.cs#L50-L56
                var rpcException = new RpcException(ex.ToStatus());
                if (ex.StackTrace is not null)
                {
                    ExceptionDispatchInfo.SetRemoteStackTrace(rpcException, ex.StackTrace);
                }
                throw rpcException;
            }
            catch (Exception ex)
            {
                if (TryResolveStatus(ex, out var status))
                {
                    context.Status = status.Value;
                    MagicOnionServerLog.Error(logger, ex, context);
                    response = default!;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                MagicOnionServerLog.EndInvokeMethod(logger, serviceContext, typeof(TResponse), TimeProvider.System.GetElapsedTime(requestBeginTimestamp).TotalMilliseconds, !isCompletedSuccessfully);
            }

            return GrpcMethodHelper.ToRaw<TResponse, TRawResponse>(response);
        }
    }

    bool TryResolveStatus(Exception ex, [NotNullWhen(true)] out Status? status)
    {
        if (isReturnExceptionStackTraceInErrorDetail)
        {
            // Trim data.
            var msg = ex.ToString();
            var lineSplit = msg.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < lineSplit.Length; i++)
            {
                if (!(lineSplit[i].Contains("System.Runtime.CompilerServices")
                      || lineSplit[i].Contains("直前に例外がスローされた場所からのスタック トレースの終わり")
                      || lineSplit[i].Contains("End of stack trace from the previous location where the exception was thrown")
                    ))
                {
                    sb.AppendLine(lineSplit[i]);
                }
                if (sb.Length >= 5000)
                {
                    sb.AppendLine("----Omit Message(message size is too long)----");
                    break;
                }
            }
            var str = sb.ToString();

            status = new Status(StatusCode.Unknown, str);
            return true;
        }

        status = default;
        return false;
    }
}
