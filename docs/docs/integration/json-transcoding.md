---
title: JSON Transcoding and Swagger
---

# JSON Transcoding and Swagger

The feature inspired by [Microsoft.AspNetCore.Grpc.JsonTranscoding.](https://learn.microsoft.com/en-us/aspnet/core/grpc/json-transcoding?view=aspnetcore-9.0)

This is the successor to the previous MagicOnion.Server.HttpGateway, but it is a completely new implementation and is not compatible, and is mainly intended for development support purposes.

- https://github.com/Cysharp/MagicOnion/pull/859

:::warning
This feature is not intented for use in the `Production` environment.
**If you want to provide Web-based (HTTP/1) APIs, we strongly recommend using ASP.NET Core Web API instead.**

By default, JsonTranscoding cannot be enabled in the `Production` environment. You need to change `MagicOnionJsonTranscodingOptions.AllowEnableInNonDevelopmentEnvironment` to `true`.
:::

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add MagicOnion services to the container and enable JSON transcoding feature.
builder.Services.AddMagicOnion().AddJsonTranscoding();
// Add MagicOnion JSON transcoding Swagger support.
builder.Services.AddMagicOnionJsonTranscodingSwagger();
// Add Swagger generator services.
builder.Services.AddSwaggerGen(options =>
{
    // Reflect the XML documentation comments of the service definition in Swagger.
    options.IncludeMagicOnionXmlComments(Path.Combine(AppContext.BaseDirectory, "JsonTranscodingSample.Shared.xml"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.MapMagicOnionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
```

![image](https://github.com/user-attachments/assets/a101cb00-c9ad-42b6-93d4-87c0d8d23773)
