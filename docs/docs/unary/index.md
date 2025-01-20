# Unary service fundamentals
A Unary service is a mechanism that provides a request/response API in the style of RPC or Web-API, and is implemented as a Unary call to gRPC.

A Unary service can be defined as a C# interface to benefit from the type. This means that it can be observed as a request over HTTP/2.

## Service definition (Shared library)
You can define a service interface in a shared library that is shared between the server and the client.

```csharp
using System;
using MagicOnion;

namespace MyApp.Shared
{
    // Defines .NET interface as a Server/Client IDL.
    // The interface is shared between server and client.
    public interface IMyFirstService : IService<IMyFirstService>
    {
        // The return type must be `UnaryResult<T>` or `UnaryResult`.
        UnaryResult<int> SumAsync(int x, int y);
        // `UnaryResult` does not have a return value like `Task`, `ValueTask`, or `void`.
        UnaryResult DoWorkAsync();
    }
}
```

## Service implementation (Server-side)
The service implementation is a class that inherits `ServiceBase<T>` and implements the service interface.

```csharp
using System;
using MagicOnion;
using MagicOnion.Server;
using MyApp.Shared;

namespace MyApp.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    // `UnaryResult<T>` allows the method to be treated as `async` method.
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        Console.WriteLine($"Received:{x}, {y}");
        return x + y;
    }

    public async UnaryResult DoWorkAsync()
    {
        // Something to do ...
    }
}
```

In MagicOnion, unlike gRPC in general, the body of the request is serialized by MessagePack for sending and receiving.
