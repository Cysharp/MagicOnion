using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Internal;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace MagicOnion.Server.Binder.Internal;

internal class MagicOnionGrpcMethodHandler<TService> where TService : class
{
    readonly IList<MagicOnionServiceFilterDescriptor> globalFilters;
    readonly IServiceProvider serviceProvider;
    readonly ILogger logger;

    readonly bool enableCurrentContext;
    readonly bool isReturnExceptionStackTraceInErrorDetail;

    public MagicOnionGrpcMethodHandler(bool enableCurrentContext, bool isReturnExceptionStackTraceInErrorDetail, IServiceProvider serviceProvider, IList<MagicOnionServiceFilterDescriptor> globalFilters, ILogger<MagicOnionGrpcMethodHandler<TService>> logger)
    {
        this.enableCurrentContext = enableCurrentContext;
        this.isReturnExceptionStackTraceInErrorDetail = isReturnExceptionStackTraceInErrorDetail;
        this.serviceProvider = serviceProvider;
        this.globalFilters = globalFilters;
        this.logger = logger;
    }

    void InitializeServiceProperties(object instance, ServiceContext serviceContext)
    {
        var service = ((IServiceBase)instance);
        service.Context = serviceContext;
        service.Metrics = serviceProvider.GetRequiredService<MagicOnionMetrics>();
    }

    public ClientStreamingServerMethod<TService, TRawRequest, TRawResponse> BuildClientStreamingMethod<TRequest, TResponse, TRawRequest, TRawResponse>(
        MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var attributeLookup = metadata.OfType<Attribute>().ToLookup(k => k.GetType());
        var filters = FilterHelper.GetFilters(globalFilters, method.Metadata.Attributes);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, serviceContext));

        return InvokeAsync;

        async Task<TRawResponse> InvokeAsync(TService instance, IAsyncStreamReader<TRawRequest> rawRequestStream, ServerCallContext context)
        {
            var isCompletedSuccessfully = false;
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();

            var requestServiceProvider = context.GetHttpContext().RequestServices;
            var metrics = requestServiceProvider.GetRequiredService<MagicOnionMetrics>();
            var requestStream = new MagicOnionAsyncStreamReader<TRequest, TRawRequest>(rawRequestStream);
            var serviceContext = new StreamingServiceContext<TRequest, Nil /* Dummy */>(
                instance,
                method,
                context,
                messageSerializer,
                metrics,
                logger,
                requestServiceProvider,
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

    public ServerStreamingServerMethod<TService, TRawRequest, TRawResponse> BuildServerStreamingMethod<TRequest, TResponse, TRawRequest, TRawResponse>(
        MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var attributeLookup = metadata.OfType<Attribute>().ToLookup(k => k.GetType());
        var filters = FilterHelper.GetFilters(globalFilters, method.Metadata.Attributes);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, serviceContext, (TRequest)serviceContext.Request!));

        return InvokeAsync;

        async Task InvokeAsync(TService instance, TRawRequest rawRequest, IServerStreamWriter<TRawResponse> rawResponseStream, ServerCallContext context)
        {
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();
            var isCompletedSuccessfully = false;

            var requestServiceProvider = context.GetHttpContext().RequestServices;
            var metrics = requestServiceProvider.GetRequiredService<MagicOnionMetrics>();
            var request = GrpcMethodHelper.FromRaw<TRawRequest, TRequest>(rawRequest);
            var responseStream = new MagicOnionServerStreamWriter<TResponse, TRawResponse>(rawResponseStream);
            var serviceContext = new StreamingServiceContext<Nil /* Dummy */, TResponse>(
                instance,
                method,
                context,
                messageSerializer,
                metrics,
                logger,
                requestServiceProvider,
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

    public DuplexStreamingServerMethod<TService, TRawRequest, TRawResponse> BuildDuplexStreamingMethod<TRequest, TResponse, TRawRequest, TRawResponse>(
        MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var attributeLookup = metadata.OfType<Attribute>().ToLookup(k => k.GetType());
        var filters = FilterHelper.GetFilters(globalFilters, method.Metadata.Attributes);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, serviceContext));

        return InvokeAsync;

        async Task InvokeAsync(TService instance, IAsyncStreamReader<TRawRequest> rawRequestStream, IServerStreamWriter<TRawResponse> rawResponseStream, ServerCallContext context)
        {
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();
            var isCompletedSuccessfully = false;

            var requestServiceProvider = context.GetHttpContext().RequestServices;
            var metrics = requestServiceProvider.GetRequiredService<MagicOnionMetrics>();
            var requestStream = new MagicOnionAsyncStreamReader<TRequest, TRawRequest>(rawRequestStream);
            var responseStream = new MagicOnionServerStreamWriter<TResponse, TRawResponse>(rawResponseStream);
            var serviceContext = new StreamingServiceContext<TRequest, TResponse>(
                instance,
                method,
                context,
                messageSerializer,
                metrics,
                logger,
                requestServiceProvider,
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

    public UnaryServerMethod<TService, TRawRequest, TRawResponse> BuildUnaryMethod<TRequest, TResponse, TRawRequest, TRawResponse>(
        IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method,
        IMagicOnionSerializer messageSerializer,
        IList<object> metadata
    )
        where TRawRequest : class
        where TRawResponse : class
    {
        var filters = FilterHelper.GetFilters(globalFilters, method.Metadata.Attributes);
        var wrappedBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (serviceContext) => method.InvokeAsync((TService)serviceContext.Instance, serviceContext, (TRequest)serviceContext.Request!));

        return InvokeAsync;

        async Task<TRawResponse> InvokeAsync(TService instance, TRawRequest requestRaw, ServerCallContext context)
        {
            var requestBeginTimestamp = TimeProvider.System.GetTimestamp();
            var isCompletedSuccessfully = false;

            var requestServiceProvider = context.GetHttpContext().RequestServices;
            var metrics = requestServiceProvider.GetRequiredService<MagicOnionMetrics>();
            var serviceContext = new ServiceContext(instance, method, context, messageSerializer, metrics, logger, requestServiceProvider);
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

                if (serviceContext.Result is {} result)
                {
                    if (result is RawBytesBox rawBytesResponse)
                    {
                        return Unsafe.As<RawBytesBox, TRawResponse>(ref rawBytesResponse); // NOTE: To disguise an object as a `TRawResponse`, `TRawResponse` must be `class`.
                    }

                    response = (TResponse)result;
                }
            }
            catch (ReturnStatusException ex)
            {
                context.Status = ex.ToStatus();

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
