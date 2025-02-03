# 하트비트 (Heartbeat)

:::tip
이 기능은 MagicOnion v7.0.0에서 추가되었습니다.
:::

하트비트 기능은 서버와 클라이언트 간의 접속을 유지하고 연결 해제를 조기에 감지하기 위해, 서버에서 클라이언트로, 클라이언트에서 서버로 정기적으로 메시지를 송신하는 기능입니다. 하트비트에는 송신처가 일정 시간 내에 응답하지 않는 경우에 연결을 해제하기 위한 타임아웃을 지정할 수 있습니다.

## HTTP/2의 PING 프레임을 사용하지 않는 이유

HTTP/2에는 하트비트를 위한 PING 프레임이라는 메커니즘이 있습니다. 그럼에도 불구하고 MagicOnion에는 독자적인 하트비트 기능이 있습니다. 이는 네트워크 구성에 로드밸런서가 포함되어 있는 경우, 로드밸런서가 PING 프레임을 처리하여 MagicOnion 서버에 도달하지 않을 가능성이 있기 때문입니다.

```plaintext
[Client] ← PING/PONG → [LoadBalancer] ← PING/PONG → [Server]
```

MagicOnion은 이러한 환경에서의 소통 확인을 확실하게 하기 위해, 서버와 클라이언트 간에 명시적으로 데이터를 송수신하는 하트비트 메커니즘을 제공하고 있습니다.

## 클라이언트
`StreamingHubClient`를 생성할 때 클라이언트로부터의 하트비트를 지정할 수 있습니다. 기본적으로는 비활성화되어 있습니다.

```csharp
// Send a message to the server every 30 seconds
var options = StreamingHubClientOptions.CreateWithDefault().WithClientHeartbeatInterval(TimeSpan.FromSeconds(30));
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

### API
```csharp
public class StreamingHubClientOptions
{
    /// <summary>
    /// Sets a heartbeat interval. If a value is <see keyword="null"/>, the heartbeat from the client is disabled.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatInterval(TimeSpan? interval);

    /// <summary>
    /// Sets a heartbeat timeout period. If a value is <see keyword="null"/>, the client does not time out.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatTimeout(TimeSpan? timeout);

    /// <summary>
    /// Sets a heartbeat callback. If additional metadata is provided by the server in the heartbeat message, this metadata is provided as an argument.
    /// </summary>
    /// <param name="onServerHeartbeatReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithServerHeartbeatReceived(Action<ServerHeartbeatEvent>? onServerHeartbeatReceived);

    /// <summary>
    /// Sets a client heartbeat response callback.
    /// </summary>
    /// <param name="onClientHeartbeatResponseReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatResponseReceived(Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived);
}
```

## 서버
서버로부터의 하트비트는 `MagicOnionOptions`를 통해 전역적으로 활성화하거나 `Heartbeat` 속성을 사용하여 개별적으로 활성화할 수 있습니다. 기본적으로는 비활성화되어 있습니다.

```csharp
// Enable heartbeat for all StreamingHub instances
options.EnableStreamingHubHeartbeat = true;
// Send heartbeat every 30 seconds, disconnect if no response within 5 seconds
options.StreamingHubHeartbeatInterval = TimeSpan.FromSeconds(30);
options.StreamingHubHeartbeatTimeout = TimeSpan.FromSeconds(5);
```

```csharp
// Enable heartbeat using the interval and timeout specified in MagicOnionOptions
[Heartbeat]
public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>
{
}

// Enable heartbeat and override the interval and timeout specified in MagicOnionOptions
[Heartbeat(Interval = 10 * 1000, Timeout = 1000)]
public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>
{
}
```

서버 측의 하트비트는 "서버가 하트비트를 송신한 시각"을 `ServerTime` 속성에 설정합니다. 클라이언트는 이것을 사용하여 클라이언트와 서버의 시각을 동기화할 수 있습니다. 서버 시각은 항상 UTC로 송신됩니다.

```csharp
// Client-side code:
var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x =>
{
    var serverTime = x.ServerTime; // ServerTime is always UTC.
});
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

## 추가 메타데이터
추가 메타데이터를 서버의 하트비트 메시지에 추가할 수 있습니다. 이는, 예를 들어, 동기화 목적으로 서버 정보를 포함하기 위해 사용할 수 있습니다.

### 서버
메타데이터를 서버의 하트비트 메시지에 추가하려면, `IStreamingHubHeartbeatMetadataProvider` 인터페이스를 구현하고, DI 컨테이너에 등록하거나, `Heartbeat` 속성의 `MetadataProvider` 프로퍼티로 지정합니다.

```csharp
public class CustomHeartbeatMetadataProvider : IStreamingHubHeartbeatMetadataProvider
{
    public bool TryWriteMetadata(IBufferWriter<byte> writer)
    {
        MessagePackSerializer.Serialize(writer, new Version(1, 0, 0, 0));
        return true;
    }
}
```
### 클라이언트
클라이언트 측에서는, `StreamingHubClient`를 생성할 때 하트비트의 콜백을 옵션으로 설정할 수 있습니다.

```csharp
var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x =>
{
    var serverVersion = MessagePackSerializer.Deserialize<Version>(x.Metadata);
});
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

## 서버 상에서의 하트비트의 고급 조작
서버로부터의 하트비트는 `IMagicOnionHeartbeatFeature` 인터페이스를 통해 몇 가지 고급 기능에 접근할 수 있습니다. 이 인터페이스의 구현은 `IHttpContext.Features` (`Context.CallContext.GetHttpContext().Features`)에서 얻을 수 있습니다.

### `Unregister` 메소드
`Unregister` 메소드는 해당 StreamingHub의 클라이언트 접속에서의 하트비트를 비활성화합니다. 디버그 시에 일시적으로 하트비트를 비활성화하는 구조가 필요한 경우에 이용할 수 있습니다.

### `SetAckCallback` 메소드
클라이언트로부터의 하트비트 응답을 수신했을 때의 콜백을 설정할 수 있습니다.

### `Latency` 속성
클라이언트와의 레이턴시를 취득합니다. 송수신되지 않은 경우는 `TimeSpan.Zero`가 반환됩니다.

## 제한사항

하트비트는 v7.0.0 이전의 클라이언트나 서버와 호환성이 없습니다. v7.0.0의 전후 버전이 혼재할 가능성이 있는 StreamingHub에서 활성화하지 마세요.
