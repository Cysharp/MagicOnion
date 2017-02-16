using System;
using Grpc.Core;

namespace MagicOnion.Server
{
    public interface IMagicOnionLogger
    {
        void BeginBuildServiceDefinition();
        void EndBuildServiceDefinition(double elapsed);

        void BeginInvokeMethod(ServiceContext context);
        void EndInvokeMethod(ServiceContext context, double elapsed, bool isErrorOrInterrupted);

        void WriteToStream(ServiceContext context);
        void ReadFromStream(ServiceContext context, bool complete);
    }

    public class NullMagicOnionLogger : IMagicOnionLogger
    {
        public void BeginBuildServiceDefinition()
        {
        }

        public void BeginInvokeMethod( ServiceContext context)
        {
        }

        public void ReadFromStream( ServiceContext context, bool complete)
        {
        }

        public void WriteToStream( ServiceContext context)
        {
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
        }

        public void EndInvokeMethod(ServiceContext context, double elapsed, bool isErrorOrInterrupted)
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

        public void BeginInvokeMethod(ServiceContext context)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(context.MethodType)} method:{context.CallContext.Method}");
        }

        public void EndInvokeMethod(ServiceContext context, double elapsed, bool isErrorOrInterrupted)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} elapsed:{elapsed} isErrorOrInterrupted:{isErrorOrInterrupted}");
        }

        public void WriteToStream( ServiceContext context)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(WriteToStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method}");
        }

        public void ReadFromStream( ServiceContext context, bool complete)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(ReadFromStream)} type:{MethodTypeToString(context.MethodType)}  method:{context.CallContext.Method} complete:{complete}");
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
    }
}