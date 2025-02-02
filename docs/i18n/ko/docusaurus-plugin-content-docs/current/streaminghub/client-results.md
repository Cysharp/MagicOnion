# 클라이언트 결과

:::tip
이 기능은 MagicOnion v7.0.0에서 추가되었습니다.
:::

"클라이언트 결과"는 [SignalR에서 구현된 동명의 기능](https://learn.microsoft.com/ko-kr/aspnet/core/signalr/hubs#client-results)에서 영감을 받은 것입니다.

기존에 StreamingHub는 서버에서 클라이언트에 대해서는 메시지를 일방적으로 송신하는(Fire-and-Forget) 것밖에 할 수 없었지만, 클라이언트 결과는 서버의 Hub나 애플리케이션 로직에서 특정 클라이언트의 메소드를 호출하고, 그 결과를 받을 수 있습니다.

```csharp
interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
}

interface IMyHubReveiver
{
    // The Client results method is defined in the Receiver with a return type of Task or Task<T>
    Task<string> HelloAsync(string name, int age);

    // Regular broadcast method
    void OnMessage(string message);
}

// Client implementation
class MyHubReceiver : IMyHubReceiver
{
    public async Task<string> HelloAsync(string name, int age)
    {
        Console.WriteLine($"Hello from {name} ({age})");
        var result = await ReadInputAsync();
        return result;
    }
    public void OnMessage()
    {
        Console.WriteLine($"OnMessage: {message}");
    }
}
```

서버 상에서는 그룹의 `Client` 또는 `Single`을 통해 메소드 호출을 수행하고, 결과를 받을 수 있습니다.

```csharp
var result = await Client.HelloAsync();
Console.WriteLine(result);
// or
var result2 = await _group.Single(clientId).HelloAsync();
Console.WriteLine(result2);
```

## 예외
클라이언트 상에서 예외가 발생한 경우, 호출자에게는 `RpcException`이 던져지고, 연결이 해제된 경우나 타임아웃이 발생한 경우는 `TaskCanceledException` (`OperationCanceledException`)이 던져집니다.

## 타임아웃
기본적으로 서버에서 클라이언트로의 호출의 타임아웃은 5초입니다. 타임아웃이 초과된 경우, 호출자에게는 `TaskCanceledException` (`OperationCanceledException`)이 던져집니다. 기본 타임아웃은 `MagicOnionOptions.ClientResultsDefaultTimeout` 속성을 통해 설정할 수 있습니다.

메소드 호출마다 타임아웃을 명시적으로 오버라이드하려면 메소드 인자로 `CancellationToken`을 지정하고, 임의의 타이밍에서 `CancellationToken`을 전달하여 타임아웃을 지정합니다. 이 취소는 클라이언트에게는 전파되지 않는다는 점에 주의해 주세요. 클라이언트는 항상 `default(CancellationToken)`을 받습니다.

```csharp
interface IMyHubReceiver
{
    Task<string> DoLongWorkAsync(CancellationToken timeoutCancellationToken = default);
}
```
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var result = await Client.DoLongWorkAsync(cts.Token);
Console.WriteLine(result);
```

## 제한사항
- 복수의 클라이언트에 대해 호출을 수행하는 것은 지원되지 않습니다. `Client` 또는 `Single`을 사용해야 합니다
- 그룹의 백플레인으로 Redis 또는 NAT가 사용되고 있는 경우, 클라이언트 결과는 지원되지 않습니다
- 클라이언트 측에서 클라이언트 결과 메소드 호출 중에 Hub 메소드를 호출하면 데드락이 발생합니다
