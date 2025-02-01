# 切断のハンドリング

StreamingHub はクライアントとサーバーサイドで切断を検出する仕組みを持っています。これは StreamingHub がクライアントとサーバー間で通信するために連続した接続を確立し、それは常に何らかの理由で切断される可能性があるためです。MagicOnion はアプリケーションがサーバーとクライアントの両方で切断を検出する仕組みを提供します。

:::tip
切断はサーバーとクライアントの両方で異なるタイミングで検出されます。つまりクライアントが切断を検出しても、サーバーがまだそれを認識していない可能性があります。そのためクライアントとサーバーの両方で切断を検出することが重要です。
:::

## サーバーでの切断の検出
サーバーサイドで StreamingHub からの切断を検出したい場合は、`OnDisconnected` メソッドを使用します。

```csharp
protected override ValueTask OnDisconnected()
{
    return ValueTask.CompletedTask;
}
```

## クライアントでの切断の検出
クライアントサイドで StreamingHub からの切断を検出したい場合は、`WaitForDisconnected` または `WaitForDisconnectAsync` API を使用します。これらの APIは、StreamingHub クライアントが何らかの理由で切断されるまで待機する `Task` を返します。

```csharp
using MagicOnion.Client;

var client = await StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, receiver);
_ = WaitForDisconnectEventAsync();

async Task WaitForDisconnectEventAsync()
{
    var reason = await client.WaitForDisconnectAsync();
    if (reason.Type != DisconnectionType.CompletedNormally)
    {
        ...
    }
}
```

### `WaitForDisconnectAsync` API
`WaitForDisconnectAsync` API は既存の `WaitForDisconnected` の更新バージョンであり、切断理由を受け取ることが可能になります。

`WaitForDisconnect` とは異なり、新しい API は `IStreamingHub` インターフェースを変更することなく、`IStreamingHubMarker` インターフェースの拡張メソッドとして追加されています。これはバイナリ互換性を壊さないようにするためです。

### APIs
```csharp
namespace MagicOnion.Client
{
    public static class StreamingHubClientExtensions
    {
        /// <summary>
        /// Wait for the disconnection and return the reason.
        /// </summary>
        public static Task<DisconnectionReason> WaitForDisconnectAsync<TStreamingHub>(this TStreamingHub hub) where TStreamingHub : IStreamingHubMarker =>
    }

    /// <summary>
    /// Provides the reason for the StreamingHub disconnection.
    /// </summary>
    public readonly struct DisconnectionReason
    {
        /// <summary>
        /// Gets the type of StreamingHub disconnection.
        /// </summary>
        public DisconnectionType Type { get; }

        /// <summary>
        /// Gets the exception that caused the disconnection.
        /// </summary>
        public Exception? Exception { get; }
    }

    /// <summary>
    /// Defines the types of StreamingHub disconnection.
    /// </summary>
    public enum DisconnectionType
    {
        /// <summary>
        /// Disconnected after completing successfully.
        /// </summary>
        CompletedNormally = 0,

        /// <summary>
        /// Disconnected due to exception while reading messages.
        /// </summary>
        Faulted = 1,

        /// <summary>
        /// Disconnected due to reaching the heartbeat timeout.
        /// </summary>
        TimedOut = 2,
    }
}
```

## 切断検知の改善
通信状況の悪化や切断の早期検知を実現するために MagicOnion はハートビート機能を提供しています。ハートビート機能については [ハートビート](heartbeat) を参照してください。

## 再接続
StreamingHub クライアントは自動での再接続を行いません。アプリケーションは切断されたことを検知して必要に応じて再接続(再作成)をする必要があります。サーバー上の StreamingHub は再開することができないため、再接続後のユーザーやアプリケーションの状態に合わせて再開処理を行ってください。

```csharp
async ValueTask KeepConnectionAsync(CancellationToken shutdownToken)
{
    while (!shutdownToken.IsCancellationRequested)
    {
        try
        {
            var client = await StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, receiver);
            await client.WaitForDisconnectAsync();
        }
        catch (RpcException ex)
        {
            ...
        }
    }
}
```

StreamingHubClient は再接続時には作り直す必要があるため、フィールドに保持していたり、外部に公開している場合などは注意が必要です。
