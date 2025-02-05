# 프로젝트 구성

이 페이지에서는 MagicOnion을 사용한 프로젝트의 추천 구성에 대해 설명합니다.

.NET 애플리케이션은 일반적으로 솔루션이라고 하는 큰 단위로 구성되며, 애플리케이션과 라이브러리와 같은 .NET 프로젝트들을 포함합니다.
MagicOnion을 사용하는 애플리케이션은 일반적으로 서버와 클라이언트 프로젝트를 가지며, 어떤 방식으로든 인터페이스를 공유하도록 구성됩니다.

## 일반적인 .NET 애플리케이션의 구조

본문에는 일반적인 .NET 클라이언트(콘솔 애플리케이션, WPF, .NET MAUI 등)와 서버, 그리고 인터페이스를 공유하는 구성 예시에 대해 설명합니다.

일반적인 .NET 애플리케이션의 프로젝트(솔루션) 구성은 다음과 같습니다.

- **MyApp.sln**: 솔루션 파일
- **MyApp.Server**: MagicOnion 서버 애플리케이션 (ASP.NET Core gRPC Server)
    - 이 프로젝트는 MagicOnion API를 제공하고 클라이언트로부터의 요청을 처리합니다
- **MyApp.Client**: MagicOnion 클라이언트 애플리케이션 (Console, WPF, WinForms, .NET MAUI, etc...)
    - 이 프로젝트는 MagicOnion 서버에 연결하고 요청을 전송합니다
- **MyApp.Shared**: 공유 인터페이스 라이브러리 (.NET 라이브러리 프로젝트)
    - 이 프로젝트는 Hub와 Service 인터페이스 정의, 요청(Request)/응답(Response)에 사용되는 MessagePack 타입 등 MagicOnion을 사용하기 위한 인터페이스 정의를 포함합니다
    -
:::note
MyApp은 프로젝트나 솔루션명으로 변경해주세요.
:::

다음은 위 프로젝트 구조에 대한 디렉토리 구조의 예시입니다.

```plaintext
MyApp.sln
|─ src
|  ├─ MyApp.Server
|  │  ├─ MyApp.Server.csproj
|  |  ├─ Hubs
|  |  |  ├─ ChatHub.cs
|  |  └─ Services
|  |     └─ GreeterService.cs
|  ├─ MyApp.Client
|  │  ├─ MyApp.Client.csproj
|  │  └─ Program.cs
|  └─ MyApp.Shared
|     ├─ MyApp.Shared.csproj
|     ├─ Hubs
|     |  ├─ IChatHub.cs
|     |─ Services
|     |  └─ IGreeterService.cs
|     |─ Requests
|     |  └─ HelloRequest.cs
|     └─ Responses
|        └─ HelloResponse.cs
└─ test
   └─ MyApp.Server.Test
      └─ ...
```

`MyApp.Shared` 프로젝트는 .NET 클래스 라이브러리로 생성되며, `MagicOnion.Abstractions` 패키지를 참조하고, 인터페이스 정의, 데이터 타입, 열거형 타입만을 정의합니다.
그리고 `MyApp.Server`와 `MyApp.Client` 프로젝트는 각각 `MyApp.Shared` 프로젝트를 참조함으로써 인터페이스를 공유합니다.

이는 최소한의 구성 예시이며, 실제 프로젝트에서는 필요에 따라 모델, 도메인, ViewModel과 같은 프로젝트나 계층 구조를 가질 수 있습니다.

## Unity 애플리케이션의 구조

다음은 Unity 클라이언트와 서버 프로젝트 간의 인터페이스를 공유하기 위한 프로젝트 구조의 예시입니다.

Unity 애플리케이션의 구성은 Unity 프로젝트와 서버(.NET) 프로젝트 간의 인터페이스를 공유하는 방법이 일반적인 .NET 프로젝트와 다릅니다.
이는 Unity 프로젝트에서 .NET 라이브러리 프로젝트를 참조할 수 없다는 등의 이유 때문입니다.

권장하는 구성은 다음과 같습니다.

- **MyApp.Server.sln**: 솔루션 파일
- **MyApp.Server**: MagicOnion 서버 애플리케이션 (ASP.NET Core gRPC Server)
- **MyApp.Unity**: Unity 클라이언트 애플리케이션
- **MyApp.Shared**: 공유 인터페이스 라이브러리 (.NET 라이브러리 프로젝트와 Unity 로컬 패키지)

