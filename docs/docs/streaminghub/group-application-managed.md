# Application manged Group

:::tip
This feature was added in MagicOnion v7.0.0. https://github.com/Cysharp/MagicOnion/pull/778
:::

In game-like applications, there are cases where you want to control groups in detail according to the application logic.

For example, in a game where there are elements such as battlefields, participating players, and teams, all players connect to the same battlefield, but each team needs a communication channel, or in a case where players change teams during a single match, etc. There are cases where you want to create and manage groups on the application logic side even when there are no participants yet, such as in a metaverse space.

Until v7, creating groups was only possible within StreamingHub, but from v7, it is possible to create groups on the application side using the [Multicaster](https://github.com/Cysharp/Multicaster) library as a foundation.

Also, this allows you to create groups without depending on StreamingHub or MagicOnion itself, making it easier to test your logic.

## Creating/Getting a group
To create a group, use the `IMulticastGroupProvider` interface (group provider) obtained through Dependency Injection. The implementation of this interface is already registered by MagicOnion, so it can be used from classes registered in the DI container such as StreamingHub, Unary services, and other Hosted Services.

`GetOrAddSynchronousGroup` or `GetOrAddAsynchronousGroup` creates or retrieves a group by specifying the key type that distinguishes the client (e.g. `Guid` or an application-specific user ID) and the type of the client's interface (often the receiver of StreamingHub), and the group name. If there is no group with the specified name in the group provider, a new group will be created.

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
`GetOrAddSynchronousGroup` is suitable when group operations can be processed synchronously, such as when processing groups only in memory. `GetOrAddAsynchronousGroup` is suitable when group operations require asynchronous processing, such as when distributing groups across multiple MagicOnion servers using Redis.
:::

## Deleting a group

To delete a group, call the `IGroup<T>.Dispose` method. This means that the group will be removed from the group provider. After that, you can create a new group by calling `GetOrAddSynchronousGroup` or `GetOrAddAsynchronousGroup` again.

## Registering clients with a group

To register a client with a group, call the `Add` or `AddAsync` method. This method takes a key that distinguishes the client and the client as arguments. If the interface of the client is the same as the receiver of StreamingHub, you can pass StreamingHub's `Client` property as the client to be registered.

```csharp
_group.Add(ConnectionId, Client);
```

## Removing clients from a group

To remove a client from a group, use the `Remove` or `RemoveAsync` method. This method takes the key that distinguishes the client as an argument.

```csharp
_group.Remove(ConnectionId);
```

Unlike StreamingHub's groups, the group is not destroyed even if there are no clients in the group.

## Sending messages to a group
Sending messages to a group is similar to the groups provided by MagicOnion's SteramingHub, and you can determine the destination by using `All`, `Except`, `Single`, etc.

```csharp
_group.All.OnMessage("Hello, world!");

_group.Only([connectionId1, connectionId2]).OnMessage("Hello, world! to specific clients");

_group.Except(ConnectionId).OnMessage("Hello, world! except me");

_group.Single(ConnectionId).OnMessage("Hello, world! to me");
```

## Implementation example
The following is an example of creating a single group in advance, adding/removing users to the group, and delivering messages.

```csharp
public class GroupService(IMulticastGroupProvider groupProvider) : IDisposable
{
    // NOTE: You can also manage multiple groups using a dictionary, etc.
    private readonly IMulticastSyncGroup<Guid, IMyReceiver> _group = groupProvider.GetOrAddSynchronousGroup<Guid, IMyHubReceiver>("MyGroup");

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

If you want to test `GroupService`, you can use the in-memory `InMemoryGroupProvider` to run the test without MagicOnion.

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

## Notes
### Type of client used in the group
It is strongly recommended to use the same type of client as the StreamingHub's Receiver when using the client in the group. This is because there is a mechanism to efficiently deliver messages within the group by treating the StreamingHub's Client specially when sending messages within the group.

### Lifetime of the group
If you create a group in the application, you need to explicitly delete the group when it is no longer needed. If you do not delete the group, the group provider will continue to hold the group, causing a memory leak.

MagicOnion managed groups are deleted when all users are removed from the group, but the lifetime of the groups managed by the application must be determined by the application.
