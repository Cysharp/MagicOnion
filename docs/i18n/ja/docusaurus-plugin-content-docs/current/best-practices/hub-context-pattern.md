# Hub-Context パターン

## 概要

MagicOnion を使用したゲームサーバーの実装パターンの一つとして Hub-Context パターンと呼ぶものがあります。
これは Context と呼ぶステートを保持するクラスを用意し、StreamingHub 自体にはステートを保持せずにゲームのロジックからは Context のステートを参照してゲームの状態を管理するパターンです。

このパターンは以下のような特徴があります:

- StreamingHub は必要最低限のステートを持つ
- Context にゲームのステートを保持する
- Context はクライアントからの操作を受けるコマンドキューを持つ
- クライアントは StreamingHub を通して Context のコマンドキューにコマンドを追加する
- Context を参照してゲームのステートを更新するループを実行する
    - 例: コマンドキューに追加されたコマンドを実行して Context のステートを更新する
    - 例: 一定間隔で Context のステートを更新する
    - 例: Context が持つ Group を通してクライアントに通知する

![](/img/docs/fig-hub-context-01.png)

## メリット

このパターンのメリットは「ゲームのステートの開始と終了を含めた管理が StreamingHub から独立している」という点と「並列実行の考慮を最小限にできる」点にあります。

### ゲームのステートの開始と終了を含めた管理が StreamingHub から独立している
例えばバトルロイヤルのバトルサーバーを実装する場合、プレイヤーはマッチメイクが完了した後「バトルフィールド」に入る必要がありますが、誰がバトルフィールドを作るべきなのかという問題が発生します。よくある解決方法の一つとしては StreamingHub に接続してきた最初のプレイヤーの処理でバトルフィールドを作成するという方法がありますが、同時に接続してきた場合や途中でプレイヤーが切断された場合など考慮すべき点がいくつか浮かび上がってきます。

そこでマッチメイクが完了した際にサーバー内あるいはサーバー間でバトルフィールドを作成し、プレイヤーはフィールドに参加するだけというフローにすることでシンプルかつ分かりやすい形で実装できます。この例での「バトルフィールド」が Context にあたります。

その他にも StreamingHub は切断や再接続といったプレイヤーのネットワークやクライアントの状況による影響を受けるため、ゲームのステートは独立して管理しておくほうが安全です。

### 並列実行の考慮を最小限にできる
このパターンではクライアントからの操作をコマンドとして受け付ける「コマンドキュー」を持ち、それを消化することでプレイヤーの操作を実行してゲームのステートを変化させます。コマンドキューは .NET の `ConcurrentQueue` などで実装することで、複数のクライアントから安全にコマンドを追加できます。コマンドキューの消化はゲーム処理を進める単一のループから実行されるため、並列でコマンドが実行されることはありません。このことから Context の更新は常に特定のスレッドで行われることが保証され、排他制御を行う範囲を限定できます。

:::warning
コマンドキューを使用することで更新を単一のスレッドから行うようにした場合であっても、StreamingHub から直接 Context のステートを参照する必要がある場合は適切な排他制御を行う必要があります。
:::

## 実装例

このセクションでは Hub-Context パターンの簡単な実装例について説明します。


このパターンで実装が必要となる要素は以下の通りです:

- `GameContext`: ゲームのステートとコマンドキューを保持するクラス
- `ICommand` および `*Command` クラス: ゲームの操作を表すコマンドインターフェースとその実装クラス
- `GameLoop`: `GameContext` を参照してゲームのステートを更新するループを実行するクラス
- `GameContextRepository`: `GameContext` とループの `Task` を保持するクラス
- `GameHub`: クライアントからの操作を受け付け、`GameContext` のコマンドキューにコマンドを追加する StreamingHub

:::warning
この実装例は概念を説明するための最低限のコードで記述されています。バリデーションやエラーハンドリング、終了処理やキャンセル、パフォーマンス面での考慮などは実際のプロジェクトに合わせて適切に実装してください。
:::

初めにゲームのステートとコマンドキューを保持する `GameContext` クラスを定義します。

`GameContext` は一意に特定するための ID と完了したかどうかのフラグ、ユーザーからのコマンドを保持する `ConcurrentQueue` を持ちます。コマンドの `ICommand` インターフェースはこの次で定義します。

```csharp
public class GameContext
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsCompleted { get; set; }
    public ConcurrentQueue<ICommand> CommandQueue { get; } = new();
}
```

次にコマンドを定義、実装します。初めにコマンドを表す `ICommand` インターフェースを定義します。コマンドは `Execute` メソッドを持ち `GameContext` を使用してゲームのステートを参照、更新します。
実際のコマンドはこのインターフェースを実装したクラスとして定義します。ここでは例として移動を行う `MoveCommand` と攻撃を行う `AttackCommand` を定義しています。コマンドはそれぞれパラメータを持ち(例えば対象プレイヤーのIDや移動先、攻撃相手など)、`Execute` でその値を使用して処理を行います。

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

次にゲームのループを実行する仕組みである `GameLoop` を定義します。このクラスは `GameContext` を参照してゲームのステートを更新するループを実行します。

