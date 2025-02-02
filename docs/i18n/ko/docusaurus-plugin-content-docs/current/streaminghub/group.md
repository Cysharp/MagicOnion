# 그룹 기본 사항

StreamingHub는 복수의 클라이언트에 메시지를 배포하는 구조가 있습니다. 예를 들어 채팅에서 수신한 텍스트 메시지를 클라이언트에 배포하는 경우에 사용합니다.

이 배포하는 대상을 서버에서 관리하는 구조가 그룹입니다. 그룹은 임의의 이름으로 생성할 수 있으며, 그 그룹에 소속되어 있는 클라이언트에 대해 메시지를 송신할 수 있게 됩니다. 이것은 채팅의 채널이나 룸과 같은 역할을 합니다.

![](/img/docs/fig-group-broadcast.png)

## 그룹 생성 및 클라이언트 추가

그룹은 StreamingHub의 `Group` 속성의 `AddAsync` 메소드에 그룹명을 전달하여 호출함으로써, 참가한 그룹의 인스턴스를 취득할 수 있습니다. 호출 시에 아직 그룹이 존재하지 않는 경우에는 새로 생성됩니다.

```csharp
public async ValueTask JoinAsync(string userName, string roomName)
{
    // Add client to the group with the specified group name.
    // If the group does not exist, it will be created.
    this.room = await Group.AddAsync(roomName);
    // ...
}
```

## 그룹에 메시지 전달
이 그룹의 인스턴스는 receiver에 대한 프록시를 제공하며, 그룹에 속한 클라이언트에 대한 메시지의 일제 배포를 수행할 수 있습니다. StreamingHub에서의 개발에서는 이 인스턴스를 필드에 보관해 두고 필요에 따라 호출합니다.

```csharp
// 룸에 포함된 모든 클라이언트에 "Hello, world!" 라는 메시지를 송신
this.room.All.OnMessage("Hello, world!");
```

모든 클라이언트뿐만 아니라, 특정 클라이언트 등 송신 대상을 한정한 프록시를 취득할 수도 있습니다.

```csharp
this.room.Only([connectionId1, connectionId2]).OnMessage("Hello, world! to specific clients");

this.room.Except(ConnectionId).OnMessage("Hello, world! except me");

this.room.Single(ConnectionId).OnMessage("Hello, world! to me");
```

- `All`: 그룹에 포함된 모든 클라이언트
- `Single`: 특정 1 클라이언트
- `Only`: 특정 클라이언트(복수)
- `Except`: 특정 클라이언트 이외(복수)

## 그룹에서 클라이언트의 삭제
그룹에서 클라이언트를 삭제하려면 `RemoveAsync` 메소드를 사용합니다.

```csharp
public async ValueTask LeaveAsync()
{
    // 그룹에서 클라이언트를 삭제
    await this.room.RemoveAsync(Context);
}
```

클라이언트가 서버에서 연결이 끊어진 경우에는 자동적으로 그룹에서 삭제되므로, 명시적으로 삭제할 필요가 없습니다. 또한, 그룹에 포함된 클라이언트가 없어진 시점에서 그룹은 삭제됩니다.

## 보다 세밀한 그룹 제어
여기서 설명한 그룹은 MagicOnion의 StreamingHub와 연결되어 관리되고 있습니다. 즉 클라이언트가 그룹의 생성과 그것에 포함되는 클라이언트의 관리는 Hub를 통해서 조작할 필요가 있습니다.

한편 게임과 같은 애플리케이션에서는 그룹의 생성이나 삭제, 클라이언트의 관리를 애플리케이션 로직 측에서 수행하고 싶은 경우도 있습니다. 그 경우에는 Hub의 그룹을 사용하지 않고, 애플리케이션 측에서 그룹의 관리를 수행할 수 있습니다. 자세한 내용은 [애플리케이션 관리형 그룹](group-application-managed) 페이지를 참조하시기 바랍니다.

## Thread Safety
그룹은 Thread Safety하며, 복수의 클라이언트로부터의 동시 액세스에 대해서 안전하게 조작할 수 있습니다. 단, 그룹 인스턴스의 생성, 삭제 시의 일관성은 애플리케이션에서 보증할 필요가 있습니다.

예를 들어 StreamingHub에 연결된 그룹에서는 마지막 사용자의 Remove와 새로운 사용자의 Add가 거의 동시에 실행된 경우는 한번 삭제된 후에 새로운 그룹이 생성됩니다. 이 동작은 그룹을 보유하고 있는 경우에는 문제가 되는 경우가 있습니다.

그룹에 포함되는 클라이언트의 증감과 메시지 배포에서 엄격한 일관성이 필요한 경우에도 애플리케이션 측에서 그룹의 관리나 락 등을 수행하는 것을 검토해 주세요.
