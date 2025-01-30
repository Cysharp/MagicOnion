# Connect groups with Redis or NATS
You can connect groups between multiple server instances using Redis or NATS. This is equivalent to the backplane of SignalR.

By using this mechanism, you can send messages to clients belonging to a specific group regardless of the server instance. This allows you to build an architecture that can scale out the server.

```mermaid
flowchart TD
    subgraph Server Instance: 1
        C0[Client or app logic] -- Send message to the group A --> G1
        I1[Hub Instance] --> C1[Client]
        I2[Hub Instance] --> C2[Client]

        G1[Group A]
        G1 -- Broadcast --> I1
        G1 -- Broadcast --> I2
    end

    R[Redis]

    G1 <-- PubSub --> R
    R --> G2
    R --> G3

    subgraph Server Instance: 2
        I3[Hub Instance] --> C3[Client]
        I4[Hub Instance] --> C4[Client]

        G2[Group A]
        G2 -- Broadcast --> I3
        G2 -- Broadcast --> I4
    end


    subgraph Server Instance: 3
        I5[Hub Instance] --> C5[Client]
        I6[Hub Instance] <-.-> C6[Client]

        G3[Group A]
        G4[Group B]
        G3 -- Broadcast --> I5
        G4 <-.-> I6
    end
```

## Connect groups with Redis

To use Redis, install the `MagicOnion.Server.Redis` package.

```shell
dotnet add package MagicOnion.Server.Redis
```

Next, set up to use Redis by using the `UseRedisGroup` method on the builder returned by `AddMagicOnion`.

```csharp
builder.Services.AddMagicOnion()
    .UseRedisGroup(options =>
    {
        //options.ConnectionString = "localhost:6379";
        //options.ConnectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");
    });
```

The `UseRedisGroup` method has an optional argument to specify whether to use the default group provider. Specify `true` if you want to use the default group provider. If not specified or `false` is specified, you need to specify the group provider for each StreamingHub using the `[GroupConfiguration]` attribute.

```csharp
[GroupConfiguration(typeof(RedisGroupProvider))]
public class MyStreamingHub : StreamingHubBase<IMyStreamingHub, IMyStreamingHubReceiver>, IMyStreamingHub
{
    // ...
}
```

## Connect groups with NATS (Preview)

NATS support is currently in preview and is provided as a package for Multicaster. To use NATS, install the `Multicaster.Distributed.Nats` package.

```shell
dotnet add package Multicaster.Distributed.Nats
```

Next, register `NatsGroupOptions` with the service.

```csharp
builder.Services.AddSingleton<NatsGroupOptions>(new NatsGroupOptions() { Url = "nats://localhost:4222" });
```

Specify the group provider to use NATS in StreamingHub by using the `[GroupConfiguration]` attribute.

```csharp
[GroupConfiguration(typeof(NatsGroupProvider))]
public class MyStreamingHub : StreamingHubBase<IMyStreamingHub, IMyStreamingHubReceiver>, IMyStreamingHub
{
    // ...
}
```

If you want to use NATS as the default group provider, replace `IMulticastGroupProvider` as follows.

```csharp
builder.Services.RemoveAll<IMulticastGroupProvider>();
builder.Services.AddSingleton<IMulticastGroupProvider, NatsGroupProvider>();
```

## Limitations

- Client results are not supported
- `Count`, `CountAsync` methods are not supported
