# HTTPS/TLS

MagicOnion は TLS による暗号化接続をサポートしています。このページでは MagicOnion での TLS 暗号化接続の設定方法について説明します。

## サーバー
サーバー側の HTTPS 暗号化設定は ASP.NET Core に従います。詳細は [ASP.NET Core で HTTPS を強制する | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl) を参照してください。

## クライアント
クライアント側の HTTPS 暗号化はランタイムが .NET Framework/.NET 8 以降あるいは Unity かどうかによって挙動や設定が異なります。これは開発用の証明書を使用する場合に影響します。

### .NET Framework または .NET 8+
.NET Framework または .NET 環境での証明書の取り扱いは `HttpClient` の標準的な挙動と同様で OS の証明書ストアを使用します。たとえば、Windows では Windows の証明書ストアを使用して証明書を検証します。

### Unity
クライアントが Unity の場合、MagicOnion では YetAnotherHttpHandler の利用を推奨しています。YetAnotherHttpHandler は独自の証明書ストアを持っているため開発用証明書を使用する場合には追加の設定が必要です。詳しくは [YetAnotherHttpHandler のドキュメント](https://github.com/Cysharp/YetAnotherHttpHandler?tab=readme-ov-file#advanced) を参照してください。


## TLS を使用しない非暗号化 HTTP 接続を使用する
原則としてサーバーとクライアントの接続には HTTPS の使用を推奨しますが、開発時など一時的に非暗号化接続を設定したい場合があります。設定を変更することで非暗号化の HTTP/2 接続を使用できます (非暗号化 HTTP/2 は HTTP/2 over cleartext (h2c) と呼ばれます)。

### サーバー
サーバーが非暗号化の HTTP/2 を受け入れるようにするには、Kestrel でエンドポイントを設定する必要があります。エンドポイントは `appsettings.json` で設定するか、ソースコードで直接設定できます。

```json
{
    ...
    "Kestrel": {
        "Endpoints": {
            "Grpc": {
                "Url": "http://localhost:5000",
                "Protocols": "Http2"
            },
            "Https": {
                "Url": "https://localhost:5001",
                "Protocols": "Http1AndHttp2"
            },
            "Http": {
                "Url": "http://localhost:5002",
                "Protocols": "Http1"
            }
        }
    },
    ...
}
```
または
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http2;
    });
});
```

### クライアント
`GrpcChannel.ForAddress` を呼び出す際に URL スキームを HTTP に変更し、ポート番号を暗号化されていないポートに変更してください。

```csharp
var channel = GrpcChannel.ForAddress("http://localhost:5000");
```

詳しくは [Call insecure gRPC services with .NET Core client | Troubleshoot gRPC on .NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client) を参照してください。

#### Unity クライアントでの追加の対応
Unity で YetAnotherHttpHandler を使用している場合は、YetAnotherHttpHandler のインスタンスを作成する際に `Http2Only` オプションを指定するよう変更する必要があります。

```csharp
var handler = new YetAnotherHttpHandler(new()
{
    Http2Only = true,
});
```


### 制限事項
非暗号化の HTTP/2 接続を受け入れる場合、同じポートで HTTP/1 と HTTP/2 を提供することはできません。TLS を有効にした場合、HTTP/2 のネゴシエーションには ALPN が使用されますが、非 TLS の場合はこれができないためです。

ウェブサイトや API をホストする際に HTTP/1 と HTTP/2 が共存するようにしたい場合は、Kestrel を設定して複数のポートでリッスンすることで実現できます。
