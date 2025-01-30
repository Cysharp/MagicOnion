# MagicOnion

Unified Realtime/API framework for .NET platform and Unity.

https://github.com/Cysharp/MagicOnion

## About MagicOnion

MagicOnion is a modern RPC framework for .NET platform that provides bi-directional real-time communications such as [SignalR](https://github.com/aspnet/AspNetCore/tree/master/src/SignalR) and [Socket.io](https://socket.io/) and RPC mechanisms such as WCF and web-based APIs.

This framework is based on [gRPC](https://grpc.io/), which is a fast and compact binary network transport for HTTP/2. However, unlike plain gRPC, it treats C# interfaces as a protocol schema, enabling seamless code sharing between C# projects without `.proto` (Protocol Buffers IDL).

![image](https://user-images.githubusercontent.com/46207/50965239-c4fdb000-1514-11e9-8365-304c776ffd77.png)

Interfaces are schemas and provide API services, just like the plain C# code

![image](https://user-images.githubusercontent.com/46207/50965825-7bae6000-1516-11e9-9501-dc91582f4d1b.png)

Using the StreamingHub real-time communication service, the server can broadcast data to multiple clients

MagicOnion uses [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) to serialize call arguments and return values. .NET primitives and other complex types that can be serialized into MessagePack objects. See MessagePack for C# for details about serialization.

## Use Cases

MagicOnion can be adopted or replaced in the following use cases:

- RPC services such as gRPC, used by Microservices, and WCF, commonly used by WinForms/WPF
- API services such as ASP.NET Core Web API targeting various platforms and clients such as Windows WPF applications, Unity games, .NET for iOS, Android, and .NET MAUI
- Bi-directional real-time communication such as Socket.io, SignalR, Photon and UNet

MagicOnion supports API services and real-time communication, making it suitable for various use cases. You can use either of these features separately, but configurations that combine both are also supported.

![](/img/docs/fig-usecase.png)

## Technology Stack
TBW
