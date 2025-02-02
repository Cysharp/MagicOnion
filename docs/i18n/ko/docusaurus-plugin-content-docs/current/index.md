---
title: MagicOnion 에 대해
---

# MagicOnion

.NET 플랫폼과 Unity를 위한 통합 Realtime/API 프레임워크입니다.

## MagicOnion 에 대해

MagicOnion은 SignalR이나 Socket.io, WCF, Web 기반 API와 같은 RPC 메커니즘과 마찬가지로 .NET 플랫폼에 양방향 실시간 통신을 제공하는 최신 RPC 프레임워크입니다.

이 프레임워크는 gRPC를 기반으로 하며, 빠르고 컴팩트한 네트워크 전송인 HTTP2를 기반으로 합니다. 그러나 일반 gRPC와 달리 C 인터페이스를 프로토콜 스키마로 취급하여 `.proto`(Protocol Buffers IDL) 없이 C# 프로젝트 간 코드 공유를 실현합니다.

인터페이스는 스키마이며, 일반 C# 코드와 마찬가지로 API 서비스를 제공합니다.

![image](https://user-images.githubusercontent.com/46207/50965239-c4fdb000-1514-11e9-8365-304c776ffd77.png)

StreamingHub 실시간 통신 서비스를 통해 서버는 여러 클라이언트에 데이터를 전달할 수 있습니다.

![image](https://user-images.githubusercontent.com/46207/50965825-7bae6000-1516-11e9-9501-dc91582f4d1b.png)

MagicOnion은 [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp)를 사용하여 호출의 인수와 반환값을 직렬화합니다. MessagePack 객체로 직렬화할 수 있는 .NET 프리미티브 및 기타 복잡한 타입을 사용할 수 있습니다. 직렬화에 대한 자세한 내용은 MessagePack for C#을 참조하십시오.

## 사용 사례

MagicOnion은 다음과 같은 사용 사례에서 채택하거나 대체할 수 있습니다:

- RPC 서비스 (마이크로서비스에서 사용되는 gRPC, WinForms/WPF에서 일반적으로 사용되는 WCF와 같은 것들이 있습니다)
- Windows에서의 WPF 애플리케이션, Unity 게임, .NET for iOS, Android, .NET MAUI 등 다양한 플랫폼과 클라이언트를 대상으로 하는 ASP.NET Core Web API가 커버하는 API 서비스
- Socket.io, SignalR, Photon, UNet 등 양방향 실시간 커뮤니케이션

MagicOnion은 API 서비스와 실시간 통신을 모두 지원하기 때문에 다양한 사용 사례에 적합합니다. 두 기능 중 하나만 사용할 수도 있지만, 두 기능을 결합한 구성도 지원됩니다.

![](/img/docs/fig-usecase.png)

## 기술 스택

MagicOnion은 다양한 최신 기술 위에 구축되어 있습니다.

![](/img/docs/fig-technology-stack.png)

서버는 ASP.NET Core 위에 구현된 gRPC 서버(grpc-dotnet) 위에 구현되며, ASP.NET Core와 그 위에 있는 Grpc.AspNetCore.Server의 기능을 활용합니다. 여기에는 DI, 로깅(Logging), 매트릭(Metrics) 및 Hosting API 등이 포함됩니다.

네트워크는 HTTP2 프로토콜을 사용하며, gRPC에 의한 바이너리 메시징을 활용합니다. 바이너리 표현은 gRPC에서 흔히 사용되는 Protocol Buffers가 아닌 .NET과의 친화성과 표현력이 높은 MessagePack을 채택하고 있습니다.

클라이언트는 .NET 런타임뿐만 아니라 Unity 게임 엔진의 런타임도 지원합니다. 이러한 런타임 위에서 .NET 표준 HttpClient를 기반으로 한 gRPC 클라이언트(grpc-dotnet)를 사용하며, 이를 바탕으로 MagicOnion의 클라이언트 라이브러리를 구축하고 있습니다.
