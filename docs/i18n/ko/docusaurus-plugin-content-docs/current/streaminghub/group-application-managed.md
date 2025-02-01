# アプリケーション管理によるグループ

:::tip
この機能は MagicOnion v7.0.0 で追加されました。https://github.com/Cysharp/MagicOnion/pull/778
:::

ゲームのようなアプリケーションではアプリケーションのロジックによってグループの細かい制御を行いたいケースが存在します。

例えばゲーム内のバトルフィールドと参加プレイヤー、チームという要素がある場合にすべてのプレイヤーは同一のバトルフィールドに接続するがチームごとにコミュニケーションチャンネルを持つ必要があるといったケースや、一つの対戦中にプレイヤーのチームを組みかえる場合があるといったケースなどです。ほかにもメタバース空間のように参加者がまだいない時点でもグループが欲しいといったケースも考えられます。

v7 以前の MagicOnion ではグループの作成は StreamingHub の中で行うことしかできませんでしたが、v7 からは [Multicaster](https://github.com/Cysharp/Multicaster) ライブラリーを基盤としたことでアプリケーションが独自にグループを作成できるようになりました。

またこれにより StreamingHub や MagicOnion 自体の依存を持たずともグループを作成できるため、ロジックのテストも容易になります。

## グループの作成と取得
グループの作成は Dependency Injection により得られる `IMulticastGroupProvider` インターフェース (グループプロバイダー) を経由して行います。このインターフェースに関する実装はすでに MagicOnion が登録しているため StreamingHub や Unary サービス、その他 Hosted Service のような DI コンテナーに登録されたクラスから利用できます。

`GetOrAddSynchronousGroup` または `GetOrAddAsynchronousGroup` にクライアントを区別するキーの型 (例えば `Guid` やアプリケーション固有のユーザー ID など)とクライアントのインターフェースの型(多くの場合 StreamingHub のレシーバー)、グループ名を指定してグループを作成または取得します。グループプロバイダー内に指定された名前のグループがない場合は新規に作成されます。

```csharp
public class GroupService(IMulticastGroupProvider groupProvider) : IDisposable
{
    // NOTE: You can also manage multiple groups using a dictionary, etc.
    private readonly IMulticastSyncGroup<Guid, IMyReceiver> _group
         = groupProvider.GetOrAddSynchronousGroup<Guid, IMyHubReceiver>("MyGroup");

    public void Dispose() => _group.Dispose();
}
```

:::tip
`GetOrAddSynchronousGroup` はグループ操作に同期的に処理できる場合、例えばグループをインメモリーでのみ処理する場合に適しています。 `GetOrAddAsynchronousGroup` はグループ操作に非同期を必要とする場合、例えば Redis を使用して複数の MagicOnion サーバーでグループを配信している場合に適しています。
:::

## グループの削除

グループの削除は `IGroup<T>.Dispose` メソッドを呼び出すことで行います。これはグループプロバイダーから削除することを意味します。その後再び `GetOrAddSynchronousGroup` または `GetOrAddAsynchronousGroup` を呼び出すことで新しいグループを作成できます。

## グループにクライアントを登録

グループにクライアントを登録するには `Add` または `AddAsync` メソッドを呼び出します。このメソッドはクライアントを区別するキーとクライアントを引数に取ります。クライアントのインターフェースを StreamingHub のレシーバーと同じ型にしている場合は登録するクライアントとして StramingHub の `Client` プロパティを渡すことができます。

```csharp
_group.Add(ConnectionId, Client);
```

## グループからクライアントを削除

グループからクライアントを削除するには `Remove` または `RemoveAsync` メソッドを呼び出します。このメソッドはクライアントを区別するキーを引数に取ります。

```csharp
_group.Remove(ConnectionId);
```

StreamingHub のグループと異なりグループに含まれるクライアントがなくなった場合にもグループは破棄されません。

## グループに対するメッセージの配信
グループに対するメッセージの配信方法は MagicOnion の SteramingHub が提供するグループと同様、`All` や `Except`, `Single` などを使用して送信先を決定して呼び出せます。

```csharp
_group.All.OnMessage("Hello, world!");

_group.Only([connectionId1, connectionId2]).OnMessage("Hello, world! to specific clients");

_group.Except(ConnectionId).OnMessage("Hello, world! except me");

_group.Single(ConnectionId).OnMessage("Hello, world! to me");
```

## 実装例
以下は単一のグループをあらかじめ作成しておき、そのグループにユーザーを追加/削除、メッセージ配信する例です。

```csharp
public class GroupService(IMulticastGroupProvider groupProvider) : IDisposable
{
    // NOTE: You can also manage multiple groups using a dictionary, etc.
    private readonly IMulticastSyncGroup<Guid, IMyReceiver> _group = groupProvider.GetOrAddSynchronousGroup<Guid, IMyHubReceiver>();

    public void SendMessageToAll(string message) => _group.All.OnMessage(message);

    public void AddMember(Guid id, IMyHubReceiver receiver) => _group.Add(receiver);
    public void RemoveMember(Guid id) => _group.Remove(id);

    public void Dispose() => _group.Dispose();
}

public class MyBackgroundService(GroupService groupService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            groupService.SendMessageToAll("Send message periodically...");
        }
    }
}

...

builder.Services.AddSingleton<GroupService>();
builder.Services.AddHostedService<MyBackgroundService>();

...

public class MyHub(GroupService groupService) : StreamingHubBase<IMyHub, IMyHubReceiver>, IMyHub
{
    protected override ValueTask OnConnected()
    {
        groupService.AddMember(ContextId, Client);
        return default;
    }

    protected override ValueTask OnDisconnected()
    {
        groupService.RemoveMember(ContextId);
        return default;
    }

    public Task SendMessage(string message) => groupService.SendMessageToAll(message);
}
```

この `GroupService` をテストしたい場合にはインメモリーの `InMemoryGroupProvider` を使用することで MagicOnion と関係なく実行できます。

```csharp
[Fact]
public async Task TestGroupService()
{
    // Arrange
    var groupProvider = new InMemoryGroupProvider(DynamicInMemoryProxyFactory.Instance);
    var groupService = new GroupService(groupProvider);
    var receiver = Substitute.For<IMyReceiver>(); // Use NSubstitute, Moq, etc.

    // Act
    groupService.AddMember(Guid.NewGuid(), receiver);
    groupService.SendMessageToAll("Hello, world!");

    await Task.Delay(100); // Wait for message delivery.

    // Assert
    receiver.Received().OnMessage("Hello, world!");
}
```

## 注意事項
### グループで使用するクライアントの型
グループで使用するクライアントの型は StreamingHub の Receiver と同じにすることを強く推奨します。これはグループの内部でメッセージを送信する際に StreamingHub の Client を特別に扱い効率よく配信する仕組みがあるためです。

### グループの生存期間
アプリケーションでグループを作成した場合はグループが不要となった時点で明示的に削除する必要があります。グループを削除しない場合、グループプロバイダーがグループを保持し続けるためメモリリークが発生します。

MagicOnion が管理するグループはグループからすべてのユーザーが削除された時点で削除されますが、アプリケーションが管理するグループの生存期間はアプリケーション側が判断する必要があります。
