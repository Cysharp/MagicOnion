# Unity 拡張
MagicOnion は Unity との統合をサポートしています。これは IL2CPP のサポートだけでなく、Unity アプリケーション上で MagicOnion をより使いやすくするための機能も含まれます。

## Unity での gRPC チャンネル管理の統合
MagicOnion は gRPC チャンネルをラップし、Unity のライフサイクルで管理する仕組みを提供します。
これにより、アプリケーションや Unity エディタがフリーズすることを防ぎ、チャンネルと StreamingHub を一か所で解放することができます。

また、エディタ拡張機能を提供しチャンネルの通信状態を表示する機能も提供します。

![](https://user-images.githubusercontent.com/9012/111609638-da21a800-881d-11eb-81b2-33abe80ea497.gif)

:::info
データレートはメソッドのメッセージ本体のみを対象として計算され、ヘッダーや Trailer、ハートビートは含まれません。
:::

### API
- `MagicOnion.GrpcChannelx` クラス
  - `GrpcChannelx.ForTarget(GrpcChannelTarget)` メソッド
  - `GrpcChannelx.ForAddress(Uri)` メソッド
  - `GrpcChannelx.ForAddress(string)` メソッド
- `MagicOnion.Unity.GrpcChannelProviderHost` クラス
  - `GrpcChannelProviderHost.Initialize(IGrpcChannelProvider)` メソッド
- `MagicOnion.Unity.IGrpcChannelProvider` インターフェース
  - `DefaultGrpcChannelProvider` クラス
  - `LoggingGrpcChannelProvider` クラス

### 使用方法
#### 1. Unity プロジェクトで `GrpcChannelx` を使用する準備をする
アプリケーション内でチャンネルを作成する前に、管理されるプロバイダーホストを初期化する必要があります。

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
public static void OnRuntimeInitialize()
{
    // Initialize gRPC channel provider when the application is loaded.
    GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(() => new GrpcChannelOptions()
    {
        HttpHandler = new YetAnotherHttpHandler()
        {
            Http2Only = true,
        },
        DisposeHttpClient = true,
    }));
}
```

GrpcChannelProviderHost は DontDestroyOnLoad として作成され、アプリケーションが実行中は常に維持されるため削除しないでください。

![image](https://user-images.githubusercontent.com/9012/111586444-2eb82980-8804-11eb-8a4f-a898c86e5a60.png)

#### 2. `GrpcChannelx.ForTarget` または `GrpcChannelx.ForAddress` を使用してチャンネルを作成する
`GrpcChannel.ForAddress` の代わりに `GrpcChannelx.ForTarget` または `GrpcChannelx.ForAddress` を使用してチャンネルを作成します。

```csharp
var channel = GrpcChannelx.ForTarget(new GrpcChannelTarget("localhost", 12345, isInsecure: true));
// or
var channel = GrpcChannelx.ForAddress("http://localhost:12345");
```

#### 3. `GrpcChannel` の代わりにチャンネルを使用する
```csharp
var channel = GrpcChannelx.ForAddress("http://localhost:12345");

var serviceClient = MagicOnionClient.Create<IGreeterService>(channel);
var hubClient = StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, this);
```

### Unity Editor 拡張 (エディターウィンドウとインスペクター)
`Window` -> `MagicOnion` -> `gRPC Channels` からチャンネルウィンドウを開けます。

![image](https://user-images.githubusercontent.com/9012/111585700-0d0a7280-8803-11eb-8ce3-3b8f9d968c13.png)
