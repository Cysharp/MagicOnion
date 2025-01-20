# Handling disconnection

StreamingHub has a mechanism to detect disconnection on both the client and server sides. This is because StreamingHub establishes a continuous connection between the client and server to communicate, and it may be disconnected for some reason. MagicOnion provides a mechanism for the application to detect disconnection on both the server and client sides.

:::tip
Disconnection is detected at different timings on the server and client sides. This means that even if the client detects disconnection, the server may not yet recognize it. Therefore, it is important to detect disconnection on both the client and server sides.
:::

## Detecting disconnection on the server
When you want to detect disconnection from the StreamingHub on the server side, use the `OnDisconnected` method.

```csharp
protected override ValueTask OnDisconnected()
{
    return ValueTask.CompletedTask;
}
```

## Detecting disconnection on the client
When you want to detect disconnection from the StreamingHub on the client side, use the `WaitForDisconnected` or `WaitForDisconnectAsync` API.

These APIs return a `Task` that waits until the StreamingHub client is disconnected for some reason.

```csharp
using MagicOnion.Client;

var client = await StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, receiver);
_ = WaitForDisconnectEventAsync();

async Task WaitForDisconnectEventAsync()
{
    var reason = await client.WaitForDisconnectAsync();
    if (reason.Type != DisconnectionType.CompletedNormally)
    {
        ...
    }
}
```

### `WaitForDisconnectAsync` API
`WaitForDisconnectAsync` API is an updated version of the existing `WaitForDisconnected`, and it will be possible to receive the reason.

Unlike `WaitForDisconnect`, the new API is added as an extension method of the `IStreamingHubMarker` interface. This is to avoid breaking binary compatibility by changing the `IStreamingHub` interface.

### APIs
```csharp
namespace MagicOnion.Client
{
    public static class StreamingHubClientExtensions
    {
        /// <summary>
        /// Wait for the disconnection and return the reason.
        /// </summary>
        public static Task<DisconnectionReason> WaitForDisconnectAsync<TStreamingHub>(this TStreamingHub hub) where TStreamingHub : IStreamingHubMarker =>
    }

    /// <summary>
    /// Provides the reason for the StreamingHub disconnection.
    /// </summary>
    public readonly struct DisconnectionReason
    {
        /// <summary>
        /// Gets the type of StreamingHub disconnection.
        /// </summary>
        public DisconnectionType Type { get; }

        /// <summary>
        /// Gets the exception that caused the disconnection.
        /// </summary>
        public Exception? Exception { get; }
    }

    /// <summary>
    /// Defines the types of StreamingHub disconnection.
    /// </summary>
    public enum DisconnectionType
    {
        /// <summary>
        /// Disconnected after completing successfully.
        /// </summary>
        CompletedNormally = 0,

        /// <summary>
        /// Disconnected due to exception while reading messages.
        /// </summary>
        Faulted = 1,

        /// <summary>
        /// Disconnected due to reaching the heartbeat timeout.
        /// </summary>
        TimedOut = 2,
    }
}
```

## Improving disconnection detection
To achieve early detection of communication deterioration and disconnection, MagicOnion provides a heartbeat feature. For more information on the heartbeat feature, see [Heartbeat](heartbeat).
