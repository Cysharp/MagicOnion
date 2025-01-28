# メソッド、インターフェース定義

## 概要
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

## シリアライズ
Unary サービスと同様にメソッドの引数及び戻り値はデフォルトで MessagePack によってシリアライズされます。そのため型が MessagePack でシリアライズできるようにマークされているか、あるいは構成されている必要があります。また、メソッドの引数は最大で15個まで許可されます。

## 継承
StreamingHub のインターフェースは継承できます。これは複数の Hub で共通のメソッドを持つ場合に役立ちます。

```csharp
public inteface ICommonHub
{
    ValueTask PingAsync();
}

public inteface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>, ICommonHub
{
    ValueTask JoinAsync(string roomName, string userName);
    ValueTask LeaveAsync();
    ValueTask SendMessageAsync(string message);
}
```

## 高度な設定

### `Ignore` 属性
`Ignore` 属性を使用することで特定のメソッドを Hub メソッドとして認識させないようにできます。


### `MethodId` 属性
`MethodId` 属性を使用することでメソッドの識別に使用する ID を手動で指定できます。Hub メソッドの ID はメソッド名から FNV1A32 で計算された値を使用するため手動で設定する必要はありません。メソッド名を変更したが元の ID を使用したい、何らかの理由で ID が衝突してしまったといった特殊な用途でのみ使用してください。
