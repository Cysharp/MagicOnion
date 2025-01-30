# Define method and interface

## Overview
StreamingHub is defined using .NET interfaces, similar to Unary services. The StreamingHub interface must inherit `IStreamingHub<TSelf, TReceiver>`. `TSelf` is the interface itself, and `TReceiver` is the receiver interface for sending and receiving messages from the server to the client.

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
    void OnSendMessage(string userName, string message);
}
```

StreamingHub provides methods called **Hub methods**. Hub methods are methods called by the client and must return `ValueTask`, `ValueTask<T>`, `Task`, `Task<T>`, or `void`. Note that this is different from Unary services.

Receiver interfaces also have methods. These are called **receiver methods**. Receiver methods are called when the server receives a message from the client. Receiver methods must return `void`, except when using [client results](client-results). In general, `void` must be specified.

## Serialization
Like Unary services, method arguments and return values are serialized by default using MessagePack. Therefore, the type must be marked as serializable by MessagePack or be configured to be serializable. Method arguments are limited to a maximum of 15.

## Inheritance
StreamingHub interfaces can be inherited. This is useful when multiple Hubs share common methods.

```csharp
public inteface ICommonHub
{
    ValueTask PingAsync();
}

public inteface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>, ICommonHub
{
    ValueTask JoinAsync(string roomName, string userName);
    ValueTask LeaveAsync();
    ValueTask SendMessageAsync(string message);
}
```

## Advanced

### `Ignore` attribute
The `Ignore` attribute can be used to prevent specific methods from being recognized as Hub methods.

### `MethodId` attribute
The `MethodId` attribute can be used to manually specify an ID to identify a method. Hub method IDs are calculated from the method name using FNV1A32, so this attribute should only be used for special cases such as when you want to use the original ID after changing the method name or when there is a conflict with an ID.
