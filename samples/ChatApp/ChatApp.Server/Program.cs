using Grpc.Core;
using MagicOnion.Hosting;
using MagicOnion.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            // for SSL/TLS connection
            //var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            //var certificates = new List<KeyCertificatePair> { new KeyCertificatePair(File.ReadAllText("server.crt"), File.ReadAllText("server.key")) };
            //var credential = new SslServerCredentials(certificates);

            await MagicOnionHost.CreateDefaultBuilder()
                .UseMagicOnion(
                    new MagicOnionOptions(isReturnExceptionStackTraceInErrorDetail: true),
                    new ServerPort("localhost", 12345, ServerCredentials.Insecure))
                    // for SSL/TLS Connection
                    //new ServerPort(config.GetValue<string>("MAGICONION_HOST", "127.0.0.1"), 12345, credential))
                .RunConsoleAsync();
        }
    }
}
