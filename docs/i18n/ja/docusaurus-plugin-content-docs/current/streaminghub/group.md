# グループの基礎

StreamingHub は複数のクライアントにメッセージを配信する仕組みがあります。例えばチャットで受け取ったテキストメッセージをクライアントに配信するといった場合に使用します。

この配信する対象をサーバーで管理する仕組みがグループです。グループは任意の名前で作成することができ、そのグループに所属しているクライアントに対してメッセージを送信できるようになります。これはチャットのチャンネルやルームのような役割を果たします。

## グループの作成/取得とクライアントの追加

グループは StreamingHub の `Group` プロパティの `AddAsync` メソッドにグループ名を渡して呼び出すことで、参加したグループのインスタンスを取得できます。呼び出し時にまだグループが存在していない場合には新たに作成されます。

```csharp
public async ValueTask JoinAsync(string userName, string roomName)
{
    // Add client to the group with the specified group name.
    // If the group does not exist, it will be created.
    this.room = await Group.AddAsync(roomName);
    // ...
}
```

## グループへのメッセージの配信
このグループのインスタンスはレシーバーに対するプロキシーを提供し、グループに属するクライアントに対するメッセージの一斉配信を行えます。 StreamingHub での開発ではこのインスタンスをフィールドに保持しておいて必要に応じて呼び出します。

```csharp
// ルームに含まれるすべてのクライアントに "Hello, workd!" というメッセージを送信
this.room.All.OnMessage("Hello, world!");
```

すべてのクライアントだけでなく、特定のクライアントなど送信先を限定したプロキシーを取得することもできます。

```csharp
this.room.Only([connectionId1, connectionId2]).OnMessage("Hello, world! to specific clients");

this.room.Except(ConnectionId).OnMessage("Hello, world! except me");

this.room.Single(ConnectionId).OnMessage("Hello, world! to me");
```

- `All`: グループに含まれるすべてのクライアント
- `Single`: 特定の1クライアント
- `Only`: 特定のクライアント (複数)
- `Except`: 特定のクライアント以外 (複数)


## グループからクライアントの削除
グループからクライアントを削除するには `RemoveAsync` メソッドを使用します。

```csharp
public async ValueTask LeaveAsync()
{
    // グループからクライアントを削除
    await this.room.RemoveAsync(Context);
}
```

クライアントがサーバーから切断された場合は自動的にグループから削除されるため、明示的に削除する必要はありません。また、グループに含まれるクライアントがなくなった時点でグループは削除されます。

## より細かいグループの制御
ここで説明したグループは MagicOnion の StreamingHub と紐づき、管理されています。つまりクライアントがグループの作成とそれに含まれるクライアントの管理は Hub を通して操作する必要があります。

一方でゲームのようなアプリケーションではグループの作成や削除、クライアントの管理をアプリケーションロジック側で行いたい場合もあります。その場合は Hub のグループを使用せず、アプリケーション側でグループの管理を行うことができます。詳細については [アプリケーション管理によるグループ](group-application-managed) を参照してください。

## スレッドセーフティ
グループはスレッドセーフであり、複数のクライアントからの同時アクセスに対して安全に操作できます。ただしグループのインスタンスの作成、削除時の一貫性はアプリケーションで保証する必要があります。

例えば StreamingHub に紐づくグループでは最後のユーザーの Remove と新しいユーザーの Add がほぼ同時に実行された場合は一度削除された後に新しいグループが作成されます。この挙動はグループを保持している場合は問題となる場合があります。

グループに含まれるクライアントの増減とメッセージ配信で厳密な一貫性が必要な場合にもアプリケーション側でグループの管理やロックなどを行うことを検討してください。
