# 事前生成されたクライアントコード

MagicOnion は Unity、NativeAOT など事前生成されたクライアントコードが必要なプラットフォームを Source Generator でサポートしています。

Source Generator は MagicOnion.Client パッケージに同梱されているため、以前のバージョンのようなコード生成ツール (moc) のインストールや追加のビルドステップの設定は不要です。

## サポートされている開発環境
- Unity 2022.3.12f1 以降
- .NET 6 以降
- Visual Studio 2022 version 17.2 以降
- Rider 2023.1 以降

## 使用方法
任意の名前の `partial` クラスをアプリケーション内に定義します。そして `MagicOnionClientGeneration` 属性を付け、サービスインターフェースを検索する対象となるアセンブリ内の任意の型を指定します。

たとえば `MyApp.Shared` アセンブリに `MyApp.Shared.Services.IGreeterService` と `MyApp.Shared.Hubs.IChatHub` が含まれている場合そのうちの一つを指定します。

```csharp
using MagicOnion.Client;

[MagicOnionClientGeneration(typeof(MyApp.Shared.Services.IGreeterService))]
partial class MagicOnionGeneratedClientInitializer {}
```

MagicOnion は起動時に生成されるクライアントの引数と戻り値の型に対応するフォーマッターファクトリーを登録します。それらは MessagePack の `Default` と `DefaultAot` から利用できます。Native AOT や IL2CPP で独自のジェネリックなオブジェクトをシリアライズのルートとして使う場合は必要な閉じた型を `MessagePackSerializable` で宣言してください。詳しくは [MessagePack v4 移行ガイド](https://github.com/MessagePack-CSharp/MessagePack-CSharp/tree/v4.0.0-rc.1#migrating-from-v3) を参照してください。

## ソース生成オプション
コンストラクターの名前付き引数でオプションを指定できます。

- **`DisableAutoRegistration`**: 起動時に `<MagicOnionClientGeneration属性を付加したクラス>.Register` メソッドを自動的に呼び出す機能を無効にするかどうかを設定します (自動登録は .NET 5+ または Unity でのみ機能します)
    - このオプションにより `Register` を呼び出さなかった場合、手動で呼び出すかクライアント作成時に `ClientFactoryProvider` または `StreamingHubClientFactoryProvider` プロパティーを渡す必要があります
- **`Serializer`**: シリアライズに使用するシリアライザーを設定します。デフォルトは `GenerateSerializerType.MessagePack` です

### 追加のオプション

`MagicOnionClientGenerationOption` 属性で追加のオプションを指定できます。

MessagePack 向けのオプションで、独自の `MessagePackFormatterFactory` クラスの登録を設定できます。

- **`MessagePack.FormatterNamespace`** は独自のフォーマッターファクトリーのルート名前空間を指定します。既定値は `MessagePack.Formatters` です。
- **`MessagePack.GenerateResolverForCustomFormatter`** を有効にすると、ジェネリック RPC 型に対応する `<TypeName>FormatterFactory<T>` が登録されます。既定値は `false` です。
