# HTTPS/TLS

MagicOnion supports encrypted connections using TLS. This page explains how to configure TLS encrypted connections in MagicOnion.

## Server
The server-side HTTPS encryption settings follow ASP.NET Core. For more information, see [Enforcing HTTPS in ASP.NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl).

## Client
The behavior and settings of HTTPS encryption on the client side depend on whether the runtime is .NET Framework/.NET 8 or later or Unity. This affects the use of development certificates.

### .NET Framework or .NET 8+
On .NET Framework or .NET runtimes, certificate handling is the same as the standard behavior of `HttpClient` and uses the OS certificate store. For example, in Windows, certificates are validated using the Windows certificate store.

### Unity
When the client is Unity, MagicOnion recommends using YetAnotherHttpHandler. YetAnotherHttpHandler has its own certificate store, so additional settings are required when using development certificates. For more information, see [YetAnotherHttpHandler README](https://github.com/Cysharp/YetAnotherHttpHandler?tab=readme-ov-file#advanced).

## Using non-encrypted HTTP connections without TLS
In general, we recommend using HTTPS for connections between servers and clients. However, there may be cases where you want to temporarily set up a non-encrypted connection, such as during development. You can configure non-encrypted HTTP/2 connections by changing the settings (non-encrypted HTTP/2 is called HTTP/2 over cleartext (h2c)).

### Server
To accept non-encrypted HTTP/2 connections, you need to configure the endpoint in Kestrel. You can configure the endpoint in `appsettings.

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
builder.WebHost.ConfigureKestrel(options =>
{
    // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http2;
    });
});
```

### Client
You need to change the URL scheme to HTTP and the port number to an unencrypted port when calling `GrpcChannel.ForAddress`.

```csharp
var channel = GrpcChannel.ForAddress("http://localhost:5000");
```

For more information, see [Call insecure gRPC services with .NET Core client | Troubleshoot gRPC on .NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client).

#### Additional configuration for Unity client

When you are using YetAnotherHttpHandler in Unity, you need to specify the `Http2Only` option when creating an instance of YetAnotherHttpHandler.

```csharp
var handler = new YetAnotherHttpHandler(new()
{
    Http2Only = true,
});
```


### Limitations
If you want to accept non-encrypted HTTP/2 connections, you cannot provide HTTP/1 and HTTP/2 on the same port. This is because when accepting non-encrypted HTTP/2 connections, ALPN is used for HTTP/2 negotiation, which cannot be done without TLS.

If you want to host a website or API that supports both HTTP/1 and HTTP/2, you can achieve this by configuring Kestrel to listen on multiple ports.
