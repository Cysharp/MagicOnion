# Hub-Context pattern

## Overview

There is a pattern of implementing a game server using MagicOnion called the Hub-Context pattern.
In this pattern, a class called Context is prepared to hold the state, and the game logic refers to the state of the Context to manage the game state without holding the state in StreamingHub itself.

This pattern has the following characteristics:

- StreamingHub holds the minimum necessary state
- Context holds the game state
- Context has a command queue that receives commands from clients
- Clients add commands to the command queue of Context via StreamingHub
- A loop is executed to update the game state by referring to the Context from within the loop
    - Example: Execute commands added to the command queue and update the state of the Context
    - Example: Update the state of the Context at regular intervals

![](/img/docs/fig-hub-context-01.png)

## Benefits

The benefits of this pattern are that "management of the start and end of the game state is independent of StreamingHub" and "Minimizing consideration of concurrent execution".

### Management of the start and end of the game state is independent of StreamingHub
For example, when implementing a battle server for a battle royale, players need to enter the "battlefield" after the match is made. In this case, the problem arises as to who should create the battlefield. One common solution is to create the battlefield in the first player's processing when connecting to StreamingHub, but there are several considerations to be made, such as when multiple players connect at the same time or when a player is disconnected.

When the match is made, the battlefield is created in the server or between servers, and the player only needs to join the field. This makes it simple and easy to understand. In this example, the "battlefield" is the Context.

In addition, StreamingHub is affected by disconnections and reconnections of players, as well as the network and client conditions of the player, so it is safer to manage the game state independently.


### Minimizing consideration of concurrent execution
This pattern has a "command queue" that accepts commands from clients and updates the game state by consuming it. By executing the command queue in a loop that advances the game process, the player's operations are executed and the game state is changed. The command queue is implemented with .NET's `ConcurrentQueue` and can safely add commands from multiple clients. The consumption of the command queue advances the game process from a single loop, so commands are never executed in parallel. This ensures that the Context is always updated by a specific thread and limits the scope of locking.

:::warning
Even if you use a command queue to update the game state from a single thread, you need to perform appropriate locking if you need to refer to the state of the Context directly from StreamingHub.
:::

## Implementation example

In this section, we will explain a simple implementation example of the Hub-Context pattern.

In this pattern, the following elements need to be implemented:

- `GameContext`: A class that holds the game state and command queue
- `ICommand` and `*Command` classes: Command interface that represents game operations and its implementation classes
- `GameLoop`: A class that executes a loop to update the game state by referring to the `GameContext`
- `GameContextRepository`: A class that holds the `GameContext` and the `Task` of the loop
- `GameHub`: A StreamingHub that accepts operations from clients and adds commands to the `GameContext` command queue

:::warning
This implementation example is written with the minimum code to explain the concept. Please implement validation, error handling, termination processing, cancellation, and performance considerations according to your project.
:::

First, define the `GameContext` class that holds the game state and command queue.

`GameContext` holds an ID to uniquely identify it, a flag indicating whether it is completed, and a `ConcurrentQueue` to hold commands from users. The `ICommand` interface that represents the command is defined in the next section.

```csharp
public class GameContext
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsCompleted { get; set; }
    public ConcurrentQueue<ICommand> CommandQueue { get; } = new();
}
```

Next, define and implement the command. The command is defined as an `ICommand` interface. The command has an `Execute` method that references and updates the game state using `GameContext`.

The command implementation is defined as a class that implements this interface. In this example, we define a `MoveCommand` that moves and an `AttackCommand` that attacks. Commands have parameters (e.g., the ID of the target player, the destination of the move, the opponent of the attack, etc.), and the `Execute` method uses these values to perform the operation.

```csharp
public interface ICommand
{
    void Execute(GameContext context);
}

public class MoveCommand(Guid playerId, int x, int y) : ICommand
{
    public void Execute(GameContext context)
    {
        // Update game state in GameContext ...
    }
}

public class AttackCommand(Guid playerId, Guid targetId) : ICommand
{
    public void Execute(GameContext context)
    {
        // Update game state in GameContext ...
    }
}
```

Next, define the mechanism to execute the game loop, which is the mechanism to execute the game loop next. This class executes a loop that updates the game state by referring to `GameContext`.

The loop is defined as an asynchronous method that takes `GameContext` as an argument. This loop continues until the `IsCompleted` flag of `GameContext` becomes `true` and executes commands from the `CommandQueue`. In this example, the loop is executed every 100ms (10fps) using `Task.Delay`.

