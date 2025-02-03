# Unary와 StreamingHub

MagicOnion은 Unary 서비스와 StreamingHub 서비스의 2가지 API 구현 방식을 제공합니다. 이들 중 어느 것을 사용하더라도 RPC 스타일의 API를 정의할 수 있습니다.

Unary와 StreamingHub의 차이점은 다음과 같습니다:

- Unary는 한 번에 1개의 요청(Request)과 1개의 응답(Response)을 처리하는 단순한 HTTP POST 요청입니다.
    - 상세: [Unary 서비스 기본 사항](/unary/fundamentals)
- StreamingHub는 지속적인 연결을 사용하여 클라이언트와 서버 간에 메시지를 주고받는 양방향 통신입니다.
    - 상세: [StreamingHub 서비스 기본 사항](/streaminghub/fundamentals)

![](/img/docs/fig-unary-streaminghub.png)

모든 것을 StreamingHub로 구현하는 것도 가능하지만, 서버로부터의 알림이 필요하지 않은 일반적인 API(REST나 Web API 대신)에서는 Unary를 사용하는 것을 권장합니다.

## Unary의 장점

- 로드밸런싱과 Observability
    - Unary는 실질적으로 HTTP POST 호출이므로 ASP.NET Core를 비롯해, 로드밸런서나 CDN, WAF와 같은 기존 시스템과의 친화성이 있습니다.
    - StreamingHub는 하나의 장시간 요청이며, 내부 Hub 메서드 호출을 로그로 남기는 것은 인프라 수준에서는 어렵습니다.
        - 이는 Hub 메서드의 호출을 로드밸런싱할 수 없다는 것을 의미합니다.
- StreamingHub의 오버헤드
    - Unary의 실체는 단순한 HTTP POST이며, StreamingHub와 같은 연결 설정이 필요하지 않습니다.
    - StreamingHub는 연결 시에 메시지 루프의 시작과 하트비트(heartbeat) 설정과 같은 추가 처리를 수행하는 오버헤드가 있습니다.

## StreamingHub의 장점

- 서버에서 클라이언트(복수 포함)로의 실시간 메시지 전송
    - 서버에서 클라이언트에 대한 알림이 필요한 경우에는 StreamingHub의 사용을 검토해 주세요.
    - 예를 들어 채팅의 메시지 알림이나 게임의 위치 동기화 등이 해당됩니다.
    - Unary나 일반 HTTP 요청의 경우에 필요한 폴링(polling)/롱폴링(long polling)의 대안이 됩니다.
