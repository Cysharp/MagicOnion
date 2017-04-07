using Grpc.Core;
using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Server2:::");

            ////Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            ////Environment.SetEnvironmentVariable("GRPC_TRACE", "all");

            //Environment.SetEnvironmentVariable("SETTINGS_MAX_HEADER_LIST_SIZE", "1000000");

            //GrpcEnvironment.SetLogger(new ConsoleLogger());

            //var service = MagicOnionEngine.BuildServerServiceDefinition(new MagicOnionOptions(true)
            //{
            //    // MagicOnionLogger = new MagicOnionLogToGrpcLogger(),
            //    MagicOnionLogger = new MagicOnionLogToGrpcLoggerWithNamedDataDump(),
            //    GlobalFilters = new MagicOnionFilterAttribute[]
            //    {
            //    }
            //});

            //var server = new global::Grpc.Core.Server
            //{
            //    Services = { service },
            //    Ports = { new ServerPort(Configuration.GrpcHost, 12346, ServerCredentials.Insecure) }
            //};

            //server.Start();
        }
    }
}
