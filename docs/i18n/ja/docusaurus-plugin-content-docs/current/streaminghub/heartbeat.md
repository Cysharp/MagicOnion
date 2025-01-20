# ハートビート

:::tip
この機能は MagicOnion v7.0.0 で追加されました。
:::

ハートビート機能はサーバーとクライアント間の接続を維持し切断を早期に検出するために、サーバーからクライアントへ、クライアントからサーバーへ定期的にメッセージを送信する機能です。ハートビートには送信先が一定時間内に応答しない場合に切断するためのタイムアウトを指定できます。

## HTTP/2 の PING フレームを使わない理由

HTTP/2 にはハートビートのための PING フレームというメカニズムがあります。それにも関わらず MagicOnion には独自のハートビート機能があります。これはネットワーク構成にロードバランサーが含まれている場合、ロードバランサーが PING フレームを処理し MagicOnion サーバーに到達しない可能性があるためです。

```plaintext
[Client] ← PING/PONG → [LoadBalancer] ← PING/PONG → [Server]
```

MagicOnion はこのような環境下での疎通確認を確実にするために、サーバーとクライアント間で明示的にデータを送受信するハートビートメカニズムを提供しています。

## クライアント
`StreamingHubClient` を作成する際にクライアントからのハートビートを指定できます。デフォルトでは無効になっています。

```csharp
// Send a message to the server every 30 seconds
var options = StreamingHubClientOptions.CreateWithDefault().WithClientHeartbeatInterval(TimeSpan.FromSeconds(30));
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

### API
```csharp
public class StreamingHubClientOptions
{
    /// <summary>
    /// Sets a heartbeat interval. If a value is <see keyword="null"/>, the heartbeat from the client is disabled.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatInterval(TimeSpan? interval);

    /// <summary>
    /// Sets a heartbeat timeout period. If a value is <see keyword="null"/>, the client does not time out.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatTimeout(TimeSpan? timeout);

    /// <summary>
    /// Sets a heartbeat callback. If additional metadata is provided by the server in the heartbeat message, this metadata is provided as an argument.
    /// </summary>
    /// <param name="onServerHeartbeatReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithServerHeartbeatReceived(Action<ServerHeartbeatEvent>? onServerHeartbeatReceived);

    /// <summary>
    /// Sets a client heartbeat response callback.
    /// </summary>
    /// <param name="onClientHeartbeatResponseReceived"></param>
    /// <returns></returns>
    public StreamingHubClientOptions WithClientHeartbeatResponseReceived(Action<ClientHeartbeatEvent>? onClientHeartbeatResponseReceived);
}
```


## サーバー
サーバーからのハートビートは `MagicOnionOptions` を介してグローバルに有効にするか `Heartbeat` 属性を使用して個別に有効にできます。デフォルトでは無効になっています。

```csharp
// Enable heartbeat for all StreamingHub instances
options.EnableStreamingHubHeartbeat = true;
// Send heartbeat every 30 seconds, disconnect if no response within 5 seconds
options.StreamingHubHeartbeatInterval = TimeSpan.FromSeconds(30);
options.StreamingHubHeartbeatTimeout = TimeSpan.FromSeconds(5);
```

```csharp
// Enable heartbeat using the interval and timeout specified in MagicOnionOptions
[Heartbeat]
public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>
{
}

// Enable heartbeat and override the interval and timeout specified in MagicOnionOptions
[Heartbeat(Interval = 10 * 1000, Timeout = 1000)]
public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>
{
}
```

サーバーサイドのハートビートは「サーバーがハートビートを送信した時刻」を `ServerTime` プロパティに設定します。クライアントはこれを使用してクライアントとサーバーの時刻を同期できます。サーバー時刻は常に UTC で送信されます。

```csharp
// Client-side code:
var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x =>
{
    var serverTime = x.ServerTime; // ServerTime is always UTC.
});
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

## 追加のメタデータ
追加のメタデータをサーバーのハートビートメッセージに追加することができます。これは、例えば、同期目的でサーバー情報を含めるために使用できます。

### サーバー
メタデータをサーバーのハートビートメッセージに追加するには、`IStreamingHubHeartbeatMetadataProvider` インターフェースを実装し、DI コンテナに登録するか、`Heartbeat` 属性の `MetadataProvider` プロパティで指定します。

```csharp
public class CustomHeartbeatMetadataProvider : IStreamingHubHeartbeatMetadataProvider
{
    public bool TryWriteMetadata(IBufferWriter<byte> writer)
    {
        MessagePackSerializer.Serialize(writer, new Version(1, 0, 0, 0));
        return true;
    }
}
```
### クライアント
クライアントサイドでは、`StreamingHubClient` を作成する際にハートビートのコールバックをオプションとして設定できます。

```csharp
var options = StreamingHubClientOptions.CreateWithDefault().WithServerHeartbeatReceived(x =>
{
    var serverVersion = MessagePackSerializer.Deserialize<Version>(x.Metadata);
});
var hub = await StreamingHubClient.ConnectAsync<IChatHub, IChatHubReceiver>(channel, receiver, options);
```

## サーバー上でのハートビートの高度な操作
サーバーからのハートビートは `IMagicOnionHeartbeatFeature` インターフェースを介していくつか高度な機能にアクセスできます。このインターフェースの実装は `IHttpContext.Features` (`Context.CallContext.GetHttpContext().Features`) から取得できます。

### `Unregister` メソッド
`Unregister` メソッドはその StreamingHub のクライアント接続におけるハートビートを無効にします。デバッグ時に一時的にハートビートを無効にする仕組みが必要な場合に利用できます。

### `SetAckCallback` メソッド
クライアントからのハートビートの応答を受信したときのコールバックを設定できます。

### `Latency` プロパティ
クライアントとのレイテンシーを取得します。送受信されていない場合は `TimeSpan.Zero` が返されます。

## 制限事項

ハートビートは、v7.0.0 より前のクライアントやサーバーと互換性がありません。v7.0.0 の前後のバージョンが混在する可能性がある StreamingHub で有効にしないでください。
