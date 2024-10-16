namespace MagicOnionTestServer;

/// <summary>
/// This project exists to be used by test projects that uses WebApplicationFactory.
/// </summary>
/// <example>
/// <code>
/// public class MagicOnionApplicationFactory&lt;TServiceImplementation&gt; : WebApplicationFactory&lt;MagicOnionTestServer.Program&gt;
/// {
///     protected override void ConfigureWebHost(IWebHostBuilder builder)
///     {
///         builder.ConfigureServices(services =&gt;
///         {
///             services.AddMagicOnion(new[] { typeof(TServiceImplementation) });
///         });
///     }
/// }
/// 
/// public class MyTest : IClassFixture&lt;MagicOnionApplicationFactory&lt;MyTestService>>
/// {
///     [Fact]
///     public void TestCase1()
///     {
///         var httpClient = factory.CreateDefaultClient();
///         var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
///         // ...
///     }
/// }
/// </code>
/// </example>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //builder.Services.AddGrpc();

        var app = builder.Build();

        //app.MapMagicOnionService();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}
