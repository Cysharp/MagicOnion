using AotSample.Server;
using AotSample.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MagicOnion services with the static method provider for AOT support
builder.Services.AddGrpc();
builder.Services.AddMagicOnion()
    .UseStaticMethodProvider<MagicOnionMethodProvider>();

var app = builder.Build();

// Map MagicOnion services
app.MapMagicOnionService([typeof(GreeterService)]);

app.MapGet("/", () => "MagicOnion AOT Sample Server");

app.Run();
