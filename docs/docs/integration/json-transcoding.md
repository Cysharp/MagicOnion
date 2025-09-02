# JSON Transcoding and Swagger

JSON transcoding is a mechanism that converts Unary services into JSON APIs and provides them as HTTP/1 endpoints, making it possible to call Unary services from tools such as cURL.

This is the successor to the previous MagicOnion.Server.HttpGateway, but it is a completely new implementation and is not compatible, and is mainly intended for development support purposes.

The feature inspired by [Microsoft.AspNetCore.Grpc.JsonTranscoding.](https://learn.microsoft.com/en-us/aspnet/core/grpc/json-transcoding?view=aspnetcore-9.0)

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
    // To use this feature, you must enable the Generate XML Comments option in project options.
    options.IncludeMagicOnionXmlComments(typeof(IMyService).Assembly);
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



## Troubleshooting

If you encounter the following exception at runtime, adding `builder.Services.AddEndpointsApiExplorer();` can help resolve the issue.
```csharp
System.AggregateException: Some services are not able to be constructed (Error while validating the service descriptor 'ServiceType: Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator Lifetime: Transient ImplementationType: Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator': No constructor for type 'Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator' can be instantiated using services from the service container and default values.)
```


Reference
MSDN documentation for the usage of Swashbuckle.AspNetCore.Swagger used in MagicOnion.
https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio

```csharp
builder.Services.AddEndpointsApiExplorer();
```
