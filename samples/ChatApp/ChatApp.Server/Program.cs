using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server.JsonTranscoding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http2;
    });
});
builder.Services.AddMagicOnion();
builder.Services.AddSingleton(typeof(IServiceMethodProvider<>), typeof(MagicOnionJsonTranscodingGrpcServiceMethodProvider<>));

var app = builder.Build();
app.MapMagicOnionService();

app.Run();
