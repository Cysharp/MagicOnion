using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;

namespace AuthSample;

public class Program
{
    public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

    // Do not change. This is the pattern our test infrastructure uses to initialize a IWebHostBuilder from
    // a users app.
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        new WebHostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .ConfigureLogging(logging =>
            {
                logging.AddDebug();
            })
            .UseKestrel()
            .UseIISIntegration();
}


public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc();
        services.AddMagicOnion(new [] { typeof(Startup).Assembly });

        services.AddAuthentication("Fake")
            .AddScheme<FakeAuthenticationHandlerOptions, FakeAuthenticationHandler>("Fake", options => { });

        services.AddAuthorization();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMagicOnionService();
        });
    }
}