```csharp
public class GameLoop
{
    public static async Task RunLoopAsync(GameContext ctx)
    {
        while (!ctx.IsCompleted)
        {
            // Do work...

            // Consume all commands in the queue.
            while (ctx.CommandQueue.TryDequeue(out var command))
            {
                command.Execute(ctx);
            }

            // Do work...

            // Wait for next frame.
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}
```

The loop in this example only updates the state by consuming the command queue, but in actual games, the server may execute processing based on the passage of time, etc.

Next, define the `GameContextRepository` that creates and holds the `GameContext`. This class creates a `GameContext` and holds the `Task` of the loop started by the `GameLoop`. In the `CreateAndRun` method, a new `GameContext` is created, the loop is started using the `Context`, and the `Context` is returned. The `TryGet` method gets the `GameContext` with the specified ID, and the `Remove` method removes the `GameContext` with the specified ID.

```csharp
public class GameContextRepository
{
    private readonly ConcurrentDictionary<Guid, (GameContext Context, Task LoopTask)> _contexts = new();

    public GameContext CreateAndRun()
    {
        var context = new GameContext();
        var loopTask = GameLoop.RunLoopAsync(context);
        _contexts[context.Id] = (context, loopTask);
        return context;
    }

    public bool TryGet(Guid id, out GameContext? context)
    {
        if (_contexts.TryGetValue(id, out var contextAndLoopTask))
        {
            context = contextAndLoopTask.Context;
            return true;
        }

        context = null;
        return false;
    }

    public void Remove(Guid id)
    {
        _contexts.Remove(id, out _);
    }
}
```

This `GameContextRepository` is registered with the DI container by `builder.Services.AddSingleton<GameContextRepository>()` so that it can be used by other classes such as StreamingHub.

Next, define the `GameHub` that receives input from the player and adds it to the `CommandQueue` of `GameContext`. This class defines `GameHub` that is a StreamingHub that receives input from the player and adds commands to the `CommandQueue` of `GameContext`. The important point here is that the implementation of the Hub method is centered around adding commands to the command queue, and the StreamingHub does not hold more operations and state than necessary.

```csharp
public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
{
    ValueTask AttackAsync(Guid targetId);
    ValueTask MoveAsync(int x, int y);
}

public interface IGameHubReceiver
{
    void OnAttack(Guid playerId, Guid targetId);
    void OnMove(Guid playerId, int x, int y);
}

public class GameHub(GameContextRepository gameContextRepository) : StreamingHubBase<IGameHub, IGameHubReceiver>
{
    public ValueTask AttackAsync(Guid targetId)
    {
        if (gameContextRepository.TryGet(Context.ContextId, out var context))
        {
            context.CommandQueue.Enqueue(new AttackCommand(Context.ContextId, targetId));
        }
        return default;
    }
    public ValueTask MoveAsync(int x, int y)
    {
        if (gameContextRepository.TryGet(Context.ContextId, out var context))
        {
            context.CommandQueue.Enqueue(new MoveCommand(Context.ContextId, x, y));
        }
        return default;
    }
}
```

The example above retrieves the Context each time for simplicity, but in cases where there is a join process, it is also possible to hold a reference to the Context in StreamingHub and use it.

Creation of `GameContext` and the timing to start the loop depend on the game flow, and the actual timing of creation depends on the game specifications. For example, it may be created when the match is completed. In any case, the server can manage the lifecycle of the Context independently of StreamingHub by creating and deleting it through `GameContextRepository`.

The following is an example of implementing internal API endpoints to start and end the game.

```csharp
app.MapPost("/internal/create", (GameContextRepository repository) =>
{
    // Create new GameContext and write information to the database.
    var context = repository.CreateAndRun();
    return context.Id;
});

app.MapPost("/internal/complete", (GameContextRepository repository, Guid id) =>
{
    // Do something to complete the game
    repository.Remove(id);
    return Ok();
});
```

At the next section, to call the client from the game logic (processing in commands or loops), you need to be able to handle groups so that you can call the client from the game logic. To achieve this, you need to hold groups in `GameContext`.

### Notifying clients using groups

So far, we have explained the implementation of input from the client and its processing. This section will explain how to handle groups to notify clients.

MagicOnion provides groups associated with StreamingHub, but you can manage groups in the application logic using the [Application-managed groups](/streaminghub/group-application-managed) feature. This feature is well-suited to the Hub-Context pattern, and by managing groups in Context, you can manage clients independently of StreamingHub.


