using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.AspNetCore.Hosting;
using MagicOnion.Server;
using System;
using Microsoft.AspNetCore.Builder;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using MagicOnion.HttpGateway.Swagger;

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


            var webHost = new WebHostBuilder()
                .ConfigureServices(collection =>
                {
                    collection.Add(new ServiceDescriptor(typeof(MagicOnionServiceDefinition), service));
                })
                .UseWebListener()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5432")
                .Build();

            webHost.Run();
        }
    }

    // AspNet Startup
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var magicOnion = app.ApplicationServices.GetService<MagicOnionServiceDefinition>();

            var xmlName = "Sandbox.ConsoleServerDefinition.xml";
            var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

            app.UseMagicOnionSwagger(magicOnion.MethodHandlers, new SwaggerOptions("MagicOnion.Server", "Swagger Integration Test", "/")
            {
                XmlDocumentPath = xmlPath
            });
            app.UseMagicOnionHttpGateway(magicOnion.MethodHandlers, new Channel("localhost:12345", ChannelCredentials.Insecure));
        }
    }
}