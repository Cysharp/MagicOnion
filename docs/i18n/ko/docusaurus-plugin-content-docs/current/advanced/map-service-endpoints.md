# 서비스의 엔드포인트 매핑

## 특정 타입이나 어셈블리에 포함된 서비스만 매핑하기

MagicOnion의 서버는 기본적으로 실행 중인 어셈블리에 포함된 서비스를 자동으로 검색하여, 발견한 서비스를 모두 등록하고 공개합니다. 하지만 경우에 따라서는 특정 타입이나 특정 어셈블리에 포함된 타입만을 공개하고 싶은 경우가 있습니다.

`MapMagicOnionService` 메소드에는 특정 타입이나 어셈블리에 포함된 서비스만을 매핑하는 오버로드가 존재합니다. 이 오버로드를 지정함으로써 수동으로 서비스를 등록할 수 있습니다.

```csharp
app.MapMagicOnionService([ typeof(MyService), typeof(MyHub) ]);
app.MapMagicOnionService([ typeof(MyService).Assembly ]);
```

## 엔드포인트 메타데이터 설정

`MapMagicOnionService` 메소드가 반환하는 빌더에서는 ASP.NET Core의 엔드포인트 메타데이터를 설정하는 메소드를 이용할 수 있습니다. 예를 들어 `RequireHost`나 `RequireAuthorization`과 같은 메소드입니다.

이를 통해 아래와 같은 형태로 여러 포트에서 다른 서비스를 제공하는 것도 가능합니다.

```csharp
// Consumers endpoints
app.MapMagicOnionService([typeof(GreeterService), typeof(ChatHub)]);

// Administration endpoints
app.MapMagicOnionService([typeof(AdministrationService)])
    .RequireHost("*:6000")
    .RequireAuthorization();
```
