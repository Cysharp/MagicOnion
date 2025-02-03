# 메타데이터와 헤더

Unary 서비스의 요청과 응답은 메타데이터를 포함할 수 있습니다. 메타데이터는 HTTP 헤더로 취급되며, 클라이언트와 서버 모두에서 설정하고 읽을 수 있습니다. 메타데이터는 gRPC에서 `Metadata` 클래스로 표현됩니다.

이는 헤더 인증 메커니즘이나 버전 정보와 같이 요청과 응답에 추가 정보를 넣어야 할 때 유용합니다.

## 클라이언트

### 요청에 메타데이터 추가하기
요청에 메타데이터를 추가하려면 `MagicOnionClient`의 `WithHeaders` 메서드를 사용합니다. 다음 예제는 `Metadata` 클래스를 사용하여 요청에 `Authorization` 헤더를 추가합니다.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel).WithHeaders(new Metadata
    { "authorization", "Bearer {token}" }
});
```

클라이언트 생성 시 `MagicOnionClientOptions` 클래스를 전달하거나 `MagicOnionClient.WithOptions` 메서드를 사용하여 `CallOptions`를 설정할 수도 있습니다. 또한 [클라이언트 필터](../filter/client-filter)를 사용하여 요청 시점에 메타데이터를 설정할 수 있습니다.

### 응답에서 메타데이터 읽기
`UnaryResult` 구조체의 `ResponseHeadersAsync` 속성을 사용하여 메타데이터를 읽을 수 있습니다. `ResponseHeadersAsync`는 서버가 응답할 때까지 기다립니다.

다음은 응답에서 메타데이터를 읽는 예제입니다.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel);
var result = client.SayHelloAsync("Alice", 18);

var headers = await result.ResponseHeadersAsync;
```

## 서버
서버에서도 클라이언트로부터 받은 요청의 메타데이터를 읽고, 응답에 메타데이터를 추가할 수 있습니다.

### 요청에서 메타데이터 읽기

`ServiceContext` 클래스의 `CallContext.RequestHeaders` 속성을 사용하여 요청의 메타데이터를 읽을 수 있습니다.

```csharp
if (Context.CallContext.RequestHeaders.GetValue("authorization") is {} authorizationHeader)
{
    ...
}
```

### 응답에 메타데이터 추가하기

`ServiceContext` 클래스의 `CallContext.WriteResponseHeadersAsync` 메서드를 사용하여 응답에 메타데이터를 추가할 수 있습니다.

```csharp
Context.CallContext.WriteResponseHeadersAsync(new Metadata
{
    { "x-server-version", "1.0.0" }
});
```

또한, ASP.NET Core의 표준 방식인 `HttpContext`에서 HTTP 헤더로 값을 설정하는 것도 가능합니다.

```csharp
Context.CallContext.GetHttpContext().Response.Headers.TryAdd("x-server-version", "1.0.0");
```
