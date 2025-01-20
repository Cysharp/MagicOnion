# StreamingHub service fundamentals
TBW

StreamingHub is a mechanism for real-time communication in an RPC style between the server and the client.

StreamingHub supports not only calls from the client to the server, but also calls from the server to the client. For example, this is used for receiving messages in a chat app, and synchronizing player position information in real-time game.

## Usage

- Define a StreamingHub interface in a shared library that is shared between the server and the client.
- Implement the StreamingHub interface in the server project.
- Implement the StreamingHub receiver in the client project.
- Create a client proxy to call the StreamingHub.

### Define StreamingHub interface in a shared library

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

### Implement StreamingHub in a server project

The StreamingHub implementation must inherit `StreamingHubBase<THub, TReceiver>` and implement the StreamingHub interface.

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
        room.All.OnLeave(Context.ConnectionId);
        await room.RemoveAsync(Context);
    }

    public async ValueTask SendMessageAsync(string message)
    {
        room.All.OnMessage(userName, message);
    }
}
```

## Implement StreamingHub receiver in a client project

The StreamingHub receiver will receive messages from the server.

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

### Create a client proxy to connect the StreamingHub

To connect to the StreamingHub, you need to create a client proxy using the `StreamingHubClient.ConnectAsync` method.

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var receiver = new ChatHubReceiver();
var client = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver);

await client.JoinAsync("room", "user1");
await client.SendMessageAsync("Hello, world!");
```


## Example

The following code is a simple example of a game implementation using StreamingHub and Unity.

```csharp
// Server -> Client definition
public interface IGamingHubReceiver
{
    // The method must have a return type of `void` and can have up to 15 parameters of any type.
    void OnJoin(Player player);
    void OnLeave(Player player);
    void OnMove(Player player);
}

// Client -> Server definition
// implements `IStreamingHub<TSelf, TReceiver>`  and share this type between server and client.
public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
{
    // The method must return `ValueTask`, `ValueTask<T>`, `Task` or `Task<T>` and can have up to 15 parameters of any type.
    ValueTask<Player[]> JoinAsync(string roomName, string userName, Vector3 position, Quaternion rotation);
    ValueTask LeaveAsync();
    ValueTask MoveAsync(Vector3 position, Quaternion rotation);
}

// for example, request object by MessagePack.
[MessagePackObject]
public class Player
{
    [Key(0)]
    public string Name { get; set; }
    [Key(1)]
    public Vector3 Position { get; set; }
    [Key(2)]
    public Quaternion Rotation { get; set; }
}
```

```csharp
// Server implementation
// implements : StreamingHubBase<THub, TReceiver>, THub
public class GamingHub : StreamingHubBase<IGamingHub, IGamingHubReceiver>, IGamingHub
{
    // this class is instantiated per connected so fields are cache area of connection.
    IGroup room;
    Player self;
    IInMemoryStorage<Player> storage;

    public async ValueTask<Player[]> JoinAsync(string roomName, string userName, Vector3 position, Quaternion rotation)
    {
        self = new Player() { Name = userName, Position = position, Rotation = rotation };

        // Group can bundle many connections and it has inmemory-storage so add any type per group.
        (room, storage) = await Group.AddAsync(roomName, self);

        // Typed Server->Client broadcast.
        Broadcast(room).OnJoin(self);

        return storage.AllValues.ToArray();
    }

    public async ValueTask LeaveAsync()
    {
        await room.RemoveAsync(this.Context);
        Broadcast(room).OnLeave(self);
    }

    public async ValueTask MoveAsync(Vector3 position, Quaternion rotation)
    {
        self.Position = position;
        self.Rotation = rotation;
        Broadcast(room).OnMove(self);
    }

    // You can hook OnConnecting/OnDisconnected by override.
    protected override ValueTask OnDisconnected()
    {
        // on disconnecting, if automatically removed this connection from group.
        return ValueTask.CompletedTask;
    }
}
```

You can write client like this.

```csharp
public class GamingHubClient : IGamingHubReceiver
{
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    IGamingHub client;

    public async ValueTask<GameObject> ConnectAsync(ChannelBase grpcChannel, string roomName, string playerName)
    {
        this.client = await StreamingHubClient.ConnectAsync<IGamingHub, IGamingHubReceiver>(grpcChannel, this);

        var roomPlayers = await client.JoinAsync(roomName, playerName, Vector3.zero, Quaternion.identity);
        foreach (var player in roomPlayers)
        {
            (this as IGamingHubReceiver).OnJoin(player);
        }

        return players[playerName];
    }

    // methods send to server.

    public ValueTask LeaveAsync()
    {
        return client.LeaveAsync();
    }

    public ValueTask MoveAsync(Vector3 position, Quaternion rotation)
    {
        return client.MoveAsync(position, rotation);
    }

    // dispose client-connection before channel.ShutDownAsync is important!
    public Task DisposeAsync()
    {
        return client.DisposeAsync();
    }

    // You can watch connection state, use this for retry etc.
    public Task WaitForDisconnect()
    {
        return client.WaitForDisconnect();
    }

    // Receivers of message from server.

    void IGamingHubReceiver.OnJoin(Player player)
    {
        Debug.Log("Join Player:" + player.Name);

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = player.Name;
        cube.transform.SetPositionAndRotation(player.Position, player.Rotation);
        players[player.Name] = cube;
    }

    void IGamingHubReceiver.OnLeave(Player player)
    {
        Debug.Log("Leave Player:" + player.Name);

        if (players.TryGetValue(player.Name, out var cube))
        {
            GameObject.Destroy(cube);
        }
    }

    void IGamingHubReceiver.OnMove(Player player)
    {
        Debug.Log("Move Player:" + player.Name);

        if (players.TryGetValue(player.Name, out var cube))
        {
            cube.transform.SetPositionAndRotation(player.Position, player.Rotation);
        }
    }
}
```
