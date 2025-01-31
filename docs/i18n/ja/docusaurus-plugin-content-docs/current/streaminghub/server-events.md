# サーバー上の接続、切断イベント

サーバー上ではクライアントが接続したり切断した際にイベントが発生します。これらのイベントは `StreamingHubBase` クラスのイベントメソッドをオーバーライドすることでハンドルできます。

## `OnConnecting` メソッド
`OnConnecting` メソッドはクライアントが StreamingHub に接続を確立中に呼び出されます。この時点ではクライアントとの接続が確立されていないため、クライアントやグループに関する操作を行うことはできません。

```csharp
protected override async ValueTask OnConnecting()
{
    // クライアントが接続を確立中の処理

    // 例: StreamingHub 自体の初期化など
}
```

## `OnConnected` メソッド
`OnConnected` メソッドはクライアントが StreamingHub に接続が完了した際に呼び出されます。この時点でクライアントとの接続が確立済みとなり、クライアントの呼び出しやグループの操作が可能です。

```csharp
protected override async ValueTask OnConnected()
{
    // クライアントが接続完了時の処理

    // 例: グループへの追加や初期状態の送信
    // this.group = await Group.AddAsync("MyGroup");
    // Client.OnConnected(initialState);
    // ..
}
```


## `OnDisconnected` メソッド
`OnDisconnected` メソッドはクライアントが StreamingHub から切断された際に呼び出されます。この時点でクライアントとの接続は切断されているため、クライアントに関する操作は無効です。

```csharp
protected override async ValueTask OnDisconnected()
{
    // クライアントが切断された際の処理
}
```

切断についての関連した操作や情報は [切断のハンドリング](disconnection) を参照してください。