In this example, we define a group as a collection of receivers of StreamingHub, and create and delete it when creating `GameContext` and deleting Context.

The `IMulticastGroupProvider` for creating groups is registered with the DI container, so it can be used by the constructor of `GameContextRepository`. In the implementation example, the group is defined as `IMulticastSyncGroup<Guid, IGameHubReceiver>` to distinguish clients based on the connection ID.

```csharp
using Cysharp.Runtime.Multicast;

public class GameContext : IDisposable
{
    public Guid Id { get; }
    public bool IsCompleted { get; set; }
    public ConcurrentQueue<ICommand> CommandQueue { get; } = new();
    public IMulticastSyncGroup<Guid, IGameHubReceiver> Group { get; }

    public GameContext(IMulticastGroupProvider groupProvider)
    {
        Id = Guid.NewGuid();
        Group = groupProvider.GetOrAddSynchronousGroup<Guid, IGameHubReceiver>($"Game/{Id}");
    }

    public void Dispose()
    {
        Group.Dispose();
    }
}

public class GameContextRepository(IMulticastGroupProvider groupProvider)
{
    ...
    public GameContext CreateAndRun()
    {
        var context = new GameContext(groupProvider);
        var loopTask = GameLoop.RunLoopAsync(context);
        _contexts[context.Id] = (context, loopTask);
        return context;
    }

    public void Remove(Guid id)
    {
        if (_contexts.Remove(id, out var contextAndTask))
        {
            contextAndTask.Context.Dispose();
        }
    }
    ...
}
```

:::warning
When creating a group manually, be sure to call `Dispose` when it is no longer needed. If you do not call `Dispose` to delete the group, the group will remain in the provider until it is deleted, causing a memory leak.
:::


The group can be registered as a member of `IGameHubReceiver`, so you can directly register the `Client` property (proxy to the client) of StreamingHub as a member of `IGameHubReceiver`.
To register a client to a group, you need to register the client to the group when the connection is established and remove it when the connection is disconnected.

```csharp
public class GameHub(GameContextRepository gameContextRepository) : StreamingHubBase<IGameHub, IGameHubReceiver>
{
    public override ValueTask OnConnected()
    {
        if (gameContextRepository.TryGet(Context.ContextId, out var context))
        {
            context.Group.Add(Context.ConnectionId, Client);
        }
        return default;
    }

    public override ValueTask OnDisconnected()
    {
        if (gameContextRepository.TryGet(Context.ContextId, out var context))
        {
            context.Group.Remove(Context.ConnectionId);
        }
        return default;
    }

    ...
}
```

:::tip
In this implementation example, the ConnectionId (StreamingHub's connection ID) is used as the key to distinguish clients registered in the group, but consider using other keys. When using the connection ID, there is a problem that the connection ID changes when reconnected. This can be avoided by using the ID of an authenticated player, for example.
:::

After registering the client to the group, you can send messages to the client through the group. For example, you can send messages to the client from commands or server processing loops.

```csharp
public class MoveCommand(Guid playerId, int x, int y) : ICommand
{
    public void Execute(GameContext context)
    {
        // Update game state in GameContext ...
        context.Group.All.OnMove(playerId, x, y);
    }
}

public class AttackCommand(Guid playerId, Guid targetId) : ICommand
{
    public void Execute(GameContext context)
    {
        // Update game state in GameContext ...
        context.Group.All.OnAttack(playerId, targetId);
    }
}
```

For more information on groups, see [Groups](/streaminghub/group) and [Application-managed groups](/streaminghub/group-application-managed).

## More effective game loops
The implementation example of the game loop above uses `Task.Delay` to execute the loop at regular intervals, but this is not suitable for general game implementations.

So, we recommend using [LogicLooper](https://github.com/Cysharp/LogicLooper/) library provided by Cysharp. This library provides a mechanism to execute loops at regular intervals like Unity's `Update` method. By using this library, you can implement more effective game loops.

```csharp
public class GameLoop
{
    public static Task RunLoopAsync(GameContext context)
    {
        return LogicLooperPool.Shared.RegisterActionAsync(() =>
        {
            // Do work...

            // Consume all commands in the queue.
            while (context.CommandQueue.TryDequeue(out var command))
            {
                command.Execute(context);
            }

            // Do work...

            return !context.IsCompleted;
        });
    }
}
```
