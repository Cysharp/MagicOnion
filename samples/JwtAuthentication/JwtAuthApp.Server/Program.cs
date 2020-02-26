using System;
using System.Threading.Tasks;
using JwtAuthApp.Server.Authentication;
using LitJWT;
using LitJWT.Algorithms;
using MagicOnion.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JwtAuthApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await MagicOnionHost.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMagicOnionJwtAuthentication<CustomJwtAuthenticationProvider>(options =>
                    {
                        var preSharedKey = Convert.FromBase64String(hostContext.Configuration.GetSection("JwtAuthApp.Server:Secret").Value);
                        var algorithm = new HS512Algorithm(preSharedKey); // Use Symmetric algorithm (HMAC SHA-512)
                        options.Encoder = new JwtEncoder(algorithm);
                        options.Decoder = new JwtDecoder(new JwtAlgorithmResolver(algorithm));
                        options.Expiry = TimeSpan.FromDays(7);
                    });
                })
                .UseMagicOnion()
                .RunConsoleAsync();
        }
    }
}
