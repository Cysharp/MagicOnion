# Server-side connection and disconnection events

On the server side, events occur when a client connects or disconnects from the StreamingHub. These events can be handled by overriding the event methods of the `StreamingHubBase` class.

## `OnConnecting` method
The `OnConnecting` method is called when a client is establishing a connection to the StreamingHub. At this point, the connection with the client has not been established, so you can not perform operations on the client or group.

```csharp
protected override async ValueTask OnConnecting()
{
    // Processing while the client is establishing a connection

    // Example: Initialization of StreamingHub itself
}
```

## `OnConnected` method
The `OnConnected` method is called when a client has completed connecting to the StreamingHub. At this point, the connection with the client has been established, and you can call the client or perform operations on the group.

```csharp
protected override async ValueTask OnConnected()
{
    // Processing when the client has completed connecting

    // Example: Adding to a group or sending initial state
    // this.group = await Group.AddAsync("MyGroup");
    // Client.OnConnected(initialState);
    // ..
}
```


## `OnDisconnected` method
The `OnDisconnected` method is called when a client is disconnected from the StreamingHub. At this point, the connection with the client has been disconnected, so operations related to the client are invalid.

```csharp
protected override async ValueTask OnDisconnected()
{
    // Processing when the client is disconnected
}
```

For related operations and information about disconnection, see [Handling disconnection](disconnection).
