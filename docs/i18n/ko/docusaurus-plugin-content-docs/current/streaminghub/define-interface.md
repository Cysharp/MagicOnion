# 메소드, 인터페이스 정의

## 개요
StreamingHub는 Unary 서비스와 마찬가지로 .NET의 인터페이스를 사용하여 정의합니다. StreamingHub의 인터페이스는 `IStreamingHub<TSelf, TReceiver>`를 상속할 필요가 있습니다. `TSelf`에는 인터페이스 자신, `TReceiver`에는 receiver 인터페이스를 지정합니다. receiver 인터페이스는 서버에서 클라이언트로 메시지를 송신하고, 수신하기 위한 인터페이스입니다.

다음은 채팅 애플리케이션의 StreamingHub 인터페이스의 예입니다. 클라이언트는 메시지의 수신이나 참가, 퇴장 이벤트를 보내는 receiver 인터페이스를 가지고 있습니다.

```csharp
// A hub must inherit `IStreamingHub<TSelf, TReceiver>`.
public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
{
    ValueTask JoinAsync(string roomName, string userName);
    ValueTask LeaveAsync();
    ValueTask SendMessageAsync(string message);
}

public interface IChatHubReceiver
{
    void OnJoin(string userName);
    void OnLeave(string userName);
    void OnSendMessage(string userName, string message);
}
```

StreamingHub가 제공하는 메소드를 **Hub 메소드**라고 부릅니다. Hub 메소드는 클라이언트에서 호출되는 메소드로, 반환값의 타입은 `ValueTask`, `ValueTask<T>`, `Task`, `Task<T>`, `void` 중 하나여야 합니다. Unary 서비스와는 다르다는 것에 주의가 필요합니다.

클라이언트가 메시지를 받는 입구가 되는 receiver 인터페이스 또한 메소드를 가집니다. 이것들을 **receiver 메소드**라고 부릅니다. receiver 메소드는 서버에서 메시지를 받았을 때 호출되는 메소드입니다. receiver 메소드의 반환값은 `void`여야 합니다. [클라이언트 결과](client-results)를 사용하는 경우를 제외하고, 원칙적으로 `void`를 지정합니다.

## 직렬화(Serialization)
Unary 서비스와 마찬가지로, 메서드 인수와 반환 값은 기본적으로 MessagePack을 사용하여 직렬화됩니다. 따라서, 타입은 반드시 MessagePack에 의해 직렬화 가능하다고 표시되거나 직렬화 가능하도록 구성되어야 합니다. 메서드 인수는 최대 15개로 제한됩니다.

## 상속
StreamingHub의 인터페이스는 상속할 수 있습니다. 이것은 복수의 Hub에서 공통의 메소드를 가질 경우에 도움이 됩니다.

```csharp
public inteface ICommonHub
{
    ValueTask PingAsync();
}

public inteface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>, ICommonHub
{
    ValueTask JoinAsync(string roomName, string userName);
    ValueTask LeaveAsync();
    ValueTask SendMessageAsync(string message);
}
```

## 고급 설정

### `Ignore` attribute
`Ignore` attribute를 사용함으로써 특정 메소드를 Hub 메소드로 인식하지 않도록 할 수 있습니다.


### `MethodId` attribute
`MethodId` attribute를 사용함으로써 메소드의 식별에 사용하는 ID를 수동으로 지정할 수 있습니다. Hub 메소드의 ID는 메소드명으로부터 FNV1A32로 계산된 값을 사용하므로 수동으로 설정할 필요가 없습니다. 메소드명을 변경했지만 원래의 ID를 사용하고 싶거나, 어떤 이유로 ID가 충돌해버렸다거나 하는 특수한 용도에서만 사용해 주세요.
