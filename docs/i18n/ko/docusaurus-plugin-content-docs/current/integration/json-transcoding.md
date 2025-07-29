# JSON 트랜스코딩과 Swagger

JSON 트랜스코딩은 Unary 서비스를 HTTP/1 엔드포인트에서 JSON API로 변환하여 제공하는 구조입니다. 이를 통해 cURL과 같은 도구에서 Unary 서비스를 호출하는 것이 가능해집니다.
이는 이전의 MagicOnion.Server.HttpGateway의 후속이며, 완전히 새로운 구현으로 호환성은 없으며, 주로 개발 지원 목적입니다.

이 기능은 [Microsoft.AspNetCore.Grpc.JsonTranscoding](https://learn.microsoft.com/ko-kr/aspnet/core/grpc/json-transcoding)에서 영감을 받았습니다.

- https://github.com/Cysharp/MagicOnion/pull/859

:::warning
이 기능은 `Production` 환경에서의 사용을 의도하지 않았습니다.
**만약 웹 기반(HTTP/1) API를 제공하고 싶다면, 대신 ASP.NET Core Web API를 강력히 추천합니다.**

기본적으로 JsonTranscoding은 `Production` 환경에서 활성화할 수 없습니다. `MagicOnionJsonTranscodingOptions.AllowEnableInNonDevelopmentEnvironment`를 `true`로 변경해야 합니다.
:::

## 활성화
JSON 트랜스코딩을 활성화하려면, `AddMagicOnion` 다음에 `AddJsonTranscoding`을 호출하고, Swagger를 활성화하기 위해 `AddJsonTranscodingSwagger`를 호출합니다.

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



## 트러블슈팅

런타임에서, 아래와같은 Exception 발생시, `builder.Services.AddEndpointsApiExplorer();` 구문 추가가 도움이 될수있습니다.
```csharp
System.AggregateException: Some services are not able to be constructed (Error while validating the service descriptor 'ServiceType: Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator Lifetime: Transient ImplementationType: Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator': No constructor for type 'Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator' can be instantiated using services from the service container and default values.)
```


참고  
MagicOnion 에서 사용된 Swashbuckle.AspNetCore.Swagger 의 사용법이 기록된 MSDN 입니다.  
https://learn.microsoft.com/ko-kr/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio

```csharp
builder.Services.AddEndpointsApiExplorer();
```
