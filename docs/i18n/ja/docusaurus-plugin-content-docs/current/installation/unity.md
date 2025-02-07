# Unity での利用
MagicOnion のクライアントは Unity のバージョン 2022.3.0f1 (LTS) 以降をサポートしています。

`.NET Standard 2.1` プロファイルと IL2CPP によるビルド、プラットフォームとして Windows, Android, iOS, macOS をサポートします。現時点でそれ以外の WebGL やコンソールといった環境における動作は未サポートです。

MagicOnion を Unity クライアントで使用するには以下のライブラリーをインストールする必要があります:

- NuGetForUnity
- YetAnotherHttpHandler
- gRPC ライブラリ
- MagicOnion.Client
- MagicOnion.Client.Unity

## NuGetForUnity のインストール

MagicOnion は NuGet パッケージで提供されているため、Unity で NuGet パッケージをインストールするための拡張である [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) をインストールします。インストール手順は NuGetForUnity の README を参照してください。

## YetAnotherHttpHandler と gRPC ライブラリのインストール

gRPC プロジェクトで C-core ベースの Unity 向けライブラリの開発が終了したため、[YetAnotherHttpHandler](https://github.com/Cysharp/YetAnotherHttpHandler) を使用する必要があります。インストール手順については [YetAnotherHttpHandler の README](https://github.com/Cysharp/YetAnotherHttpHandler) を参照してください。[grpc-dotnet (Grpc.Net.Client) のインストール方法に関しても説明しています](https://github.com/Cysharp/YetAnotherHttpHandler#using-grpc-grpc-dotnet-library)。

## MagicOnion.Client のインストール
Unity で MagicOnion のクライアントを使用するには NuGet のパッケージと Unity 向け拡張 Unity パッケージの2つをインストールする必要があります。

初めに NuGetForUnity を使用して MagicOnion.Client パッケージをインストールします。

次に Unity 向けの拡張パッケージを Unity Package Manager を使用してインストールします。インストールするには Unity Package Manager の "Add package from git URL..." に以下の URL を指定してください。必要に応じてバージョンタグを指定してください。

```
https://github.com/Cysharp/MagicOnion.git?path=src/MagicOnion.Client.Unity/Assets/Scripts/MagicOnion.Client.Unity#{Version}
```

:::note
`{Version}` をインストールしたいバージョン番号 (例: `7.0.0`) に置き換えてください。
:::

## クライアントの使用方法
gRPC のチャンネルを作成する際に下記の通り YetAnotherHttpHandler を使用するように変更する必要があります。

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5000", new GrpcChannelOptions
{
    HttpHandler = new YetAnotherHttpHandler()
    {
        // If you want to use HTTP/2 over cleartext (h2c), set `Http2Only = true`.
        // Http2Only = true,
    },
    DisposeHttpClient = true,
});
var client = MagicOnionClient.Create<IMyFirstService>(channel);
```

Unity 向けに GrpcChannel をラップした、より開発に役立つ機能を提供する拡張も提供しています。詳しくは [Unity インテグレーション](../integration/unity) ページを参照してください。

## UnityEngine.Vector3 などの Unity 固有型を使用する
`UnityEngine.Vector2`, `UnityEngine.Vector3` などの Unity に固有な型を使用するには追加のパッケージのインストールが必要です。

Unity プロジェクトには MessagePack-CSharp の Unity 向けパッケージの導入が必要となり、サーバーや共有ライブラリーでは Unity 固有の型をサポートするための NuGet パッケージをインストールする必要があります。詳しくは MessagePack-CSharp の [Unity support](https://github.com/MessagePack-CSharp/MessagePack-CSharp?tab=readme-ov-file#unity-support) および [Extensions](https://github.com/MessagePack-CSharp/MessagePack-CSharp?tab=readme-ov-file#extensions) のセクションを参照してください。

## IL2CPP での動作

Unity プロジェクトがスクリプティングバックエンドとして IL2CPP を使用している場合、追加の設定が必要です。詳細については [Source Generator を使用した Ahead-of-Time コンパイルサポート](../source-generator/client) ページを参照してください。
