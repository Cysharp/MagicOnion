using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging.Testing;

namespace MagicOnion.Server.InternalTesting;

#pragma warning disable CS1998
public class MagicOnionApplicationFactory<TServiceImplementation> : MagicOnionApplicationFactory
{
    protected override IEnumerable<Type> GetServiceImplementationTypes()
    {
        yield return typeof(TServiceImplementation);
    }
}

public abstract class MagicOnionApplicationFactory : WebApplicationFactory<Program>
{
    public const string ItemsKey = "MagicOnionApplicationFactory.Items";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logger =>
        {
            logger.AddFakeLogging();
        });
        builder.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<ConcurrentDictionary<string, object>>(ItemsKey);
            OnConfigureMagicOnionBuilder(services.AddMagicOnion());
        });
        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService([..GetServiceImplementationTypes()]);
            });
        });
    }

    protected virtual void OnConfigureMagicOnionBuilder(IMagicOnionServerBuilder builder){}

    protected abstract IEnumerable<Type> GetServiceImplementationTypes();

    public WebApplicationFactory<Program> WithMagicOnionOptions(Action<MagicOnionOptions> configure)
    {
        return WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<MagicOnionOptions>(configure);
            });
        });
    }
}


public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationFactory<Program> @this)
    {
        public ConcurrentDictionary<string, object> Items => @this.Services.GetRequiredKeyedService<ConcurrentDictionary<string, object>>(MagicOnionApplicationFactory.ItemsKey);
        public FakeLogCollector Logs => @this.Services.GetRequiredService<FakeLogCollector>();

        public void Initialize()
        {
            @this.Logs.Clear();
            @this.Items.Clear();
        }
    }
}
