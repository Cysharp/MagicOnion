# 주요 특징

MagicOnion에는 다음과 같은 특징이 있습니다.

- 마찰이 적은 통일된 개발 경험
    - .NET / C#의 타입을 사용한 통일된 서비스 정의와 투명한 프록시 생성
- RPC 스타일의 API 서비스
- 실시간 통신
    - 서버에서 클라이언트로의 실시간 알림
    - 여러 클라이언트의 동시 호출
    - 하트비트(haertbeat)를 통한 연결 해제 감지
    - Redis/NATS를 사용한 다중 클라이언트 호출
- Ecosystem
    - 클라이언트로 .NET 및 Unity 지원
    - 서버로서 ASP.NET Core 위에 구축하고, .NET Ecosystem을 활용
    - gRPC over HTTP2를 기반으로 한 효율적인 통신 및 상호 운용성
    - MessagePack을 통한 효율적인 바이너리 직렬화
        - 직렬화 확장 포인트 제공
- MIT 라이선스가 부여된 오픈소스 라이브러리
