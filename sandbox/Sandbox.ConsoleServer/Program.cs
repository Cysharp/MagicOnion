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

            var ttt = typeof(Sandbox.ConsoleServer.Services.MyFirstService);
            var interfaces = ttt.GetInterfaces();

            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

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