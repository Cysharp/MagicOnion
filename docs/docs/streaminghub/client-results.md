# Client Results

:::tip
The feature is added in MagicOnion v7.0.0.
:::

"Client results" is inspired by [a feature of the same name implemented in SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-8.0#client-results).

In the previous version, StreamingHub could only send messages from the server to the client in a one-way manner (Fire-and-Forget), but with this feature, you can call a method of a specific client from the server's Hub or application logic and receive the result.

```csharp
interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
}

interface IMyHubReveiver
{
    // The Client results method is defined in the Receiver with a return type of Task or Task<T>
    Task<string> HelloAsync(string name, int age);

    // Regular broadcast method
    void OnMessage(string message);
}

// Client implementation
class MyHubReceiver : IMyHubReceiver
{
    public async Task<string> HelloAsync(string name, int age)
    {
        Console.WriteLine($"Hello from {name} ({age})");
        var result = await ReadInputAsync();
        return result;
    }
    public void OnMessage()
    {
        Console.WriteLine($"OnMessage: {message}");
    }
}
```

On the server, method calls can be made through `Client` or `Single` of the group, and the result can be received.

```csharp
var result = await Client.HelloAsync();
Console.WriteLine(result);
// or
var result2 = await _group.Single(clientId).HelloAsync();
Console.WriteLine(result2);
```

## Exceptions
If an error occurs on the client, an `RpcException` is thrown to the caller, and if the connection is disconnected or a timeout occurs, a `TaskCanceledException` (`OperationCanceledException`) is thrown.

## Timeout
The timeout for server-to-client calls is 5 seconds by default. If the timeout is exceeded, a `TaskCanceledException` (`OperationCanceledException`) is thrown to the caller. The default timeout can be set via the `MagicOnionOptions.ClientResultsDefaultTimeout` property.

To explicitly override the timeout per method call, specify a `CancellationToken` as a method argument and pass in the `CancellationToken` to timeout at any desired timing. Note that this cancellation does not propagate to the client; the client always receives `default(CancellationToken)`.

```csharp
interface IMyHubReceiver
{
    Task<string> DoLongWorkAsync(CancellationToken timeoutCancellationToken = default);
}
```
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var result = await Client.DoLongWorkAsync(cts.Token);
Console.WriteLine(result);
```

## Limitations
- Invoking calls to multiple clients is not supported. You must use either `Client` or `Single`.
- Client results are not supported when Redis or NAT is used as the group's backplane.
- If invoking a Hub method while a Client results method is called on the client side, it will cause a deadlock.
