# 애플리케이션 관리형 그룹

:::tip
이 기능은 MagicOnion v7.0.0에서 추가되었습니다. https://github.com/Cysharp/MagicOnion/pull/778
:::

게임과 같은 애플리케이션에서는 애플리케이션의 로직에 의해 그룹의 세세한 제어를 하고 싶은 케이스가 존재합니다.

예를 들어 게임 내의 배틀필드와 참가 플레이어, 팀이라는 요소가 있는 경우에 모든 플레이어는 동일한 배틀필드에 접속하지만 팀별로 커뮤니케이션 채널을 가질 필요가 있는 케이스나, 하나의 대전 중에 플레이어의 팀을 재구성하는 경우가 있는 케이스 등입니다. 그 외에도 메타버스 공간처럼 참가자가 아직 없는 시점에서도 그룹이 필요한 케이스도 생각할 수 있습니다.

v7 이전의 MagicOnion에서는 그룹의 생성은 StreamingHub 안에서 수행하는 것밖에 할 수 없었지만, v7부터는 [Multicaster](https://github.com/Cysharp/Multicaster) 라이브러리를 기반으로 함으로써 애플리케이션이 독자적으로 그룹을 생성할 수 있게 되었습니다.

또한 이로 인해 StreamingHub나 MagicOnion 자체의 의존성을 가지지 않고도 그룹을 생성할 수 있기 때문에, 로직의 테스트도 용이해집니다.

## 그룹 생성/가져오기
그룹을 생성하려면, Dependency Injection을 통해 얻은 `IMulticastGroupProvider` 인터페이스(group provider)를 사용합니다. 이 인터페이스의 구현은 이미 MagicOnion에 의해 등록되어 있으므로, StreamingHub, Unary 서비스 및 기타 Hosted Services와 같이 DI 컨테이너에 등록된 클래스에서 사용할 수 있습니다.

`GetOrAddSynchronousGroup` 또는 `GetOrAddAsynchronousGroup`에 클라이언트를 구별하는 키의 타입 (예를 들어 `Guid`나 애플리케이션 고유의 사용자 ID 등)과 클라이언트의 인터페이스 타입(대부분의 경우 StreamingHub의 receiver), 그룹 이름을 지정하여 그룹을 생성 또는 취득합니다. group provider 내에 지정된 이름의 그룹이 없는 경우는 새로 생성됩니다.

```csharp
public class GroupService(IMulticastGroupProvider groupProvider) : IDisposable
{
    // NOTE: You can also manage multiple groups using a dictionary, etc.
    private readonly IMulticastSyncGroup<Guid, IMyReceiver> _group
         = groupProvider.GetOrAddSynchronousGroup<Guid, IMyHubReceiver>("MyGroup");

    public void Dispose() => _group.Dispose();
}
```

:::tip
`GetOrAddSynchronousGroup`는 그룹 조작을 동기적으로 처리할 수 있는 경우, 예를 들어 그룹을 인메모리에서만 처리하는 경우에 적합합니다. `GetOrAddAsynchronousGroup`는 그룹 조작에 비동기를 필요로 하는 경우, 예를 들어 Redis를 사용하여 복수의 MagicOnion 서버에서 그룹을 배포하고 있는 경우에 적합합니다.
:::

## 그룹 삭제

그룹 삭제는 `IGroup<T>.Dispose` 메서드를 호출함으로써 수행합니다. 이는 group provider에서 삭제하는 것을 의미합니다. 그 후 다시 `GetOrAddSynchronousGroup` 또는 `GetOrAddAsynchronousGroup`을 호출함으로써 새로운 그룹을 생성할 수 있습니다.

## 그룹에 클라이언트를 등록

그룹에 클라이언트를 등록하려면 `Add` 또는 `AddAsync` 메서드를 호출합니다. 이 메서드는 클라이언트를 구별하는 키와 클라이언트를 인자로 받습니다. 클라이언트의 인터페이스를 StreamingHub의 receiver와 같은 타입으로 하고 있는 경우는 등록하는 클라이언트로서 StreamingHub의 `Client` 속성을 전달할 수 있습니다.

```csharp
_group.Add(ConnectionId, Client);
```

## 그룹에서 클라이언트를 삭제

그룹에서 클라이언트를 삭제하려면 `Remove` 또는 `RemoveAsync` 메서드를 호출합니다. 이 메서드는 클라이언트를 구별하는 키를 인자로 받습니다.

```csharp
_group.Remove(ConnectionId);
```

StreamingHub의 그룹과 달리 그룹에 포함된 클라이언트가 없어진 경우에도 그룹은 파기되지 않습니다.

## 그룹에 대한 메시지의 배포
그룹에 대한 메시지의 배포 방법은 MagicOnion의 StreamingHub가 제공하는 그룹과 마찬가지로, `All`이나 `Except`, `Single` 등을 사용하여 송신처를 결정하여 호출할 수 있습니다.

```csharp
_group.All.OnMessage("Hello, world!");

_group.Only([connectionId1, connectionId2]).OnMessage("Hello, world! to specific clients");

_group.Except(ConnectionId).OnMessage("Hello, world! except me");

_group.Single(ConnectionId).OnMessage("Hello, world! to me");
```

## 구현 예
다음은 단일 그룹을 미리 생성해 두고, 그 그룹에 사용자를 추가/삭제, 메시지 배포하는 예입니다.

```csharp
public class GroupService(IMulticastGroupProvider groupProvider) : IDisposable
{
    // NOTE: You can also manage multiple groups using a dictionary, etc.
    private readonly IMulticastSyncGroup<Guid, IMyReceiver> _group = groupProvider.GetOrAddSynchronousGroup<Guid, IMyHubReceiver>();

    public void SendMessageToAll(string message) => _group.All.OnMessage(message);

    public void AddMember(Guid id, IMyHubReceiver receiver) => _group.Add(receiver);
    public void RemoveMember(Guid id) => _group.Remove(id);

    public void Dispose() => _group.Dispose();
}

public class MyBackgroundService(GroupService groupService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            groupService.SendMessageToAll("Send message periodically...");
        }
    }
}

...

builder.Services.AddSingleton<GroupService>();
builder.Services.AddHostedService<MyBackgroundService>();

...

public class MyHub(GroupService groupService) : StreamingHubBase<IMyHub, IMyHubReceiver>, IMyHub
{
    protected override ValueTask OnConnected()
    {
        groupService.AddMember(ContextId, Client);
        return default;
    }

    protected override ValueTask OnDisconnected()
    {
        groupService.RemoveMember(ContextId);
        return default;
    }

    public Task SendMessage(string message) => groupService.SendMessageToAll(message);
}
```

이 `GroupService`를 테스트하고 싶은 경우에는 인메모리의 `InMemoryGroupProvider`를 사용함으로써 MagicOnion과 관계없이 실행할 수 있습니다.

```csharp
[Fact]
public async Task TestGroupService()
{
    // Arrange
    var groupProvider = new InMemoryGroupProvider(DynamicInMemoryProxyFactory.Instance);
    var groupService = new GroupService(groupProvider);
    var receiver = Substitute.For<IMyReceiver>(); // Use NSubstitute, Moq, etc.

    // Act
    groupService.AddMember(Guid.NewGuid(), receiver);
    groupService.SendMessageToAll("Hello, world!");

    await Task.Delay(100); // Wait for message delivery.

    // Assert
    receiver.Received().OnMessage("Hello, world!");
}
```

## 주의사항
### 그룹에서 사용하는 클라이언트의 타입
그룹에서 사용하는 클라이언트의 타입은 StreamingHub의 Receiver와 같게 하는 것을 강력히 추천합니다. 이는 그룹의 내부에서 메시지를 송신할 때 StreamingHub의 Client를 특별히 취급하여 효율적으로 배포하는 구조가 있기 때문입니다.

### 그룹의 생존 기간(Lifecycle)
애플리케이션에서 그룹을 생성한 경우는 그룹이 불필요하게 된 시점에서 명시적으로 삭제할 필요가 있습니다. 그룹을 삭제하지 않는 경우, group provider가 그룹을 계속 보유하기 때문에 메모리 누수가 발생합니다.

MagicOnion이 관리하는 그룹은 그룹에서 모든 사용자가 삭제된 시점에서 삭제되지만, 애플리케이션이 관리하는 그룹의 생존 기간은 애플리케이션 측이 판단할 필요가 있습니다.
