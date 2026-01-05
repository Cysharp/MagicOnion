using AotSample.Server;
using AotSample.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MagicOnion services with static providers for AOT support
builder.Services.AddGrpc();
builder.Services.AddMagicOnion()
    .UseStaticMethodProvider<MagicOnionMethodProvider>()    // AOT-compatible service methods
    .UseStaticProxyFactory<MulticasterProxyFactory>();      // AOT-compatible StreamingHub broadcast

var app = builder.Build();

// Map MagicOnion services
app.MapMagicOnionService([typeof(GreeterService), typeof(ChatHub)]);

app.MapGet("/", () => "MagicOnion AOT Sample Server");

app.Run();
