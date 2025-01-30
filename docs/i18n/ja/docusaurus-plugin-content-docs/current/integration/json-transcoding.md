# JSON トランスコーディングと Swagger

JSON トランスコーディングはUnary サービスを  HTTP/1 エンドポイントで JSON API として変換して提供する仕組みです。これにより cURL のようなツールから Unary サービスを呼び出すことが可能となります。
これは以前の MagicOnion.Server.HttpGateway の後継であり、完全に新しい実装であり互換性はなく、主に開発サポート目的です。

この機能は [Microsoft.AspNetCore.Grpc.JsonTranscoding](https://learn.microsoft.com/en-us/aspnet/core/grpc/json-transcoding?view=aspnetcore-9.0) に触発されたものです。

- https://github.com/Cysharp/MagicOnion/pull/859

:::warning
この機能は `Production` 環境での使用を意図していません。
**もし Web ベース (HTTP/1) の API を提供したい場合は、代わりに ASP.NET Core Web API を強くお勧めします。**

デフォルトでは JsonTranscoding は `Production` 環境で有効にすることはできません。`MagicOnionJsonTranscodingOptions.AllowEnableInNonDevelopmentEnvironment` を `true` に変更する必要があります。
:::

## 有効化
JSON トランスコーディングとを有効にするには、`AddMagicOnion` に続いて `AddJsonTranscoding` を呼び出し、Swagger を有効にするために `AddJsonTranscodingSwagger` を呼び出します。

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
