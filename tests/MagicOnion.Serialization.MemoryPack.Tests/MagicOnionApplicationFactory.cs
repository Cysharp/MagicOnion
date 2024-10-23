using System.Collections.Concurrent;
using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Serialization.MemoryPack.Tests;

public abstract class MagicOnionApplicationFactory<TServiceImplementation> : MagicOnionApplicationFactory
{
    protected override IEnumerable<Type> GetServiceImplementationTypes() => [typeof(TServiceImplementation)];
}

public abstract class MagicOnionApplicationFactory : WebApplicationFactory<MagicOnionTestServer.Program>
{
    public const string ItemsKey = "MagicOnionApplicationFactory.Items";
    public ConcurrentDictionary<string, object> Items => Services.GetRequiredKeyedService<ConcurrentDictionary<string, object>>(ItemsKey);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<ConcurrentDictionary<string, object>>(ItemsKey);
            services.AddMagicOnion();
        });
        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService([.. GetServiceImplementationTypes()]);
            });
        });
    }

    protected abstract IEnumerable<Type> GetServiceImplementationTypes();

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
