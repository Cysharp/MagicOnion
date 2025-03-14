# Unity WebGL

MagicOnion은 Unity WebGL 플랫폼을 실험적으로 지원합니다. 이 페이지는 WebGL 플랫폼에서 MagicOnion을 도입하는 방법과 제한 사항을 설명합니다.

## 설치

Unity WebGL 플랫폼에서 MagicOnion을 사용하려면 IL2CPP 지원 외에도 [GrpcWebSocketBridge](https://github.com/Cysharp/GrpcWebSocketBridge)를 설치해야 합니다.

GrpcWebSocketBridge는 WebSocket 상에서 gRPC 통신을 실현하는 라이브러리입니다. 이 라이브러리를 클라이언트와 서버에 도입하면 브라우저에서 MagicOnion 서버와 통신할 수 있습니다.

## 제한 사항
- 현재 클라이언트 측에서의 Heartbeat는 지원되지 않습니다
  - Heartbeat 구현은 내부적으로 스레드 기반 타이머(System.Threading.Timer)를 사용하는 런타임 기능에 의존하는데, Unity WebGL은 스레드를 지원하지 않기 때문에 작동하지 않습니다
