# HTTPS/TLS

MagicOnion은 TLS에 의한 암호화 연결을 지원합니다. 이 페이지에서는 MagicOnion에서의 TLS 암호화 연결 설정 방법에 대해 설명합니다.

## 서버
서버 측의 HTTPS 암호화 설정은 ASP.NET Core를 따릅니다. 자세한 내용은 [ASP.NET Core에서 HTTPS 적용 | Microsoft Docs](https://learn.microsoft.com/ko-kr/aspnet/core/security/enforcing-ssl)를 참조해 주세요.

## 클라이언트
클라이언트 측의 HTTPS 암호화는 런타임이 .NET Framework/.NET 8 이상인지 또는 Unity인지에 따라 동작이나 설정이 다릅니다. 이는 개발용 인증서를 사용하는 경우에 영향을 미칩니다.

### .NET Framework 또는 .NET 8+
.NET Framework 또는 .NET 환경에서의 인증서 처리는 `HttpClient`의 표준적인 동작과 같으며 OS의 인증서 저장소를 사용합니다. 예를 들어, Windows에서는 Windows의 인증서 저장소를 사용하여 인증서를 검증합니다.

### Unity
클라이언트가 Unity인 경우, MagicOnion에서는 YetAnotherHttpHandler의 사용을 권장하고 있습니다. YetAnotherHttpHandler는 독자적인 인증서 저장소를 가지고 있기 때문에 개발용 인증서를 사용하는 경우에는 추가 설정이 필요합니다. 자세한 내용은 [YetAnotherHttpHandler의 문서](https://github.com/Cysharp/YetAnotherHttpHandler?tab=readme-ov-file#advanced)를 참조해 주세요.

## TLS 없이 암호화되지 않은 HTTP 연결 사용하기
일반적으로 서버와 클라이언트 간의 연결에는 HTTPS를 사용하는 것을 권장합니다. 하지만 개발 중에 임시로 암호화되지 않은 연결을 설정하고 싶은 경우가 있을 수 있습니다. 설정을 변경하여 암호화되지 않은 HTTP/2 연결을 구성할 수 있습니다(암호화되지 않은 HTTP/2는 cleartext를 통한 HTTP/2(h2c)라고 합니다).

### 서버
암호화되지 않은 HTTP/2 연결을 수락하려면 Kestrel에서 엔드포인트를 구성해야 합니다. `appsettings`에서 엔드포인트를 구성할 수 있습니다.

```json
{
    ...
    "Kestrel": {
        "Endpoints": {
            "Grpc": {
                "Url": "http://localhost:5000",
                "Protocols": "Http2"
            },
            "Https": {
                "Url": "https://localhost:5001",
                "Protocols": "Http1AndHttp2"
            },
            "Http": {
                "Url": "http://localhost:5002",
                "Protocols": "Http1"
            }
        }
    },
    ...
}
```
또는
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http2;
    });
});
```

### 클라이언트
`GrpcChannel.ForAddress` 호출 시 URL 스키마를 HTTP로 변경하고 포트 번호를 암호화되지 않은 포트로 변경해야 합니다.

```csharp
var channel = GrpcChannel.ForAddress("http://localhost:5000");
```

자세한 내용은 [.NET Core 클라이언트를 사용하여 안전하지 않은 gRPC 서비스 호출](https://learn.microsoft.com/ko-kr/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client)를 참조하시기 바랍니다.

#### Unity 클라이언트에서의 추가 대응
Unity에서 YetAnotherHttpHandler를 사용하고 있는 경우, YetAnotherHttpHandler의 인스턴스를 생성할 때 `Http2Only` 옵션을 지정하도록 변경해야 합니다.

```csharp
var handler = new YetAnotherHttpHandler(new()
{
    Http2Only = true,
});
```

### 제한사항
암호화되지 않은 HTTP/2 연결을 수락하려는 경우, 동일한 포트에서 HTTP/1과 HTTP/2를 모두 제공할 수 없습니다. 이는 암호화되지 않은 HTTP/2 연결을 수락할 때 HTTP/2 협상을 위해 ALPN이 사용되는데, 이는 TLS 없이는 수행할 수 없기 때문입니다.

HTTP/1과 HTTP/2를 모두 지원하는 웹사이트나 API를 호스팅하려면, Kestrel이 여러 포트에서 수신하도록 구성하여 이를 달성할 수 있습니다.
