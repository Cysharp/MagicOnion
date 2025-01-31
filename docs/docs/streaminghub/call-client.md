# Send messages to the client

StreamingHub can send messages from the server to the client (receiver). This message is sent by calling the receiver interface method (receiver method) on the server.

There are two main ways to send messages. One is to call a client connected to the StreamingHub instance, and the other is to send messages to clients belonging to a group.

## Send messages to the client connected to the StreamingHub instance

To send a message to the client connected to the StreamingHub instance, use the `Client` property. This property provides a proxy for the client that implements the receiver interface.

```csharp
public async Task EchoAsync(string message)
{
    this.Client.OnMessage("Echo: " + message);
}
```

## Send messages to clients belonging to a group

To send a message to clients belonging to a group, you need to get or create a group to get an instance of the group. For sending using group, see [Group fundamentals](group).
