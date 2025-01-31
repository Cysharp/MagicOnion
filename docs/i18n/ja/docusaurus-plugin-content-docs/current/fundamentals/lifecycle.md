# ServiceContext とライフサイクル

## ServiceContext

Unary サービスと StreamingHub のメソッド、およびフィルター内では `this.Context` で `ServiceContext` にアクセスできます。

| プロパティ | 型 | 説明 |
| --- | --- | --- |
| Items | `ConcurrentDictionary<string, object>` | リクエスト/接続ごとのオブジェクトストレージ |
| ContextId | `Guid` | リクエスト(Service)ごと/接続(StreamingHub)ごとの一意の ID |
| Timestamp | `DateTime` | リクエスト/接続開始時刻 |
| ServiceType | `Type` | 呼び出されたクラス |
| MethodInfo | `MethodInfo` | 呼び出されたメソッド |
| AttributeLookup | `ILookup<Type, Attribute>` | サービスとメソッドの両方をマージしたキャッシュされた属性 |
| CallContext | `ServerCallContext` | 生の gRPC コンテキスト |
| MessageSerializer | `IMagicOnionSerializer` | 使用しているシリアライザー |
| ServiceProvider | `IServiceProvider` | リクエストに関連付けられたサービスプロバイダー |

`Items` は認証フィルターのような場所から値をセットし、サービスメソッドから取り出すために利用できます。

:::warning
**ServiceContext をキャッシュしないでください。** ServiceContext はリクエスト中にのみ有効であり、MagicOnion はインスタンスを再利用する可能性があります。リクエスト後にコンテキストから参照されるオブジェクトの状態も不定です。
:::

:::warning
ServiceContext は「接続ごと」のコンテキストです。StreamingHub 内でも ServiceContext にアクセスできますが、接続中は常に同じコンテキストを共有するため注意が必要です。例えば Timestamp は接続したときの時刻、メソッドに関連するプロパティーは常に特別なメソッドである `Connect` がセットされます。`Items` プロパティーも Hub メソッド呼び出し単位ではクリアされません。

StreamingHubFilter 内では StreamingHubContext を使用してください。StreamingHubContext は StreamingHub のメソッド呼び出しごとのコンテキストです。
:::

### グローバルな ServiceContext
MagicOnion は HttpContext.Current のようにグローバルに現在のコンテキストを取得できます。`ServiceContext.Current` で取得できますが、`MagicOnionOptions.EnableCurrentContext = true` が必要です。デフォルトは `false` です。

パフォーマンスやコードの複雑性の観点から `ServiceContext.Current` の使用は避けることをお勧めします。

## ライフサイクル

Unary サービスのライフサイクルは疑似コードで以下のようになります。リクエストを受信し処理する度に新しいサービスのインスタンスが作成されます。

```csharp
async Task<Response> UnaryMethod(Request request)
{
    var service = new ServiceImpl();
    var context = new ServiceContext();
    context.Request = request;

    var response = await Filters.Invoke(context, (args) =>
    {
        service.ServiceContext = context;
        return await service.MethodInvoke(args);
    });

    return response;
}
```

StreamingHub サービスのライフサイクルは疑似コードで以下のようになります。接続中、StreamingHub のインスタンスは維持されるためステートを保持できます。

```csharp
async Task StreamingHubMethod()
{
    var context = new ServiceContext();

    var hub = new StreamingHubImpl();
    hub.ServiceContext = context;

    await Filters.Invoke(context, () =>
    {
        var streamingHubContext = new StreamingHubContext(context);
        while (connecting)
        {
            var message = await ReadHubInvokeMessageFromStream();
            streamingHubContext.Intialize(context);

            await StreamingHubFilters.Invoke(streamingHubContext, () =>
            {
                return await hub.MethodInvoke();
            });
        }
    });
}
```
