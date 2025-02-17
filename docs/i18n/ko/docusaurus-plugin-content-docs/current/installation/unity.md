# Unity 환경에서 사용하기
MagicOnion 클라이언트는 Unity 버전 2022.3.0f1(LTS) 이상을 지원합니다.

`.NET Standard 2.1` 프로파일과 IL2CPP에 의한 빌드, 플랫폼으로 Windows, Android, iOS, macOS를 지원합니다. 현 시점에서 그 외의 WebGL이나 콘솔과 같은 환경에서의 동작은 지원하지 않습니다.

MagicOnion을 Unity 클라이언트에서 사용하려면 다음 라이브러리를 설치해야 합니다:

- NuGetForUnity
- YetAnotherHttpHandler
- gRPC library
- MagicOnion.Client
- MagicOnion.Client.Unity

## NuGetForUnity 설치

MagicOnion은 NuGet 패키지로 제공되므로, Unity에서 NuGet 패키지를 설치하기 위한 확장인 [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)를 설치합니다. 설치 절차는 NuGetForUnity의 README를 참조하시기 바랍니다.

## YetAnotherHttpHandler와 gRPC 라이브러리 설치

gRPC 프로젝트에서 C-core 기반의 Unity용 라이브러리 개발이 종료되었기 때문에, [YetAnotherHttpHandler](https://github.com/Cysharp/YetAnotherHttpHandler)를 사용해야 합니다. 설치 절차는 [YetAnotherHttpHandler의 README](https://github.com/Cysharp/YetAnotherHttpHandler)를 참조하시기 바랍니다. [grpc-dotnet (Grpc.Net.Client)의 설치 방법](https://github.com/Cysharp/YetAnotherHttpHandler#using-grpc-grpc-dotnet-library)에 대해서도 설명하고 있습니다.

## MagicOnion.Client 설치
Unity에서 MagicOnion의 클라이언트를 사용하려면 NuGet 패키지와 Unity용 확장 Unity 패키지 두 가지를 설치해야 합니다.

먼저 NuGetForUnity를 사용하여 MagicOnion.Client 패키지를 설치합니다.

다음으로 Unity용 확장 패키지를 Unity Package Manager를 사용하여 설치합니다. 설치하려면 Unity Package Manager의 "Add package from git URL..."에 다음 URL을 지정하십시오. 필요에 따라 버전 태그를 지정하시기 바랍니다.

```
https://github.com/Cysharp/MagicOnion.git?path=src/MagicOnion.Client.Unity/Assets/Scripts/MagicOnion.Client.Unity#{Version}
```

:::note
`{Version}`을 설치하고자 하는 버전 번호(예: `7.0.0`)로 교체하시기 바랍니다.
:::

## 클라이언트 사용 방법
gRPC 채널을 생성할 때 아래와 같이 YetAnotherHttpHandler를 사용하도록 변경해야 합니다.

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5000", new GrpcChannelOptions
{
    HttpHandler = new YetAnotherHttpHandler()
    {
        // If you want to use HTTP/2 over cleartext (h2c), set `Http2Only = true`.
        // Http2Only = true,
    },
    DisposeHttpClient = true,
});
var client = MagicOnionClient.Create<IMyFirstService>(channel);
```

Unity의 경우, GrpcChannel을 래핑하고 개발에 더 유용한 기능들을 제공하는 확장기능들을 제공합니다. 자세한 내용은 [Unity 통합](../integration/unity) 페이지를 참조하시기 바랍니다.

## IL2CPP에서 작동

Unity 프로젝트가 스크립팅 백엔드로 IL2CPP를 사용하는 경우, 추가 설정이 필요합니다. 자세한 내용은 [Source Generator를 사용한 Ahead-of-Time 컴파일 지원](../source-generator/client) 페이지를 참조하시기 바랍니다.
