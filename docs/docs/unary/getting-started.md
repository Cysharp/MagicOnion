# Getting Started with Unary

This tutorial introduces the simple steps to implement a Unary service.

## Steps to implement a Unary service

The following steps are required to define, implement, and use a Unary service.

- Define a Unary service interface to be shared between the server and client
- Implement the Unary service interface defined in the server project
- Call the defined Unary service from the client project

## Define a Unary service interface to be shared between the server and client

Define a Unary service interface in a shared library project (in the case of Unity, use source code copy or file link). The Unary service interface must inherit `IService<TSelf>`.

The following is an example of a Unary service interface that returns a greeting.

```csharp
public interface IGreeterService : IService<IGreeterService>
{
    UnaryResult<string> SayHelloAsync(string name, int age);
}
```

The return type of the method defined in the Unary service must be `UnaryResult` or `UnaryResult<T>`. This is a return type specific to Unary services that has a similar meaning to `ValueTask`, `ValueTask<T>`.

## Implement the Unary service interface defined in the server project

Implement the Unary service called by the client on the server. The server implementation must inherit `ServiceBase<TSelf>` and implement the defined Unary service interface.

```csharp
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public async UnaryResult<string> SayHelloAsync(string name, int age)
    {
        return $"Hello {name}! Your age is {age}.";
    }
}
```

`UnaryResult` and `UnaryResult<T>` types can be defined as asynchronous methods (`async`) like `ValueTask`, `Task`, etc.

If the method does not require asynchronous processing, you can also return synchronously using `UnaryResult.FromResult` or `UnaryResult.CompletedResult`.

```csharp
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public UnaryResult<string> SayHelloAsync(string name, int age)
    {
        return UnaryResult.FromResult($"Hello {name}! Your age is {age}.");
    }
}
```

## Call the defined Unary service from the client project

To call the Unary service method from the client, use the `MagicOnionClient.Create<T>` method.

`Create<T>` method takes a Unary service interface and a `GrpcChannel` object as the destination. This method generates a client proxy corresponding to the specified interface. At this point, no request is sent to the server.


```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = await MagicOnionClient.Create<IGreeterService>(channel, receiver);
```

Call the method of the Unary service using the generated client proxy.

```csharp
var result = await client.SayHelloAsync("Alice", 18);
Console.WriteLine(result);
```
