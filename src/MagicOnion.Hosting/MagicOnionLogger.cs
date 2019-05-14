using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using System;

namespace MagicOnion.Hosting
{
    public class MagicOnionLogger : MagicOnion.Server.IMagicOnionLogger
    {
        ILogger logger;

        public MagicOnionLogger(ILoggerProvider provider)
        {
            logger = provider.CreateLogger("MagicOnion");
        }

        public void BeginBuildServiceDefinition()
        {
            logger.LogDebug(nameof(BeginBuildServiceDefinition));
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            logger.LogDebug($"{nameof(EndBuildServiceDefinition)} elapsed:{elapsed}");
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            logger.LogDebug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(context.MethodType)} method:{context.CallContext.Method} size:{request.Length}");
        }

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            logger.LogDebug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{response.Length} elapsed:{elapsed} {msg}");
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
            logger.LogDebug($"{nameof(WriteToStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{writeData.Length}");
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
            logger.LogDebug($"{nameof(ReadFromStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{readData.Length} complete:{complete}");
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

        public void BeginInvokeHubMethod(StreamingHubContext context, ArraySegment<byte> request, Type type)
        {
            logger.LogDebug($"{nameof(BeginInvokeHubMethod)} method:{context.Path} size:{request.Count}");

        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            logger.LogDebug($"{nameof(EndInvokeHubMethod)} method:{context.Path} size:{responseSize} elapsed:{elapsed} {msg}");
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
            logger.LogDebug($"{nameof(InvokeHubBroadcast)} size:{responseSize} broadcastGroupCount:{broadcastGroupCount}");
        }
    }
}
