using System.Collections.Concurrent;
using MagicOnion.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Integration.Tests;

#pragma warning disable CS1998
public class MagicOnionApplicationFactory<TServiceImplementation> : WebApplicationFactory<MagicOnionTestServer.Program>
{
    public const string ItemsKey = "MagicOnionApplicationFactory.Items";
    public ConcurrentDictionary<string, object> Items => Services.GetRequiredKeyedService<ConcurrentDictionary<string, object>>(ItemsKey);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<ConcurrentDictionary<string, object>>(ItemsKey);
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
