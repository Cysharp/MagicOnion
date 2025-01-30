# Group fundamentals

StreamingHub provides a mechanism for delivering messages to multiple clients. This is used, for example, to deliver text messages received in a chat to clients.

The mechanism for managing the targets to deliver messages to on the server side is called a group. A group can be created with an arbitrary name, and messages can be sent to clients belonging to that group. This like a channel or room in a chat.

![](/img/docs/fig-group-broadcast.png)

## Creating/Getting a group and adding clients

A group can be obtained by calling the `AddAsync` method of the `Group` property of StreamingHub with the group name. If the group does not exist at the time of the call, it will be created.

```csharp
public async ValueTask JoinAsync(string userName, string roomName)
{
    // Add client to the group with the specified group name.
    // If the group does not exist, it will be created.
    this.room = await Group.AddAsync(roomName);
    // ...
}
```

## Sending messages to clients belonging to a group
A group instance provides a proxy for the receiver, allowing messages to be broadcast to clients belonging to the group. In StreamingHub app development, this instance is kept as a field and called as needed.

```csharp
// Send a message "Hello, world!" to all clients in the room
this.room.All.OnMessage("Hello, world!");
```

You can also get a proxy that limits the recipients, such as specific clients, in addition to all clients in the room

```csharp
this.room.Only([connectionId1, connectionId2]).OnMessage("Hello, world! to specific clients");

this.room.Except(ConnectionId).OnMessage("Hello, world! except me");

this.room.Single(ConnectionId).OnMessage("Hello, world! to me");
```

- `All`: All clients in the group
- `Single`: A specific client
- `Only`: Specific clients (multiple)
- `Except`: All clients except specific clients (multiple)


## Removing clients from a group
To remove a client from a group, use the `RemoveAsync` method.

```csharp
public async ValueTask LeaveAsync()
{
    // Remove the client from the group
    await this.room.RemoveAsync(Context);
}
```

If the client is disconnected from the server, it will be automatically removed from the group, so there is no need to explicitly remove it. Also, the group will be deleted when there are no clients in the group.

## More fine-grained group control
Groups are managed and controlled by StreamingHub. The groups described here are associated with StreamingHub and managed by it. This means that the creation of groups and the management of clients included in them must be done through the Hub.


On the other hand, in applications such as games, you may want to create and delete groups and manage clients on the application logic side. In that case, you can manage groups on the application side without using Hub groups. For more information, see [Group management by application](group-application-managed).

## Thread safety
Groups are thread-safe and can be safely operated on by multiple clients simultaneously. However, the consistency of group instance creation and deletion must be guaranteed by the application.

For example, if the Remove of the last user and the Add of a new user are executed almost simultaneously in a group associated with StreamingHub, the group will be deleted after being deleted once and a new group will be created. This behavior may be a problem if you are holding a group.

When strict consistency is required for group management and message delivery due to the increase and decrease of clients in the group and message delivery, consider managing groups and locks on the application side.
