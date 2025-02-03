# 예외 처리 및 상태 코드

MagicOnion은 서버의 처리 결과를 반환값과는 별도로 상태 코드로 반환하는 구조가 있습니다. 이는 HTTP의 상태 코드나 gRPC의 상태 코드와 유사합니다. 이 상태 코드에는 애플리케이션 고유의 상태 코드를 포함할 수도 있습니다.

## 서버에서 클라이언트로 상태 코드 알림
서버에서 클라이언트로 커스텀 상태 코드를 반환하는 경우에는 `ReturnStatusException`을 사용할 수 있습니다.

```csharp
public Task SendMessageAsync(string message)
{
    if (message.Contains("foo"))
    {
        //
        throw new ReturnStatusException((Grpc.Core.StatusCode)MyStatusCode.SomethingWentWrong, "invalid");
    }

    // ....
```

`ReturnStatusException`은 상태 코드인 `Grpc.Core.StatusCode` 열거형을 가지며, 이 값을 클라이언트로 통지합니다. 애플리케이션의 커스텀 상태 코드를 반환하려면 독자적인 `int` 값 또는 열거형을 `Grpc.Core.StatusCode` 열거형으로 캐스팅하세요.

예외 발생을 피하여 성능을 중시하는 경우에는 `CallContext.Status` (`ServiceContext.CallContext.Status`)를 사용하여 상태를 직접 설정할 수 있습니다.

## 클라이언트에서 예외 처리
클라이언트에서는 메서드 호출 시의 예외가 모두 gRPC의 `RpcException`으로 수신됩니다.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel);
try
{
    var result = await client.SayHelloAsync("Alice", 18);
}
catch (RpcException ex)
{
    // handle exception ...
    if (((MyStatusCode)ex.Status.StatusCode) == MyStatusCode.SomethingWentWrong)
    {
        // ...
    }
}
```

네트워크에 문제가 있는 경우에는 `RpcException`이 `StatusCode.Unavailable`로 발생됩니다. 자세한 에러 내용은 `InnerException` 속성으로 조사할 수 있습니다.

## 서버에서 처리되지 않은 예외
서버의 메서드 호출 중에 에러가 발생하여 `ReturnStatusException`을 제외한 처리되지 않은 예외가 발생된 경우, 클라이언트에서는 `StatusCode.Unknown`이 설정된 `RpcException`으로 처리됩니다.

이때 `MagicOnionOption.IsReturnExceptionStackTraceInErrorDetail`이 `true`인 경우, 클라이언트는 서버의 예외 스택 트레이스(Stack Trace)를 수신할 수 있습니다. 이는 디버깅에 매우 유용하지만, 심각한 보안 문제가 있으므로 디버그 빌드에서만 활성화해야 합니다.