ループの実行は `GameContext` を受ける非同期メソッドの `RunLoopAsync` メソッドとして定義します。このループは `GameContext` の `IsCompleted` フラグが `true` になるまで続けられ、`CommandQueue` からコマンドを取り出し、それを実行します。この例では `Task.Delay` を使用して 100ms ごと(10fps)にループを実行しています。

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

この例のループ中ではコマンドキューの消化によるステートの更新のみが実装されていますが、実際のゲームではサーバーが時間経過などによって処理を実行するといったことも考えられます。

次に `GameContext` の作成や保持をする `GameContextRepository` を定義します。このクラスは `GameContext` を作成したり、Context と `GameLoop` で開始したループの `Task` を保持します。下記の例では `CreateAndRun` メソッドで新しい `GameContext` を作成し Context を使用してループを開始したのち、Context を返します。`TryGet` メソッドでは指定した ID の `GameContext` を取得し、`Remove` メソッドで指定した ID の `GameContext` を削除しています。

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

この `GameContextRepository` は `builder.Services.AddSingleton<GameContextRepository>()` といった形で DI コンテナに登錍しておくことで StreamingHub など他のクラスから利用できるようにします。

次にプレイヤーからの入力を受け取る StreamingHub である `GameHub` を定義します。このクラスは `GameContextRepository` を DI で受け取り、プレイヤーからの操作を受け付けて `GameContext` の `CommandQueue` にコマンドを追加します。ここで重要となる点は Hub メソッドの実装はコマンドキューにコマンドを追加する操作が中心となり、必要以上の操作とステートは StreamingHub は持たないようにする点です。

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

ここでは簡単に例を示すため都度 Context を取得していますが、参加処理などがある場合は Context への参照を StreamingHub に保持して、コマンドキューに登録するといった方法も考えられます。

`GameContext` を作成してループを開始するタイミングはゲームのフローによって異なり、実際にどこで作成するかはゲームの仕様によって変わります。例えばマッチメイクが完了した際に作成するといったケースが考えられます。どのような場合であってもサーバーでは `GameContextRepository` を通して作成、削除を行うことで StreamingHub とは関係なく Context のライフサイクルを管理できます。

以下はゲームの開始と終了を行う内部 API エンドポイントを実装した例です。

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

次のセクションではゲームロジック (コマンド内やループ内の処理) からクライアントを呼び出せるようにグループを取り扱えるようにします。これを実現するには `GameContext` にグループを保持する必要があります。

### グループを使用したクライアントへの通知

ここまではクライアントからの入力とその処理に関する実装について説明しました。ここではクライアントに通知するためのグループの取り扱いについて説明します。

MagicOnion では StreamingHub に紐づいたグループが提供されますが、[アプリケーション管理によるグループ](/streaminghub/group-application-managed) という形でアプリケーションのロジックでグループを管理できます。この機能は Hub-Context パターンと相性がよく、Context でグループを管理することで StreamingHub から独立してクライアントの管理が可能となります。

ここではグループは StreamingHub のレシーバーを束ねたものとし、 `GameContext` を作成する際に作られ、Context の削除と共に削除するようにします。

グループを作成するための `IMulticastGroupProvider` は DI コンテナーに登録されているので `GameContextRepository` のコンストラクターで受け取ることで使用できます。実装例でのグループは接続の ID を元にクライアントを区別するため `IMulticastSyncGroup<Guid, IGameHubReceiver>` として定義しています。

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
グループを手動で作成した場合は不要になった際に `Dispose` を呼び出してください。`Dispose` を呼び出してグループを削除するまでグループはプロバイダーに残り続けるため、適切に削除されなかった場合にメモリーリークとなります。
:::


グループは `IGameHubReceiver` を束ねているため、StreamingHub の `Client` プロパティ(クライアントへのプロキシー)を直接登録できます。
グループへのクライアントの登録は StreamingHub で行います。ここでは例として StreamingHub に接続完了時にグループに登録し、切断時に削除するよう実装します。

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
この実装例ではグループに登録したクライアントを区別するキーとして ConnectionId (StreamingHub の接続 ID) を使用していますが他のキーを使用することを検討してください。接続 ID を使用した場合、再接続時に接続 ID が変化してしまう問題が発生します。これは認証されたプレイヤーの ID などを使用することで回避できます。
:::


これ以降 Context のグループを通してクライアントにメッセージを送信できるようになります。例えばコマンドの処理からクライアントにメッセージを通知したり、ループ中のサーバー処理から通知したりといったことが可能となります。

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

グループに関しての詳しい情報は [グループ](/streaminghub/group) と [アプリケーション管理によるグループ](/streaminghub/group-application-managed) を参照してください。

## より効果的なゲームループ
実装例のゲームループの実装には `Task.Delay` を使用して一定間隔でループを実行していますが、これは一般的なゲームの実装には適していません。

Cysharp では [LogicLooper](https://github.com/Cysharp/LogicLooper/) というライブラリを提供しています。これは Unity の `Update` メソッドのように一定間隔でループを実行するためのライブラリです。このライブラリを使用することでより効果的なゲームループを実装できます。

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
