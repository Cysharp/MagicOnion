# StreamingHub を始める

このチュートリアルでは StreamingHub を始めるための簡単な手順を紹介します。

## 手順

StreamingHub を実装、利用するには下記の手順が必要となります。

- サーバーとクライアントの間で共有する StreamingHub インターフェイスを定義する
- サーバープロジェクトで定義した StreamingHub インターフェイスを実装する
- クライアントプロジェクトで定義した StreamingHub レシーバーを実装する
- クライアントプロジェクトで定義した StreamingHub を呼び出すためのクライアントプロキシを作成する

## サーバーとクライアントの間で共有する StreamingHub インターフェイスを定義する

共有ライブラリープロジェクトに StreamingHub のインターフェイスを定義します (Unity の場合はソースコードコピーやファイルリンクで対応します)。

StreamingHub のインターフェースは `IStreamingHub<TSelf, TReceiver>` を継承する必要があります。`TSelf` にはインターフェース自身、`TReceiver` にはレシーバーインターフェイスを指定します。レシーバーインターフェースはサーバーからクライアントにメッセージを送信し、受信するためのインターフェースです。

以下はチャットアプリケーションの StreamingHub インターフェイスの例です。クライアントはメッセージの受信や参加、退出イベントを送るレシーバーインターフェースを持っています。

```csharp
// A hub must inherit `IStreamingHub<TSelf, TReceiver>`.
public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
{
    ValueTask JoinAsync(string roomName, string userName);
    ValueTask LeaveAsync();
    ValueTask SendMessageAsync(string message);
}

public interface IChatHubReceiver
{
    void OnJoin(string userName);
    void OnLeave(string userName);
    void OnSendMessage(string userName, string message);
}
```

StreamingHub が提供するメソッドを **Hub メソッド** と呼びます。Hub メソッドはクライアントから呼び出されるメソッドで、戻り値の型は `ValueTask`, `ValueTask<T>`, `Task`, `Task<T>`, `void` のいずれかである必要があります。Unary サービスとは異なることに注意が必要です。

クライアントがメッセージを受け取る口となるレシーバーインターフェースもまたメソッドを持ちます。これらを **レシーバーメソッド** と呼びます。レシーバーメソッドはサーバーからメッセージを受けたときに呼び出されるメソッドです。レシーバーメソッドの戻り値は `void` である必要があります。[クライアント結果](client-results)を使用する場合を除き、原則として `void` を指定します。

## サーバープロジェクトで StreamingHub を実装する

サーバー上にクライアントから呼び出せる StreamingHub を実装する必要があります。サーバー実装は `StreamingHubBase<THub, TReceiver>` を継承し、定義した StreamingHub インターフェイスを実装する必要があります。

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    public async ValueTask JoinAsync(string roomName, string userName)
        => throw new NotImplementedException();

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

初めにチャットルームに参加するメソッド `JoinAsync` を実装します。このメソッドは指定された名前のルームに指定されたユーザー名で参加します。

`Group.AddAsync` メソッドでグループを作成し、そのグループへの参照を StreamingHub に保持してこの後の処理で使用します。グループの `All` プロパティーを介してグループに参加しているクライアントのレシーバーインターフェースを得られるので `OnJoin` メソッドを呼び出して参加を通知します。

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

`Client` プロパティーを使用するとその SteramingHub に接続しているクライアントのみを呼び出すこともできます。ここでは接続してきたクライアントにのみウェルカムメッセージを送信してみましょう。

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);

        Client.OnMessage("System", $"Welcome, hello {userName}!");
    }

    public async ValueTask LeaveAsync()
        => throw new NotImplementedException();

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

次に退出するメソッド `LeaveAsync` も実装します。退出時にはグループからクライアントを削除します。これは `Group.RemoveAsync` メソッドを使用して行います。`RemoveAsync` メソッドには `StreamingHubContext` クラスのオブジェクト (`Context` プロパティー)を渡します。グループからクライアントが削除されるとグループを介したメッセージがそのクライアントには届かなくなります。

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
    {
        room.All.OnLeave(Context.ConnectionId);
        await room.RemoveAsync(Context);
    }

    public async ValueTask SendMessageAsync(string message)
        => throw new NotImplementedException();
}
```

最後にクライアントからメッセージを受け取ったらグループに配信する `SendMessageAsync` メソッドを実装します。このメソッドではグループの `All` プロパティーを介してグループに参加しているクライアントの `OnMessage` メソッドを呼び出して通知します。

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    IGroup<IChatHubReceiver>? room;
    string userName = "unknown";

    public async ValueTask JoinAsync(string roomName, string userName)
    {
        this.room = await Group.AddAsync(roomName);
        this.userName = userName;
        room.All.OnJoin(userName);
    }

    public async ValueTask LeaveAsync()
    {
        room.All.OnLeave(Context.ConnectionId);
        await room.RemoveAsync(Context);
    }

    public async ValueTask SendMessageAsync(string message)
    {
        room.All.OnMessage(userName, message);
    }
}
```

## クライアントプロジェクトで StreamingHub レシーバーを実装する

クライアントプロジェクトで StreamingHub のレシーバーインターフェースを実装します。このインターフェースはクライアント側でメッセージを受け取り、処理したい型に実装します。

ここではシンプルな ChatHubReceiver 型を作成し、レシーバーインターフェース `IChatHubReceiver` を実装します。それぞれのメソッドはサーバーから送信されたメッセージを受け取りコンソールにメッセージを出力します。

```csharp
class ChatHubReceiver : IChatHubReceiver
{
    public void OnJoin(string userName)
        => Console.WriteLine($"{userName} joined.");
    public void OnLeave(string userName)
        => Console.WriteLine($"{userName} left.");
    public void OnMessage(string userName, string message)
        => Console.WriteLine($"{userName}: {message}");
}
```

## クライアントプロキシを作成して StreamingHub に接続する

SteramingHub に接続するには、`StreamingHubClient.ConnectAsync` メソッドを使用することで接続とクライアントプロキシの作成を行います。

`ConnectAsync` メソッドには接続先の `GrpcChannel` オブジェクトとレシーバーインターフェースのインスタンスを渡します。接続が確立されるとクライアントプロキシが返されます。サーバーから受信したメッセージはここで渡したレシーバーのインスタンスのメソッド呼び出しとなります。

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var receiver = new ChatHubReceiver();
var client = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver);

await client.JoinAsync("room", "user1");
await client.SendMessageAsync("Hello, world!");
```
