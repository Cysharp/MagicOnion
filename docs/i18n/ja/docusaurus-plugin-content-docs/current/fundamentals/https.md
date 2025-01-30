# HTTPS/TLS
TBW

MagicOnion は TLS による暗号化接続をサポートしています。このページでは MagicOnion での TLS 暗号化接続の設定方法について説明します。

## サーバー
サーバー側の HTTPS 暗号化設定は ASP.NET Core に従います。詳細は [ASP.NET Core で HTTPS を強制する | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl) を参照してください。

## クライアント
クライアント側の HTTPS 暗号化設定はランタイムが .NET Framework/.NET 8 以降か Unity かどうかによって異なります。

### .NET Framework か .NET 8+
クライアントが .NET Standard 2.1 かそれ以降をサポートしている場合、MagicOnion は gRPC 接続に `Grpc.Net.Client` (純粋な C# 実装) を使用します。

`Grpc.Net.Client` は内部で `HttpClient` を使用しているため、証明書の扱いは `HttpClient` と同じです。たとえば、Windows では Windows の証明書ストアを使用して証明書を検証します。

### Unity
クライアントが Unity の場合、MagicOnion では YetAnotherHttpHandler の利用を推奨しています。YetAnotherHttpHandler は独自の証明書ストアを持っているため開発用証明書を使用する場合には追加の設定が必要です。詳しくは [YetAnotherHttpHandler のドキュメント](https://github.com/Cysharp/YetAnotherHttpHandler?tab=readme-ov-file#advanced) を参照してください。


## Use HTTP unencrypted connection without TLS
It is recommended to use HTTPS for server-client connection, but in some cases during development you may want to configure unencrypted connection. Also, you need to configure unencrypted connection in macOS because ALPN over TLS is not supported.

### Server
To allow your server to accept unencrypted HTTP/2, you must configure an endpoint to listen to Kestrel. Endpoints can be configured either by using `appsettings.json` or directly in the source code.

See also [Unable to start ASP.NET Core gRPC app on macOS | Troubleshoot gRPC on .NET Core](https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot#unable-to-start-aspnet-core-grpc-app-on-macos) for details.

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
or
```csharp
builder.WebHost.UseKestrel(options =>
{
    // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http2;
    });
});
```

#### Client (.NET Standard 2.1 or .NET 6+; Grpc.Net.Client)
When calling `GrpcChannel.ForAddress`, change the URL scheme to HTTP and the port to an unencrypted port.

```csharp
var channel = GrpcChannel.ForAddress("http://localhost:5000");
```

See also [Call insecure gRPC services with .NET Core client | Troubleshoot gRPC on .NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client) for details.

#### Client (Unity)
YetAnotherHttpHandler のインスタンスを作成する際に `Http2Only` オプションを指定してください。

```csharp
var handler = new YetAnotherHttpHandler(new()
{
    Http2Only = true
});
```

When calling `GrpcChannel.ForAddress`, change the URL scheme to HTTP and the port to an unencrypted port.

```csharp
var channel = GrpcChannel.ForAddress("http://localhost:5000");
```

#### Limitations
If unencrypted HTTP/2 connection is accepted, HTTP/1 and HTTP/2 cannot be served on the same port.
When TLS is enabled, ALPN is used for HTTP/2 negotiation, but with non-TLS, this is not possible.

If you want HTTP/1 and HTTP/2 to work together for the convenience of hosting a web site or API, you can listen on multiple ports by configuring Kestrel.
