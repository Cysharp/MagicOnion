# サービスのエンドポイントマッピング

## 特定の型やアセンブリーに含まれるサービスのみをマッピングする

MagicOnion のサーバーはデフォルトで起動しているアセンブリーに含まれているサービスを自動的に検索し、見つけたサービスをすべて登録して公開します。しかし場合によっては特定の型や特定のアセンブリーに含まれる型のみを公開したいケースがあります。

`MapMagicOnionService` メソッドには特定の型やアセンブリーに含まれるサービスのみをマッピングするオーバーロードが存在します。このオーバーロードを指定することで手動でサービスの登録できます。

```csharp
app.MapMagicOnionService([ typeof(MyService), typeof(MyHub) ]);
app.MapMagicOnionService([ typeof(MyService).Assembly ]);
```

## エンドポイントメタデータの設定

`MapMagicOnionService` メソッドが返すビルダーでは ASP.NET Core のエンドポイントメタデータを設定するメソッドを利用できます。例えば `RequireHost` や `RequireAuthorization` といったメソッドです。

これにより下記のような形で複数のポートで異なるサービスを提供するといったことも可能です。

```csharp
// Consumers endpoints
app.MapMagicOnionService([typeof(GreeterService), typeof(ChatHub)]);

// Administration endpoints
app.MapMagicOnionService([typeof(AdministrationService)])
    .RequireHost("*:6000")
    .RequireAuthorization();
```
