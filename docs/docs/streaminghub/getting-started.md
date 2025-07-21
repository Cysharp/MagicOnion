# Getting Started with StreamingHub

This tutorial introduces the basic steps to get started with StreamingHub.

## Steps

To define, implement, and use StreamingHub, the following steps are required:

- Define the StreamingHub interface to be shared between the server and the client
- Implement the StreamingHub interface in the server project
- Implement the StreamingHub receiver defined in the client project
- Create a client proxy to call the StreamingHub defined in the client project

## Define the StreamingHub interface to be shared between the server and the client

Define the StreamingHub interface in a shared library project.

The StreamingHub interface must inherit `IStreamingHub<TSelf, TReceiver>`. `TSelf` is the interface itself, and `TReceiver` is the receiver interface for sending and receiving messages from the server to the client.

The following is an example of a StreamingHub interface for a chat application. The client has a receiver interface that sends join, leave, and message events.

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
    void OnMessage(string userName, string message);
}
```

The methods provided by StreamingHub are called **Hub methods**. Hub methods are methods called by the client, and the return type must be `ValueTask`, `ValueTask<T>`, `Task`, `Task<T>`, or `void`. Note that this is different from Unary services.

The receiver interface, which is the client's message receiving interface, also has methods. These are called **receiver methods**. Receiver methods are called when a message is received from the server. The return value of the receiver method must be `void`. Unless you are using [client results](client-results), you should specify `void` as the return value.

## Implement StreamingHub in the server project

To call StreamingHub from the client, you need to implement a StreamingHub that can be called from the client on the server. The server implementation must inherit `StreamingHubBase<THub, TReceiver>` and implement the defined StreamingHub interface.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    public async ValueTask JoinAsync(string roomName, string userName)
        => throw new NotImplementedException();

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

At first, implement the method `JoinAsync` to join the chat room. This method joins the room with the specified name and user name.

Use the `Group.AddAsync` method to create a group and store a reference to the group in the StreamingHub for later use. You can get the receiver interface of the clients who have joined the group through the `All` property of the group, so call the `OnJoin` method to notify the join.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

`Client` property can be used to call only the clients connected to that SteramingHub. Here, let's send a welcome message only to the clients who have connected.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);

        Client.OnMessage("System", $"Welcome, hello {userName}!");
    }

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

Next, implement the method `LeaveAsync` to exit. When a client exits, the client is removed from the group. This is done using the `Group.RemoveAsync` method. Pass the `StreamingHubContext` object (`Context` property) to the `RemoveAsync` method. When a client is removed from the group, messages through the group will no longer reach that client.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
    {
        room.All.OnLeave(ConnectionId.toString());
        await room.RemoveAsync(Context);
    }

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

Finally, implement the `SendMessageAsync` method to deliver messages to the group when the server receives a message from the client. In this method, notify the group by calling the `OnMessage` method of the clients who have joined the group through the `All` property of the group.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
    {
        room.All.OnLeave(ConnectionId.toString());
        await room.RemoveAsync(Context);
    }

    public async ValueTask SendMessageAsync(string message)
    {
        room.All.OnMessage(userName, message);
    }
}
```

## Implement the StreamingHub receiver in the client project

In the client project, implement the receiver interface of StreamingHub. This interface is implemented in the client project and processes the messages received on the client side.

Create a simple `ChatHubReceiver` type and implement the receiver interface `IChatHubReceiver`. Each method receives a message sent from the server and outputs the message to the console.

```csharp
class ChatHubReceiver : IChatHubReceiver
{
    public void OnJoin(string userName)
        => Console.WriteLine($"{userName} joined.");
    public void OnLeave(string userName)
        => Console.WriteLine($"{userName} left.");
    public void OnMessage(string userName, string message)
        => Console.WriteLine($"{userName}: {message}");
}
```

## Create a client proxy to call the StreamingHub defined in the client project

To connect from the client to the StreamingHub, use the `StreamingHubClient.ConnectAsync` method. This method establishes a connection and returns a client proxy.

`ConnectAsync` method takes a `GrpcChannel` object for the connection destination and an instance of the receiver interface. Messages received from the server will be method calls on the instance of the receiver passed here.

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var receiver = new ChatHubReceiver();
var client = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver);

await client.JoinAsync("room", "user1");
await client.SendMessageAsync("Hello, world!");
```
