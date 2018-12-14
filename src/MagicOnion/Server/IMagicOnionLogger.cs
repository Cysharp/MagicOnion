using Grpc.Core;
using MagicOnion.Server.Hubs;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;

namespace MagicOnion.Server
{
    public interface IMagicOnionLogger
    {
        void BeginBuildServiceDefinition();
        void EndBuildServiceDefinition(double elapsed);

        void BeginInvokeMethod(ServiceContext context, byte[] request, Type type);
        void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted);

        void BeginInvokeHubMethod(StreamingHubContext context, ArraySegment<byte> request, Type type);
        void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted);
        void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount);

        void WriteToStream(ServiceContext context, byte[] writeData, Type type);
        void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete);
    }

    public class NullMagicOnionLogger : IMagicOnionLogger
    {
        public void BeginBuildServiceDefinition()
        {
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
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

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
        }

        public void BeginInvokeHubMethod(StreamingHubContext context, ArraySegment<byte> request, Type type)
        {
        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
        }
    }

    public class MagicOnionLogToGrpcLogger : IMagicOnionLogger
    {
        public MagicOnionLogToGrpcLogger()
        {

        }

        public void BeginBuildServiceDefinition()
        {
            GrpcEnvironment.Logger.Debug(nameof(BeginBuildServiceDefinition));
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(EndBuildServiceDefinition)} elapsed:{elapsed}");
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(context.MethodType)} method:{context.CallContext.Method} size:{request.Length}");
        }

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{response.Length} elapsed:{elapsed} {msg}");
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(WriteToStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{writeData.Length}");
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
            if (context.ServiceType == typeof(EmbeddedServices.MagicOnionEmbeddedHeartbeat)) return;

            GrpcEnvironment.Logger.Debug($"{nameof(ReadFromStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{readData.Length} complete:{complete}");
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
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeHubMethod)} method:{context.Path} size:{request.Count}");

        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeHubMethod)} method:{context.Path} size:{responseSize} elapsed:{elapsed} {msg}");
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(InvokeHubBroadcast)} size:{responseSize} broadcastGroupCount:{broadcastGroupCount}");

        }
    }

    /// <summary>
    /// Data dump is slightly heavy, recommended to only use debugging.
    /// </summary>
    public class MagicOnionLogToGrpcLoggerWithDataDump : IMagicOnionLogger
    {
        public MagicOnionLogToGrpcLoggerWithDataDump()
        {

        }

        public void BeginBuildServiceDefinition()
        {
            GrpcEnvironment.Logger.Debug(nameof(BeginBuildServiceDefinition));
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(EndBuildServiceDefinition)} elapsed:{elapsed}");
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(context.MethodType)} method:{context.CallContext.Method} size:{request.Length} {ToJson(request)}");
        }

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{response.Length} elapsed:{elapsed} {msg} {ToJson(response)}");
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(WriteToStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{writeData.Length} {ToJson(writeData)}");
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
            if (context.ServiceType == typeof(EmbeddedServices.MagicOnionEmbeddedHeartbeat)) return;

            GrpcEnvironment.Logger.Debug($"{nameof(ReadFromStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{readData.Length} complete:{complete} {ToJson(readData)}");
        }

        string ToJson(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            if (bytes.Length >= 5000) return "log is too large.";

            return "dump:" + LZ4MessagePackSerializer.ToJson(bytes);
        }

        string ToJson(ArraySegment<byte> bytes)
        {
            if (bytes == null || bytes.Count == 0) return "";
            if (bytes.Count >= 5000) return "log is too large.";

            return "dump:" + LZ4MessagePackSerializer.ToJson(bytes);
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
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeHubMethod)} method:{context.Path} size:{request.Count} {ToJson(request)}");
        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeHubMethod)} method:{context.Path} size:{responseSize} elapsed:{elapsed} {msg}");
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(InvokeHubBroadcast)} size:{responseSize} broadcastGroupCount:{broadcastGroupCount}");
        }
    }

    /// <summary>
    /// Named data dump is most heavy, recommended to only use debugging.
    /// </summary>
    public class MagicOnionLogToGrpcLoggerWithNamedDataDump : IMagicOnionLogger
    {
        readonly IFormatterResolver dumpResolver;

        public MagicOnionLogToGrpcLoggerWithNamedDataDump()
            : this(ContractlessFirstStandardResolver.Instance)
        {

        }

        public MagicOnionLogToGrpcLoggerWithNamedDataDump(IFormatterResolver dumpResolver)
        {
            this.dumpResolver = dumpResolver;
        }

        public void BeginBuildServiceDefinition()
        {
            GrpcEnvironment.Logger.Debug(nameof(BeginBuildServiceDefinition));
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(EndBuildServiceDefinition)} elapsed:{elapsed}");
        }

        public void BeginInvokeMethod(ServiceContext context, byte[] request, Type type)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(context.MethodType)} method:{context.CallContext.Method} size:{request.Length} {ToJson(request, type, context)}");
        }

        public void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{response.Length} elapsed:{elapsed} {msg} {ToJson(response, type, context)}");
        }

        public void WriteToStream(ServiceContext context, byte[] writeData, Type type)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(WriteToStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{writeData.Length} {ToJson(writeData, type, context)}");
        }

        public void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete)
        {
            if (context.ServiceType == typeof(EmbeddedServices.MagicOnionEmbeddedHeartbeat)) return;

            GrpcEnvironment.Logger.Debug($"{nameof(ReadFromStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} size:{readData.Length} complete:{complete} {ToJson(readData, type, context)}");
        }

        string ToJson(byte[] bytes, Type type, ServiceContext context)
        {
            if (bytes == null || bytes.Length == 0) return "";
            if (bytes.Length >= 5000) return "log is too large.";

            var reData = LZ4MessagePackSerializer.NonGeneric.Deserialize(type, bytes, context.FormatterResolver);
            var reSerialized = MessagePackSerializer.NonGeneric.Serialize(type, reData, dumpResolver);

            return "dump:" + MessagePackSerializer.ToJson(reSerialized);
        }

        string ToJson(ArraySegment<byte> bytes, Type type, IFormatterResolver resolver)
        {
            if (bytes == null || bytes.Count == 0) return "";
            if (bytes.Count >= 5000) return "log is too large.";

            var reData = LZ4MessagePackSerializer.NonGeneric.Deserialize(type, bytes, resolver);
            var reSerialized = MessagePackSerializer.NonGeneric.Serialize(type, reData, dumpResolver);

            return "dump:" + MessagePackSerializer.ToJson(reSerialized);
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
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeHubMethod)} method:{context.Path} size:{request.Count} {ToJson(request, type, context.FormatterResolver)}");
        }

        public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted)
        {
            var msg = isErrorOrInterrupted ? "error" : "";
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeHubMethod)} method:{context.Path} size:{responseSize} elapsed:{elapsed} {msg}");
        }

        public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(InvokeHubBroadcast)} size:{responseSize} broadcastGroupCount:{broadcastGroupCount}");
        }
    }

    internal class ContractlessFirstStandardResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new ContractlessFirstStandardResolver();

        static readonly IFormatterResolver[] resolvers = new[]
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

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

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