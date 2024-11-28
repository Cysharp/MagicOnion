using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

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
    public ConcurrentDictionary<string, object> Items => Services.GetRequiredKeyedService<ConcurrentDictionary<string, object>>(ItemsKey);
    public List<string> Logs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logger =>
        {
            logger.AddFakeLogging(options =>
            {
                options.OutputFormatter = x => $"{x.Timestamp}\t{x.Level}\t{x.Id}\t{x.Message}\t{x.Exception}";
                options.OutputSink = x => Logs.Add(x);
            });
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
        return this.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(services =>
            {
                services.Configure<MagicOnionOptions>(configure);
            });
        });
    }
}
