using Grpc.Core;
using MagicOnion.Hosting;
using MagicOnion.Server;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            await new HostBuilder()
                .UseMagicOnion(new[] { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }, new MagicOnionOptions { IsReturnExceptionStackTraceInErrorDetail = true })
                .RunConsoleAsync();
        }
    }
}
