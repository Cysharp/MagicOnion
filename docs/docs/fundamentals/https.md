# HTTPS/TLS
TBW

MagicOnion supports TLS encrypted connection.

### Server-side
In general, HTTPS encryption settings on the server follow ASP.NET Core. For more information, see [Enforce HTTPS in ASP.NET Core | Microsoft Docs](https://docs.microsoft.com/ja-jp/aspnet/core/security/enforcing-ssl).

> **NOTE**: The limitations on macOS environment and when running on Docker are also described in ASP.NET Core documentation.

### Client-side
Depending on whether the client supports .NET Standard 2.1 or .NET Standard 2.1 (including Unity), the configuration is different.

#### .NET Standard 2.1 or .NET 6+
If the client supports .NET Standard 2.1 or newer, MagicOnion uses `Grpc.Net.Client` (a pure C# implementation) for gRPC connection.

Grpc.Net.Client uses `HttpClient` internally, so it handles certificates the same way as `HttpClient`. For example, on Windows, it uses Windows's certificate store to validate certificates.

#### .NET Standard 2.0 (.NET Framework 4.6.1+)
If the client supports .NET Standard 2.0, MagicOnion uses `Grpc.Core` (C-library binding) for gRPC connection.

Grpc.Core has its [own certificate store built into the library](https://github.com/grpc/grpc/blob/master/etc/roots.pem) and uses it unless you specify a certificate. This certificate store contains common CAs and is rarely a problem in production environment.

However, there is a problem when connecting with a server using [ASP.NET Core development certificate](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos). For example, if you see the following exceptions when you try to connect, the server certificate validation may have failed.

```
Grpc.Core.RpcException: 'Status(StatusCode="Unavailable", Detail="failed to connect to all addresses", ...')
```

The following workarounds are suggested for such cases:

- Issue and configure a trusted certificate to the server
- Use OpenSSL commands to issue and configure self-signed certificates to servers and clients
- Unencrypted connection without TLS

### Use HTTP unencrypted connection without TLS
It is recommended to use HTTPS for server-client connection, but in some cases during development you may want to configure unencrypted connection. Also, you need to configure unencrypted connection in macOS because ALPN over TLS is not supported.

#### Server-side
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

```csharp
webBuilder
    .UseKestrel(options =>
    {
        // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
        options.ConfigureEndpointDefaults(endpointOptions =>
        {
            endpointOptions.Protocols = HttpProtocols.Http2;
        });
    })
    .UseStartup<Startup>();
```

#### Client-side (.NET Standard 2.1 or .NET 6+; Grpc.Net.Client)
When calling `GrpcChannel.ForAddress`, change the URL scheme to HTTP and the port to an unencrypted port.

```csharp
var channel = GrpcChannel.ForAddress("http://localhost:5000");
```

See also [Call insecure gRPC services with .NET Core client | Troubleshoot gRPC on .NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client) for details.

#### Limitations
If unencrypted HTTP/2 connection is accepted, HTTP/1 and HTTP/2 cannot be served on the same port.
When TLS is enabled, ALPN is used for HTTP/2 negotiation, but with non-TLS, this is not possible.

If you want HTTP/1 and HTTP/2 to work together for the convenience of hosting a web site or API, you can listen on multiple ports by configuring Kestrel.
