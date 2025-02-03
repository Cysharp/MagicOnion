# Unity
MagicOnion은 Unity와의 통합을 지원합니다. 이는 IL2CPP 지원뿐만 아니라, Unity 애플리케이션에서 MagicOnion을 더 사용하기 쉽게 하는 기능도 포함됩니다.

## Unity에서의 gRPC 채널 관리 통합
MagicOnion은 gRPC 채널을 래핑하고, Unity의 라이프사이클에서 관리하는 구조를 제공합니다.
이를 통해 애플리케이션이나 Unity 에디터가 프리즈되는 것을 방지하고, 채널과 StreamingHub를 한 곳에서 해제할 수 있습니다.

또한, 에디터 확장 기능을 제공하여 채널의 통신 상태를 표시하는 기능도 제공합니다.

![](https://user-images.githubusercontent.com/9012/111609638-da21a800-881d-11eb-81b2-33abe80ea497.gif)

:::info
데이터 레이트는 메소드의 메시지 본문만을 대상으로 계산되며, 헤더나 Trailer, 하트비트는 포함되지 않습니다.
:::

### API
- `MagicOnion.GrpcChannelx` 클래스
    - `GrpcChannelx.ForTarget(GrpcChannelTarget)` 메소드
    - `GrpcChannelx.ForAddress(Uri)` 메소드
    - `GrpcChannelx.ForAddress(string)` 메소드
- `MagicOnion.Unity.GrpcChannelProviderHost` 클래스
    - `GrpcChannelProviderHost.Initialize(IGrpcChannelProvider)` 메소드
- `MagicOnion.Unity.IGrpcChannelProvider` 인터페이스
    - `DefaultGrpcChannelProvider` 클래스
    - `LoggingGrpcChannelProvider` 클래스

### 사용 방법
#### 1. Unity 프로젝트에서 `GrpcChannelx` 사용 준비하기
애플리케이션 내에서 채널을 생성하기 전에, 관리되는 프로바이더 호스트(Provider Host)를 초기화해야 합니다.

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
public static void OnRuntimeInitialize()
{
    // Initialize gRPC channel provider when the application is loaded.
    GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(() => new GrpcChannelOptions()
    {
        HttpHandler = new YetAnotherHttpHandler()
        {
            Http2Only = true,
        },
        DisposeHttpClient = true,
    }));
}
```

GrpcChannelProviderHost는 DontDestroyOnLoad로 생성되며, 애플리케이션이 실행 중에는 항상 유지되므로 삭제하지 마세요.

![image](https://user-images.githubusercontent.com/9012/111586444-2eb82980-8804-11eb-8a4f-a898c86e5a60.png)

#### 2. `GrpcChannelx.ForTarget` 또는 `GrpcChannelx.ForAddress`를 사용하여 채널 생성하기
`GrpcChannel.ForAddress` 대신 `GrpcChannelx.ForTarget` 또는 `GrpcChannelx.ForAddress`를 사용하여 채널을 생성합니다.

```csharp
var channel = GrpcChannelx.ForTarget(new GrpcChannelTarget("localhost", 12345, isInsecure: true));
// or
var channel = GrpcChannelx.ForAddress("http://localhost:12345");
```

#### 3. `GrpcChannel` 대신 채널 사용하기
```csharp
var channel = GrpcChannelx.ForAddress("http://localhost:12345");

var serviceClient = MagicOnionClient.Create<IGreeterService>(channel);
var hubClient = StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, this);
```

### Unity Editor 확장 (에디터 윈도우와 인스펙터)
`Window` -> `MagicOnion` -> `gRPC Channels`에서 채널 윈도우를 열 수 있습니다.

![image](https://user-images.githubusercontent.com/9012/111585700-0d0a7280-8803-11eb-8ce3-3b8f9d968c13.png)
