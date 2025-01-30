# Quick Start
This guide shows how to create a simple MagicOnion server and client. The server provides a simple service that adds two numbers, and the client calls the service to get the result.

MagicOnion provides RPC services like Web API and StreamingHub for real-time communication. This section implements an RPC service like Web API.

## Server-side: Defining and Implementing a Service

At first, create a MagicOnion server project and define and implement a service interface.

### 1. Setting up a gRPC server project for MagicOnion

To start with a Minimal API project (see: [Tutorial: Create a minimal web API with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api)), create a project from the **ASP.NET Core Empty** template. Add the NuGet package `MagicOnion.Server` to the project. If you are using the .NET CLI tool to add it, run the following command:

```bash
dotnet add package MagicOnion.Server
```

Open `Program.cs` and add some method calls to `Services` and `app`.

```csharp
using MagicOnion;
using MagicOnion.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMagicOnion(); // Add this line(MagicOnion.Server)

var app = builder.Build();

app.MapMagicOnionService(); // Add this line

app.Run();
```

At this point, you are ready to use MagicOnion in your server project.

### 2. Implementing a Unary Service

Add the `IMyFirstService` interface to share it between the server and the client. In this case, the namespace that contains the shared interface is `MyApp.Shared`.

The return type must be `UnaryResult<T>` or `UnaryResult`, which is treated as an asynchronous method like `Task` or `ValueTask`.

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
    }
}
```

Add a class that implements the `IMyFirstService` interface. The client calls this class to process the request.

```csharp
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
}
```

The service definition and implementation are now complete.

It is now ready to start the MagicOnion server. You can start the MagicOnion server by pressing the F5 key or using the `dotnet run` command. At this time, note the URL displayed when the server starts, as it will be the connection destination for the client.

## Client-side: Calling a Unary Service

Create a **Console Application** project and add the NuGet package `MagicOnion.Client`.


Share the `IMyFirstService` interface and use it in the client. You can share the interface in various ways, such as file links, shared libraries, or copy & paste...

In the client code, create a client proxy using `MagicOnionClient` based on the shared interface and call the service transparently.

At first, create a gRPC channel. The gRPC channel abstracts the connection, and you can create it using the `GrpcChannel.ForAddress` method. Then, create a MagicOnion client proxy using the created channel.

```csharp
using Grpc.Net.Client;
using MagicOnion.Client;
using MyApp.Shared;

// Connect to the server using gRPC channel.
var channel = GrpcChannel.ForAddress("https://localhost:5001");

// Create a proxy to call the server transparently.
var client = MagicOnionClient.Create<IMyFirstService>(channel);

// Call the server-side method using the proxy.
var result = await client.SumAsync(123, 456);
Console.WriteLine($"Result: {result}");
```

:::tip
When using MagicOnion client in Unity applications, see also [Using in Unity](/installation/unity).
:::
