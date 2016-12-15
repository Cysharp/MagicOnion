using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion.Server;
using System;

namespace MagicOnion.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server:::");

            //Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            //Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var service = MagicOnionEngine.BuildServerServiceDefinition(new MagicOnionOptions(true)
            {
                MagicOnionLogger = new MagicOnionLogToGrpcLogger()
            });

            var server = new global::Grpc.Core.Server
            {
                Services = { service },
                Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.ReadLine();
        }
    }
}