using Grpc.Core;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Diagnostics;

public static partial class MagicOnionServerLog
{
    public static void BeginInvokeMethod(ILogger logger, ServiceContext context, Type type)
        => BeginInvokeMethod(logger, MethodTypeToString(context.MethodType), context.CallContext.Method);

    public static void EndInvokeMethod(ILogger logger, ServiceContext context, Type type, double elapsed, bool isErrorOrInterrupted)
        => EndInvokeMethod(logger, MethodTypeToString(context.MethodType), context.CallContext.Method, elapsed, (isErrorOrInterrupted ? "error" : ""));

    public static void WriteToStream(ILogger logger, ServiceContext context, Type type)
        => WriteToStream(logger, MethodTypeToString(context.MethodType), context.CallContext.Method);

    public static void ReadFromStream(ILogger logger, ServiceContext context, Type type, bool complete)
        => ReadFromStream(logger, MethodTypeToString(context.MethodType), context.CallContext.Method, complete);

    public static void BeginInvokeHubMethod(ILogger logger, StreamingHubContext context, ReadOnlyMemory<byte> request, Type type)
        => BeginInvokeHubMethod(logger, context.Path, request.Length);

    public static void EndInvokeHubMethod(ILogger logger, StreamingHubContext context, int responseSize, Type? type, double elapsed, bool isErrorOrInterrupted)
        => EndInvokeHubMethod(logger, context.Path, responseSize, elapsed, isErrorOrInterrupted ? "error" : "");

    public static void Error(ILogger logger, Exception ex, ServerCallContext context)
        => ErrorOnServiceMethod(logger, ex, context.Method);
    public static void Error(ILogger logger, Exception ex, StreamingHubContext context)
        => ErrorOnHubMethod(logger, ex, context.Path);

    // enum.ToString is slow.
    static string MethodTypeToString(MethodType type) =>
        type switch
        {
            MethodType.Unary => "Unary",
            MethodType.ClientStreaming => "ClientStreaming",
            MethodType.ServerStreaming => "ServerStreaming",
            MethodType.DuplexStreaming => "DuplexStreaming",
            _ => ((int)type).ToString(),
        };

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, EventName = nameof(BeginBuildServiceDefinition), Message = nameof(BeginBuildServiceDefinition))]
    public static partial void BeginBuildServiceDefinition(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, EventName = nameof(EndBuildServiceDefinition), Message = nameof(EndBuildServiceDefinition) +" elapsed:{elapsed}")]
    public static partial void EndBuildServiceDefinition(ILogger logger, double elapsed);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, EventName = nameof(BeginInvokeMethod), Message = nameof(BeginInvokeMethod) + " type:{methodType} method:{method}")]
    public static partial void BeginInvokeMethod(ILogger logger, string methodType, string method);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, EventName = nameof(EndInvokeMethod), Message = nameof(EndInvokeMethod) + " type:{methodType} method:{method} elapsed:{elapsed} {message}")]
    public static partial void EndInvokeMethod(ILogger logger, string methodType, string method, double elapsed, string message);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, EventName = nameof(WriteToStream), Message = nameof(WriteToStream) + " type:{methodType} method:{method}")]
    public static partial void WriteToStream(ILogger logger, string methodType, string method);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, EventName = nameof(ReadFromStream), Message = nameof(ReadFromStream) + " type:{methodType} method:{method} complete:{complete}")]
    public static partial void ReadFromStream(ILogger logger, string methodType, string method, bool complete);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, EventName = nameof(BeginInvokeHubMethod), Message = nameof(BeginInvokeHubMethod) + " method:{method} size:{size}")]
    public static partial void BeginInvokeHubMethod(ILogger logger, string method, int size);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, EventName = nameof(EndInvokeHubMethod), Message = nameof(EndInvokeHubMethod) + " method:{method} size:{size} elapsed:{elapsed} {message}")]
    public static partial void EndInvokeHubMethod(ILogger logger, string method, int size, double elapsed, string message);

    [LoggerMessage(EventId = 9, Level = LogLevel.Debug, EventName = nameof(InvokeHubBroadcast), Message = nameof(InvokeHubBroadcast) + " groupName:{groupName} size:{size} broadcastGroupCount:{broadcastGroupCount}")]
    public static partial void InvokeHubBroadcast(ILogger logger, string groupName, int size, int broadcastGroupCount);

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, EventName = nameof(BeginHeartbeatTimer), Message = nameof(BeginHeartbeatTimer) + " method:{method}, heartbeatInterval:{heartbeatInterval}, timeoutDuration:{timeoutDuration}")]
    public static partial void BeginHeartbeatTimer(ILogger logger, string method, TimeSpan heartbeatInterval, TimeSpan timeoutDuration);

    [LoggerMessage(EventId = 11, Level = LogLevel.Debug, EventName = nameof(ShutdownHeartbeatTimer), Message = nameof(ShutdownHeartbeatTimer) + " method:{method}")]
    public static partial void ShutdownHeartbeatTimer(ILogger logger, string method);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug, EventName = nameof(HeartbeatTimedOut), Message = nameof(HeartbeatTimedOut) + " method:{method}, connectionId:{connectionId}")]
    public static partial void HeartbeatTimedOut(ILogger logger, string method, Guid connectionId);

    [LoggerMessage(EventId = 13, Level = LogLevel.Debug, EventName = nameof(SendHeartbeat), Message = nameof(SendHeartbeat) + " method:{method}")]
    public static partial void SendHeartbeat(ILogger logger, string method);

    [LoggerMessage(EventId = 14, Level = LogLevel.Trace, EventName = nameof(AddStreamingHubMethod), Message = "Added StreamingHub method '{methodName}' to StreamingHub '{hubName}'. Method Id: {methodId}")]
    public static partial void AddStreamingHubMethod(ILogger logger, string hubName, string methodName, int methodId);

    [LoggerMessage(EventId = 15, Level = LogLevel.Trace, EventName = nameof(ServiceMethodDiscovered), Message = "Discovered gRPC and StreamingHub methods for '{serviceName}' by '{methodProviderName}'")]
    public static partial void ServiceMethodDiscovered(ILogger logger, string serviceName, string methodProviderName);

    [LoggerMessage(EventId = 16, Level = LogLevel.Trace, EventName = nameof(ServiceMethodNotDiscovered), Message = "Could not found gRPC and StreamingHub methods for '{serviceName}'")]
    public static partial void ServiceMethodNotDiscovered(ILogger logger, string serviceName);

    [LoggerMessage(EventId = 90, Level = LogLevel.Error, EventName = nameof(ErrorOnServiceMethod), Message = "A service handler throws an exception occurred in {method}")]
    public static partial void ErrorOnServiceMethod(ILogger logger, Exception ex, string method);

    [LoggerMessage(EventId = 91, Level = LogLevel.Error, EventName = nameof(ErrorOnHubMethod), Message = "A hub method handler throws an exception occurred in {path}")]
    public static partial void ErrorOnHubMethod(ILogger logger, Exception ex, string path);
}
