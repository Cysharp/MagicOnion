using Grpc.Core;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server.Diagnostics;

public class MagicOnionLogToLogger : IMagicOnionLogger
{
    readonly ILogger logger;

    public MagicOnionLogToLogger(ILogger<MagicOnionLogToLogger> logger)
    {
        this.logger = logger;
    }

    public void BeginBuildServiceDefinition()
    {
        logger.LogDebug(nameof(BeginBuildServiceDefinition));
    }

    public void EndBuildServiceDefinition(double elapsed)
    {
        logger.LogDebug($"{nameof(EndBuildServiceDefinition)} elapsed:{elapsed}");
    }

    public void BeginInvokeMethod(ServiceContext context, Type type)
    {
        logger.LogDebug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(context.MethodType)} method:{context.CallContext.Method}");
    }

    public void EndInvokeMethod(ServiceContext context, Type type, double elapsed, bool isErrorOrInterrupted)
    {
        var msg = isErrorOrInterrupted ? "error" : "";
        logger.LogDebug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} elapsed:{elapsed} {msg}");
    }

    public void WriteToStream(ServiceContext context, Type type)
    {
        logger.LogDebug($"{nameof(WriteToStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method}");
    }

    public void ReadFromStream(ServiceContext context, Type type, bool complete)
    {
        logger.LogDebug($"{nameof(ReadFromStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} complete:{complete}");
    }

    // enum.ToString is slow.
    string MethodTypeToString(MethodType type)
    {
        switch (type)
        {
            case MethodType.Unary:
                return "Unary";
            case MethodType.ClientStreaming:
                return "ClientStreaming";
            case MethodType.ServerStreaming:
                return "ServerStreaming";
            case MethodType.DuplexStreaming:
                return "DuplexStreaming";
            default:
                return ((int)type).ToString();
        }
    }

    public void BeginInvokeHubMethod(StreamingHubContext context, ReadOnlyMemory<byte> request, Type type)
    {
        logger.LogDebug($"{nameof(BeginInvokeHubMethod)} method:{context.Path} size:{request.Length}");

    }

    public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type? type, double elapsed, bool isErrorOrInterrupted)
    {
        var msg = isErrorOrInterrupted ? "error" : "";
        logger.LogDebug($"{nameof(EndInvokeHubMethod)} method:{context.Path} size:{responseSize} elapsed:{elapsed} {msg}");
    }

    public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
    {
        logger.LogDebug($"{nameof(InvokeHubBroadcast)} size:{responseSize} broadcastGroupCount:{broadcastGroupCount}");
    }

    public void Error(Exception ex, ServerCallContext context)
    {
        logger.LogError(ex, "MagicOnionHandler throws exception occurred in " + context.Method);
    }
    public void Error(Exception ex, StreamingHubContext context)
    {
        logger.LogError(ex, "Hub Method Handler throws exception occurred in " + context.Path);
    }
}
