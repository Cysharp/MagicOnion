# Native AOT Support

MagicOnion supports .NET Native AOT publishing through the `MagicOnion.Server.SourceGenerator` package. This allows you to publish your MagicOnion server applications as native executables with faster startup times and smaller memory footprint.

## Overview

By default, MagicOnion uses reflection to discover services and generate method handlers at runtime. This approach is incompatible with Native AOT because:

1. Assembly scanning requires runtime reflection
2. `Expression.Compile()` is not supported in AOT
3. `Activator.CreateInstance()` with generic types requires runtime code generation

The `MagicOnion.Server.SourceGenerator` solves these issues by generating all necessary code at compile time.

## Getting Started

### 1. Install the Source Generator

Add the `MagicOnion.Server.SourceGenerator` package to your server project:

```xml
<ItemGroup>
  <PackageReference Include="MagicOnion.Server.SourceGenerator" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2. Create a Method Provider Class

Create a partial class decorated with `[MagicOnionServerGeneration]`:

```csharp
using MagicOnion;

// Option 1: Explicit service types
[MagicOnionServerGeneration(typeof(GreeterService), typeof(ChatHub))]
public partial class MagicOnionMethodProvider { }

// Option 2: Auto-discover all services in the compilation
[MagicOnionServerGeneration]
public partial class MagicOnionMethodProvider { }
```

### 3. Configure the Server

Use the `UseStaticMethodProvider` extension method to register the generated provider:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMagicOnion()
    .UseStaticMethodProvider<MagicOnionMethodProvider>();

var app = builder.Build();

app.MapMagicOnionService([typeof(GreeterService), typeof(ChatHub)]);

app.Run();
```

## StreamingHub Broadcast Support (Multicaster)

If your application uses StreamingHub with broadcast features (like `Broadcast.All.OnMessage(...)`), you also need to configure the Multicaster proxy factory for AOT.

### 1. Install Multicaster.SourceGenerator

Add the `Multicaster.SourceGenerator` package:

```xml
<ItemGroup>
  <PackageReference Include="Multicaster.SourceGenerator" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2. Create a Proxy Factory Class

Create a partial class decorated with `[MulticasterProxyGeneration]` that lists all your StreamingHub receiver interfaces:

```csharp
using Cysharp.Runtime.Multicast;

[MulticasterProxyGeneration(typeof(IChatHubReceiver), typeof(IGameHubReceiver))]
public partial class MulticasterProxyFactory { }
```

### 3. Configure Both Providers

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMagicOnion()
    .UseStaticMethodProvider<MagicOnionMethodProvider>()
    .UseStaticProxyFactory<MulticasterProxyFactory>();  // Add this for StreamingHub broadcast

var app = builder.Build();

app.MapMagicOnionService([typeof(GreeterService), typeof(ChatHub)]);

app.Run();
```

### Complete AOT Example

Here's a complete example for a chat application:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMagicOnion()
    .UseStaticMethodProvider<MagicOnionMethodProvider>()
    .UseStaticProxyFactory<MulticasterProxyFactory>();

var app = builder.Build();
app.MapMagicOnionService([typeof(ChatHub)]);
app.Run();

// MagicOnionMethodProvider.cs
[MagicOnionServerGeneration(typeof(ChatHub))]
public partial class MagicOnionMethodProvider { }

// MulticasterProxyFactory.cs
[MulticasterProxyGeneration(typeof(IChatHubReceiver))]
public partial class MulticasterProxyFactory { }

// ChatHub.cs
public interface IChatHubReceiver
{
    void OnMessage(string user, string message);
    void OnUserJoined(string user);
}

public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
{
    Task JoinAsync(string roomName, string userName);
    Task SendMessageAsync(string message);
}

public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "";

    public async Task JoinAsync(string roomName, string userName)
    {
        this.userName = userName;
        room = await Group.AddAsync(roomName);
        Broadcast(room).OnUserJoined(userName);
    }

    public Task SendMessageAsync(string message)
    {
        Broadcast(room!).OnMessage(userName, message);
        return Task.CompletedTask;
    }
}
```

### 4. Enable AOT Publishing

Add the following properties to your project file:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

## How It Works

The source generator analyzes your service implementations at compile time and generates:

1. **Static method lists** - Pre-computed lists of `IMagicOnionGrpcMethod` instances
2. **Static invokers** - Lambda expressions that call service methods directly
3. **Hub method registrations** - Pre-computed StreamingHub method handlers

This eliminates all runtime reflection and dynamic code generation.

## Generated Code Example

For a service like:

```csharp
public interface IGreeterService : IService<IGreeterService>
{
    UnaryResult<string> SayHello(string name);
}

public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public UnaryResult<string> SayHello(string name)
        => UnaryResult.FromResult($"Hello, {name}!");
}
```

The generator produces:

```csharp
partial class MagicOnionMethodProvider : IMagicOnionGrpcMethodProvider
{
    public void MapAllSupportedServiceTypes(MagicOnionGrpcServiceMappingContext context)
    {
        context.Map<GreeterService>();
    }

    public IReadOnlyList<IMagicOnionGrpcMethod> GetGrpcMethods<TService>() where TService : class
    {
        if (typeof(TService) == typeof(GreeterService))
            return __GreeterService_ServiceMethods.Methods;
        return Array.Empty<IMagicOnionGrpcMethod>();
    }
    
    // ...
}

file static class __GreeterService_ServiceMethods
{
    public static readonly IReadOnlyList<IMagicOnionGrpcMethod> Methods = new IMagicOnionGrpcMethod[]
    {
        new MagicOnionUnaryMethod<GreeterService, string, string, Box<string>, Box<string>>(
            "IGreeterService", 
            "SayHello", 
            static (instance, context, request) => instance.SayHello(request)),
    };
}
```

## Limitations

- All service types must be known at compile time
- Generic service implementations are not supported
- Dynamic service registration is not available

## Serialization

Ensure your serializer also supports AOT:

- **MessagePack**: Use `MessagePackSerializer` with source-generated formatters
- **MemoryPack**: Fully AOT-compatible by design

## Troubleshooting

### "No service implementation found"

Ensure your service classes:
- Are non-abstract
- Implement `IService<T>` or `IStreamingHub<T, TReceiver>`
- Are included in the `[MagicOnionServerGeneration]` attribute or are in the same compilation

### Trimming Warnings

If you see trimming warnings, ensure all types used in service methods are preserved. You may need to add `[DynamicallyAccessedMembers]` attributes or use `rd.xml` files.
