# ASP.NET Core Blazor

MagicOnion supports scenarios using ASP.NET Core Blazor on the client. The document shows how to use MagicOnion with Blazor, and explains the considerations and limitations.

## Hosting models and Render modes

Blazor has two hosting models, Blazor WebAssembly and Blazor Server, and some MagicOnion features may not be supported.

Also, starting with .NET 8, the actual hosting model is determined by the render mode. For example, if it is Interactive Server mode, it is Blazor Server, and if it is Interactive WebAssembly mode, it is Blazor WebAssembly. There is also an automatic mode that combines both features. The MagicOnion features and support vary depending on the hosting model determined by the render mode.

## Static Server (Blazor Server)
Static Server mode (static SSR) is a mode that returns HTML rendered on the server. In this mode, the MagicOnion client has no special restrictions and works as expected.

However, it is important to note that the Static Server mode is not suitable for handling real-time communication processing such as StreamingHub.

## Interactive Server (Blazor Server)
Interactive Server mode (interactive SSR) is an interactive mode that continuously connects the browser and the server, conveys client operations to the server, and returns HTML rendered on the server. In this mode, the MagicOnion client works without any special restrictions as the code runs on the server where ASP.NET Core is running.

If you want to display real-time communication using StreamingHub, it works without any problems in Interactive Server mode.

## Interactive WebAssembly (Blazor WebAssembly)

Interactive WebAssembly is a mode that runs code on the client side, i.e., in the browser. In this mode, the MagicOnion client runs in the browser, so it cannot directly use gRPC with HTTP/2. Additional configurations are required to avoid this limitation between the client and the server. MagicOnion supports one of the following mechanisms:

- gRPC-Web
- GrpcWebSocketBridge

### Using gRPC-Web

gRPC-Web is a mechanism developed by the gRPC project to use gRPC from a web browser. In .NET, it is available with the Grpc.AspNetCore.Web and Grpc.Net.Client.Web packages. For more information, see [gRPC-Web in ASP.NET Core gRPC apps](https://learn.microsoft.com/en-us/aspnet/core/grpc/grpcweb?view=aspnetcore-9.0).

:::warning
If you use gRPC-Web, only Unary is available, and StreamingHub is not supported. This is a limitation because gRPC-Web does not support Duplex Streaming.
:::

### Using GrpcWebSocketBridge

[GrpcWebSocketBridge](https://github.com/Cysharp/GrpcWebSocketBridge) is a library for performing gRPC communication using WebSocket provided by Cysharp. Unlike gRPC-Web, it supports the operation of StreamingHub.

GrpcWebSocketBridge is based on gRPC-Web, but it is not compatible with the gRPC-Web ecosystem, so it does not support connections from other languages or mechanisms such as proxies based on gRPC-Web.

## Interactive Auto (Blazor Server + WebAssembly)

Interactive Auto mode is a mode that starts as a server interactive at the time of connection, and switches to WebAssembly interactive after the client-side application code is loaded.

This mode has both the characteristics of Blazor Server and Blazor WebAssembly, so you need to support each rendering. In particular, when handling MagicOnion or gRPC, the communication method available depends on whether it is WebAssembly, so you need to support both.

:::info
We recommend using either Interactive Server or WebAssembly mode for the render mode to reduce the complexity of the architecture when implementing interactive applications with MagicOnion.
:::
