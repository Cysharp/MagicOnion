using System;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace ChatApp.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            // WORKAROUND:
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            options.ConfigureEndpointDefaults(endpointOptions =>
                            {
                                endpointOptions.Protocols = HttpProtocols.Http2;
                            });
                        })
                        .UseStartup<Startup>();
                });
    }
}
