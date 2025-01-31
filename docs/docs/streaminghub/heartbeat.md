# Heartbeat

:::tip
The feature is added in MagicOnion v7.0.0.
:::

The heartbeat feature allows for maintaining connection and early detection of disconnections by sending messages periodically from the server to client and client to server. The heartbeat can have a specified timeout to disconnect if the target does not respond within a certain time frame.

## Why not use the HTTP/2 PING frame?

HTTP/2 has a mechanism called the PING frame for keep-alive. But MagicOnion has its own heartbeat feature.

This is because when a load balancer is included in the network configuration, the load balancer may process the PING frame and not reach the MagicOnion server.

```plaintext
[Client] ← PING/PONG → [LoadBalancer] ← PING/PONG → [Server]
```

MagicOnion provides a heartbeat mechanism that explicitly sends and receives data between the server and client for communication confirmation in cases like this.

## Client
The client heartbeat can be specified when creating the `StreamingHubClient`. It is disabled by default.

```csharp
// Send a message to the server every 30 seconds
var options = StreamingHubClientOptions.CreateWithDefault().WithClientHeartbeatInterval(TimeSpan.FromSeconds(30));
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

### API
```csharp
public class StreamingHubClientOptions
{
    /// <summary>
    /// Sets a heartbeat interval. If a value is <see keyword="null"/>, the heartbeat from the client is disabled.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatInterval(TimeSpan? interval);

    /// <summary>
    /// Sets a heartbeat timeout period. If a value is <see keyword="null"/>, the client does not time out.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatTimeout(TimeSpan? timeout);

    /// <summary>
    /// Sets a heartbeat callback. If additional metadata is provided by the server in the heartbeat message, this metadata is provided as an argument.
    /// </summary>
    /// <param name="onServerHeartbeatReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithServerHeartbeatReceived(Action<ServerHeartbeatEvent>? onServerHeartbeatReceived);

    /// <summary>
    /// Sets a client heartbeat response callback.
    /// </summary>
    /// <param name="onClientHeartbeatResponseReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatResponseReceived(Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived);
}
```


## Server
The server-side heartbeat can be enabled globally through `MagicOnionOptions` or via the `Heartbeat` attribute. It is disabled by default.

```csharp
// Enable heartbeat for all StreamingHub instances
options.EnableStreamingHubHeartbeat = true;
// Send heartbeat every 30 seconds, disconnect if no response within 5 seconds
options.StreamingHubHeartbeatInterval = TimeSpan.FromSeconds(30);
options.StreamingHubHeartbeatTimeout = TimeSpan.FromSeconds(5);
```

```csharp
// Enable heartbeat using the interval and timeout specified in MagicOnionOptions
[Heartbeat]
public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>
{
}

// Enable heartbeat and override the interval and timeout specified in MagicOnionOptions
[Heartbeat(Interval = 10 * 1000, Timeout = 1000)]
public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>
{
}
```

The server-side heartbeat has the “time the server sent the heartbeat” set in the `ServerTime` property. Clients can use this to synchronize the client and server times. `ServerTime` is always UTC.

```csharp
// Client-side code:
var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x =>
{
    var serverTime = x.ServerTime; // ServerTime is always UTC.
});
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

## Additional Metadata
Additional metadata can be added to the heartbeat messages from the server. This can be used, for example, to include the server information for synchronization purposes.

### Server
To add metadata to the server's heartbeat messages, implement the `IStreamingHubHeartbeatMetadataProvider` interface and register it with the DI container or specify it in the `MetadataProvider` property of the `Heartbeat` attribute.

```csharp
public class CustomHeartbeatMetadataProvider : IStreamingHubHeartbeatMetadataProvider
{
    public bool TryWriteMetadata(IBufferWriter<byte> writer)
    {
        MessagePackSerializer.Serialize(writer, new Version(1, 0, 0, 0));
        return true;
    }
}
```
### Client
On the client side, a callback for the heartbeat can be set as an option when creating the StreamingHubClient.

```csharp
var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x =>
{
    var serverVersion = MessagePackSerializer.Deserialize<Version>(x.Metadata);
});
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```


## Advanced Operations on the Server Side Heartbeat
You can access some advanced features of the server-side heartbeat through the `IMagicOnionHeartbeatFeature` interface. The implementation of this interface can be obtained from `IHttpContext.Features` (`Context.CallContext.GetHttpContext().Features`).

### `Unregister` method
`Unregister` method disables the heartbeat for the client connection of that StreamingHub. This can be used when you need to temporarily disable the heartbeat for debugging purposes.

### `SetAckCallback` method
You can set a callback when the server receives a heartbeat response from the client.

### `Latency` property
The `Latency` property gets the latency between the server and client. If no messages have been sent or received, `TimeSpan.Zero` is returned.


## Limitations

Heartbeat is not compatible with clients and servers before v7.0.0. Do not enable it in StreamingHub where there may be a mix of versions before and after v7.0.0.
