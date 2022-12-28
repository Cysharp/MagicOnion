using MagicOnion.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Redis.Tests;

#pragma warning disable CS1998
public class MagicOnionApplicationFactory<TServiceImplementation> : WebApplicationFactory<MagicOnionTestServer.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddMagicOnion(new[] { typeof(TServiceImplementation) });
        });
    }

    public WebApplicationFactory<MagicOnionTestServer.Program> WithMagicOnionOptions(Action<MagicOnionOptions> configure)
    {
        return this.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(services =>
            {
                services.Configure<MagicOnionOptions>(configure);
            });
        });
    }
}
