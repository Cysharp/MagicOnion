using Grpc.Core;
using MagicOnion.Server.Hubs;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using Microsoft.Extensions.Logging;

namespace MagicOnion.Server
{
    public interface IMagicOnionLogger
    {
        void BeginBuildServiceDefinition();
        void EndBuildServiceDefinition(double elapsed);

        void BeginInvokeMethod(ServiceContext context, Type type);
        void EndInvokeMethod(ServiceContext context, Type type, double elapsed, bool isErrorOrInterrupted);

        void BeginInvokeHubMethod(StreamingHubContext context, ReadOnlyMemory<byte> request, Type type);
        void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type? type, double elapsed, bool isErrorOrInterrupted);
        void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount);

        void WriteToStream(ServiceContext context, byte[] writeData, Type type);
        void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete);

        void Error(Exception ex, ServerCallContext context);
        void Error(Exception ex, StreamingHubContext context);
    }

    public class NullMagicOnionLogger : IMagicOnionLogger
    {
        public void BeginBuildServiceDefinition()
        {
        }

        public void BeginInvokeMethod(ServiceContext context, Type type)
        {
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
        }

        public void EndInvokeMethod(ServiceContext context, Type type, double elapsed, bool isErrorOrInterrupted)
        {
        }

        public void BeginInvokeHubMethod(StreamingHubContext context, ReadOnlyMemory<byte> request, Type type)
        {
        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type? type, double elapsed, bool isErrorOrInterrupted)
        {
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
        }

        public void Error(Exception ex, ServerCallContext context)
        {
        }
        public void Error(Exception ex, StreamingHubContext context)
        {
        }
    }

    public class MagicOnionLogToLogger : IMagicOnionLogger
    {
        readonly ILogger _logger;

        public MagicOnionLogToLogger(ILogger<MagicOnionLogToLogger> logger)
        {
            _logger = logger;
        }

        public void BeginBuildServiceDefinition()
        {
            _logger.LogDebug(nameof(BeginBuildServiceDefinition));
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            _logger.LogDebug($"{nameof(EndBuildServiceDefinition)} elapsed:{elapsed}");
        }

        public void BeginInvokeMethod(ServiceContext context, Type type)
        {
            _logger.LogDebug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(context.MethodType)} method:{context.CallContext.Method}");
        }

        public void EndInvokeMethod(ServiceContext context, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            _logger.LogDebug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} elapsed:{elapsed} {msg}");
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
            _logger.LogDebug($"{nameof(WriteToStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{writeData.Length}");
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
            _logger.LogDebug($"{nameof(ReadFromStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{readData.Length} complete:{complete}");
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
            _logger.LogDebug($"{nameof(BeginInvokeHubMethod)} method:{context.Path} size:{request.Length}");

        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type? type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            _logger.LogDebug($"{nameof(EndInvokeHubMethod)} method:{context.Path} size:{responseSize} elapsed:{elapsed} {msg}");
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
            _logger.LogDebug($"{nameof(InvokeHubBroadcast)} size:{responseSize} broadcastGroupCount:{broadcastGroupCount}");
        }

        public void Error(Exception ex, ServerCallContext context)
        {
            _logger.LogError(ex, "MagicOnionHandler throws exception occured in " + context.Method);
        }
        public void Error(Exception ex, StreamingHubContext context)
        {
            _logger.LogError(ex, "Hub Method Handler throws exception occured in " + context.Path);
        }
    }

    internal class ContractlessFirstStandardResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new ContractlessFirstStandardResolver();

        static readonly IFormatterResolver[] resolvers = new IFormatterResolver[]
        {
            BuiltinResolver.Instance, // Try Builtin
            
            DynamicEnumResolver.Instance, // Try Enum
            DynamicGenericResolver.Instance, // Try Array, Tuple, Collection
            DynamicUnionResolver.Instance, // Try Union(Interface)
            DynamicContractlessObjectResolver.Instance,
        };

        ContractlessFirstStandardResolver()
        {
        }

        public IMessagePackFormatter<T>? GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T>? formatter;

            static FormatterCache()
            {
                foreach (var item in resolvers)
                {
                    var f = item.GetFormatter<T>();
                    if (f != null)
                    {
                        formatter = f;
                        return;
                    }
                }
            }
        }
    }
}