```plaintext
MyApp.Server.sln
|─ src
|   ├─ MyApp.Server
|   │  ├─ MyApp.Server.csproj
|   │  ├─ Program.cs
|   │  ├─ Hubs
|   │  │  └─ ChatHub.cs
|   │  └─ Services
|   │     └─ GreeterHub.cs
|   ├─ MyApp.Shared
|   │  ├─ MyApp.Shared.csproj
|   │  ├─ package.json
|   │  ├─ Hubs
|   │  │  └─ IChatHub.cs
|   │  └─ Services
|   │     └─ IGreeterHub.cs
|   └─ MyApp.Unity
|      ├─ Assembly-CSharp.csproj
|      ├─ MyApp.Unity.sln
|      └─ Assets
|         └─ ...
└─ test
   └─ MyApp.Server.Test
      └─ ...
```

`MyApp.Shared` 프로젝트는 .NET 클래스 라이브러리로 생성하고, `MagicOnion.Abstractions` 패키지를 참조하며, 인터페이스 정의와 데이터 타입, 열거형만을 정의합니다.
추가로 Unity 프로젝트에서 Unity Package Manager로 참조할 수 있도록 `package.json`을 포함하고, `bin`, `obj`와 같은 폴더가 출력되지 않도록 구성합니다.

`package.json`의 내용은 아래와 같은 최소 구성의 JSON 파일입니다.

```json
{
  "name": "com.cysharp.magiconion.samples.myapp.shared.unity",
  "version": "1.0.0",
  "displayName": "MyApp.Shared.Unity",
  "description": "MyApp.Shared.Unity",
}
```

어셈블리를 분할하기 위해 `MyApp.Shared.Unity.asmdef` (Assembly Definition) 파일을 추가하는 것도 권장합니다. `.Unity` 접미사를 붙이는 등 `MyApp.Shared`와 완전히 같은 이름이 되지 않도록 주의하세요.

다음으로 `MyApp.Shared.csproj`에 아래와 같은 설정을 추가합니다. `bin`, `obj` 디렉토리를 생성하지 않도록 하는 설정과 Unity용 파일을 IDE 상에 표시하지 않도록 하는 설정입니다.

```xml
  <PropertyGroup>
    <!-- https://learn.microsoft.com/en-us/dotnet/core/sdk/artifacts-output -->
    <ArtifactsPath>$(MSBuildThisFileDirectory).artifacts</ArtifactsPath>
  </PropertyGroup>

  <!-- Hide Unity-specific files from Visual Studio and .NET SDK -->
  <ItemGroup>
    <None Remove="**\package.json" />
    <None Remove="**\*.asmdef" />
    <None Remove="**\*.meta" />
  </ItemGroup>

  <!-- Hide build artifacts from Visual Studio and .NET SDK -->
  <ItemGroup>
    <None Remove=".artifacts\**\**.*" />
    <None Remove="obj\**\*.*;bin\**\*.*" />
    <Compile Remove=".artifacts\**\**.*" />
    <Compile Remove="bin\**\*.*;obj\**\*.*" />
    <EmbeddedResource Remove=".artifacts\**\**.*" />
    <EmbeddedResource Remove="bin\**\*.*;obj\**\*.*" />
  </ItemGroup>
```

이 설정으로 인해 중간 파일과 빌드된 파일은 `.artifacts` 아래에 출력되므로, `bin`, `obj` 디렉토리가 생성되어 있는 경우 삭제해 주세요.

다음으로 Unity 프로젝트의 `Packages/manifest.json`에 `MyApp.Shared`에 대한 참조를 추가합니다.

```json
{
  "dependencies": {
    "com.cysharp.magiconion.samples.myapp.shared.unity": "file:../MyApp.Shared/MyApp.Shared.Unity"
  }
}
```

`MyApp.Server` 프로젝트에서는 `MyApp.Shared` 프로젝트를 참조함으로써 인터페이스를 공유합니다. 이는 일반적인 .NET 애플리케이션과 동일합니다.

### SlnMerge를 사용한 솔루션 병합

[SlnMerge](https://github.com/Cysharp/SlnMerge)라는 Unity용 에디터 확장을 사용하면 Unity가 생성하는 솔루션과 .NET 프로젝트의 솔루션을 통합할 수 있습니다.

예를 들어 MyApp.Server.sln에는 MyApp.Server와 MyApp.Shared 프로젝트가 포함되지만, Unity에서 생성되는 솔루션에는 Unity용 프로젝트(Assembly-CSharp, Assembly-CSharp-Editor 등)만 포함됩니다. SlnMerge를 사용하면 이러한 솔루션들을 통합하여, Unity에서 솔루션을 열었을 때도 원활하게 서버 프로젝트를 참조할 수 있게 됩니다.

이를 통해 Unity와 .NET 프로젝트 사이에서 참조 검색이나 디버거 스텝인 같은 기능을 사용할 수 있어 개발 효율성이 향상될 수 있습니다.
