# 서버 상의 연결, 연결 해제 이벤트

서버 상에서는 클라이언트가 연결되거나 연결 해제가 될 때 이벤트가 발생합니다. 이러한 이벤트들은 `StreamingHubBase` 클래스의 이벤트 메소드를 오버라이드함으로써 핸들링할 수 있습니다.

## `OnConnecting` 메서드
`OnConnecting` 메서드는 클라이언트가 StreamingHub에 연결을 설정할 때 호출됩니다. 이 시점에서는 클라이언트와의 연결이 아직 설정되지 않았으므로, 클라이언트나 그룹에 대한 작업을 수행할 수 없습니다.

```csharp
protected override async ValueTask OnConnecting()
{
    // 클라이언트가 연결을 설정하는 중의 처리

    // 예: StreamingHub 자체의 초기화 등
}
```

## `OnConnected` 메서드
`OnConnected` 메서드는 클라이언트가 StreamingHub에 연결을 완료했을 때 호출됩니다. 이 시점에서는 클라이언트와의 연결이 설정되었으므로, 클라이언트를 호출하거나 그룹에 대한 작업을 수행할 수 있습니다.

```csharp
protected override async ValueTask OnConnected()
{
    // 클라이언트가 접속 완료 시의 처리

    // 예: 그룹에 추가하거나 초기 상태의 송신
    // this.group = await Group.AddAsync("MyGroup");
    // Client.OnConnected(initialState);
    // ..
}
```


## `OnDisconnected` 메서드
`OnDisconnected` 메서드는 클라이언트가 StreamingHub에서 연결 해제될 때 호출됩니다. 이 시점에서는 클라이언트와의 연결이 해제되었으므로, 클라이언트와 관련된 작업은 유효하지 않습니다.

```csharp
protected override async ValueTask OnDisconnected()
{
    // 클라이언트가 연결 해제된 시점의 처리
}
```

연결 해제에 관련된 조작이나 정보는 [연결 해제의 핸들링](disconnection)을 참조해 주세요.
