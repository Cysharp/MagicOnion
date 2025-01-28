# メタデータとヘッダー

Unary サービスのリクエストとレスポンスにはメタデータを付加できます。メタデータは HTTP のヘッダーとして扱われ、クライアントとサーバーでそれぞれ設定と読み取りが可能です。メタデータは gRPC の `Metadata` クラスで表現されます。

これはヘッダー認証のような仕組みやバージョン情報など追加で必要な情報をリクエストやレスポンスに付加する際に役立ちます。

## クライアント

### リクエストにメタデータを付加する
リクエストにメタデータを付加するには `MagicOnionClient` の `WithHeaders` メソッドを使用します。以下の例では、`Metadata` クラスを使用してリクエストに `Authorization` ヘッダーを付加しています。

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel).WithHeaders(new Metadata
    { "authorization", "Bearer {token}" }
});
```

クライアント作成時に `MagicOnionClientOptions` クラスを渡すオーバーロードや `MagicOnionClient.WithOptions` メソッドなどで `CallOptions` を使用して設定できます。また、それ以外の方法として [クライアントフィルター](../filter/client-filter) を使用することでリクエスト時にメタデータを設定できます。

### レスポンスのメタデータを読み取る
`UnaryResult` 構造体の `ResponseHeadersAsync` プロパティを使用してメタデータを読み取れます。 `ResponseHeadersAsync` はサーバーが応答するまで待機します。

以下はレスポンスのメタデータを読み取る例です。

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel);
var result = client.SayHelloAsync("Alice", 18);

var headers = await result.ResponseHeadersAsync;
```


## サーバー
サーバーでもクライアントから受信したリクエストのメタデータを読み取ったり、レスポンスにメタデータを付加できます。

### リクエストのメタデータを読み取る

`ServiceContext` クラスの `CallContext.RequestHeaders` プロパティを使用してリクエストのメタデータを読み取れます。

```csharp
if (Context.CallContext.RequestHeaders.GetValue("authorization") is {} authorizationHeader)
{
    ...
}
```

### レスポンスにメタデータを付加する

`ServiceContext` クラスの `CallContext.WriteResponseHeadersAsync` メソッドを使用してレスポンスにメタデータを付加できます。

```csharp
Context.CallContext.WriteResponseHeadersAsync(new Metadata
{
    { "x-server-version", "1.0.0" }
});
```

他にも ASP.NET Core の標準的な方法として `HttpContext` から HTTP ヘッダーとして値をセットすることも可能です。

```csharp
Context.CallContext.GetHttpContext().Response.Headers.TryAdd("x-server-version", "1.0.0");
```
