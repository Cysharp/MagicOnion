# 연결 해제의 핸들링

StreamingHub는 클라이언트와 서버 양쪽에서 연결 해제를 감지하는 메커니즘을 가지고 있습니다. 이는 StreamingHub가 통신을 위해 클라이언트와 서버 간의 지속적인 연결을 설정하고, 어떤 이유로 연결이 끊어질 수 있기 때문입니다. MagicOnion은 서버와 클라이언트 양쪽에서 애플리케이션이 연결 해제를 감지할 수 있는 메커니즘을 제공합니다.

:::tip
연결 해제는 서버와 클라이언트 측에서 서로 다른 타이밍에 감지됩니다. 이는 클라이언트가 연결 해제를 감지하더라도 서버가 아직 이를 인식하지 못할 수 있다는 것을 의미합니다. 따라서 클라이언트와 서버 양쪽에서 연결 해제를 감지하는 것이 중요합니다.
:::

## 서버에서 연결 해제 감지하기
서버 측에서 StreamingHub의 연결 해제를 감지하고 싶을 때는 `OnDisconnected` 메서드를 사용합니다.

```csharp
protected override ValueTask OnDisconnected()
{
    return ValueTask.CompletedTask;
}
```

## 클라이언트에서 연결 해제 감지하기
클라이언트 측에서 StreamingHub의 연결 해제를 감지하고 싶을 때는 `WaitForDisconnected` 또는 `WaitForDisconnectAsync` API를 사용합니다.

이러한 API들은 StreamingHub 클라이언트가 어떤 이유로 연결이 해제될 때까지 기다리는 `Task`를 반환합니다.

```csharp
using MagicOnion.Client;

var client = await StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, receiver);
_ = WaitForDisconnectEventAsync();

async Task WaitForDisconnectEventAsync()
{
    var reason = await client.WaitForDisconnectAsync();
    if (reason.Type != DisconnectionType.CompletedNormally)
    {
        ...
    }
}
```

### `WaitForDisconnectAsync` API
`WaitForDisconnectAsync` API는 기존의 `WaitForDisconnected`의 업데이트 버전이며, 연결 해제 이유를 받을 수 있게 됩니다.

`WaitForDisconnect`와는 달리, 새로운 API는 `IStreamingHub` 인터페이스를 변경하지 않고, `IStreamingHubMarker` 인터페이스의 확장 메소드로서 추가되었습니다. 이는 바이너리 호환성을 깨지 않도록 하기 위해서입니다.

### APIs
```csharp
namespace MagicOnion.Client
{
    public static class StreamingHubClientExtensions
    {
        /// <summary>
        /// Wait for the disconnection and return the reason.
        /// </summary>
        public static Task<DisconnectionReason> WaitForDisconnectAsync<TStreamingHub>(this TStreamingHub hub) where TStreamingHub : IStreamingHubMarker =>
    }

    /// <summary>
    /// Provides the reason for the StreamingHub disconnection.
    /// </summary>
    public readonly struct DisconnectionReason
    {
        /// <summary>
        /// Gets the type of StreamingHub disconnection.
        /// </summary>
        public DisconnectionType Type { get; }

        /// <summary>
        /// Gets the exception that caused the disconnection.
        /// </summary>
        public Exception? Exception { get; }
    }

    /// <summary>
    /// Defines the types of StreamingHub disconnection.
    /// </summary>
    public enum DisconnectionType
    {
        /// <summary>
        /// Disconnected after completing successfully.
        /// </summary>
        CompletedNormally = 0,

        /// <summary>
        /// Disconnected due to exception while reading messages.
        /// </summary>
        Faulted = 1,

        /// <summary>
        /// Disconnected due to reaching the heartbeat timeout.
        /// </summary>
        TimedOut = 2,
    }
}
```

## 연결 해제 감지의 개선
통신 상황의 악화나 연결 해제의 조기 감지를 실현하기 위해 MagicOnion은 하트비트 기능을 제공하고 있습니다. 하트비트 기능에 대해서는 [하트비트](heartbeat)를 참조해 주세요.

## 재접속
StreamingHub 클라이언트는 자동으로 재접속을 수행하지 않습니다. 애플리케이션은 연결이 해제된 것을 감지하여 필요에 따라 재접속(재생성)을 해야 합니다. 서버 상의 StreamingHub는 재개할 수 없기 때문에, 재접속 후의 사용자와 애플리케이션의 상태에 맞춰서 재개 처리를 수행해 주세요.

```csharp
async ValueTask KeepConnectionAsync(CancellationToken shutdownToken)
{
    while (!shutdownToken.IsCancellationRequested)
    {
        try
        {
            var client = await StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, receiver);
            await client.WaitForDisconnectAsync();
        }
        catch (RpcException ex)
        {
            ...
        }
    }
}
```

StreamingHubClient는 재접속 시에는 다시 만들어야 하기 때문에, 필드에 보관하고 있거나 외부에 공개하고 있는 경우 등은 주의가 필요합니다.
