using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public interface IMagicOnionLogger
    {
        void BeginBuildServiceDefinition();
        void EndBuildServiceDefinition(double elapsed);

        void BeginInvokeMethod(MethodType type, string method);
        void EndInvokeMethod(MethodType type, string method, double elapsed, bool isErrorOrInterrupted);
    }

    public class NullMagicOnionLogger : IMagicOnionLogger
    {
        public void BeginBuildServiceDefinition()
        {
        }

        public void BeginInvokeMethod(MethodType type, string method)
        {
        }

        public void EndBuildServiceDefinition(double elapsed)
        {
        }

        public void EndInvokeMethod(MethodType type, string method, double elapsed, bool isErrorOrInterrupted)
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

        public void BeginInvokeMethod(MethodType type, string method)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(BeginInvokeMethod)} type:{MethodTypeToString(type)} method:{method}");
        }

        public void EndInvokeMethod(MethodType type, string method, double elapsed, bool isErrorOrInterrupted)
        {
            GrpcEnvironment.Logger.Debug($"{nameof(EndInvokeMethod)} type:{MethodTypeToString(type)}  method:{method} elapsed:{elapsed} isErrorOrInterrupted:{isErrorOrInterrupted}");
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