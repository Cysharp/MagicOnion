import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# Unity와 .NET 서버로 시작하기

이 가이드에서는 MagicOnion을 사용한 간단한 Unity 애플리케이션의 클라이언트와 .NET 서버를 만드는 방법을 설명합니다. 여기서는 .NET 서버에 구현된 두 숫자를 더하는 API 서비스를 Unity 클라이언트에서 호출하는 간단한 구현을 수행합니다.

이 가이드는 서버와 클라이언트를 만들기 위해 다음과 같은 환경을 가정합니다

- Windows 또는 macOS
- .NET 8 SDK 이상
- Unity 2022.3 이상 또는 Unity 6 (6000.0.34f1) 이상

:::note
Unity 6의 일부 버전에서는 Source Generator 관련 문제가 있으므로 6000.0.34f1 이상을 사용해 주세요.
:::

## 미리 구성된 템플릿 사용하기 (선택사항)
이 가이드는 프로젝트 생성, 패키지 설치 및 기타 단계들을 보여줍니다. 이 가이드의 완성된 상태의 템플릿을 [MagicOnion.Template.Unity](https://github.com/Cysharp/MagicOnion.Template.Unity) 리포지토리에서 확인 하실 수 있습니다.

템플릿을 사용한 개발을 시작하려면 GitHub에서 리포지토리를 아카이브 파일로 다운로드하거나 GitHub 템플릿에서 리포지토리를 생성하면 됩니다. 자세한 내용은 [GitHub에서 템플릿으로 리포지토리를 생성하는 방법](https://docs.github.com/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template)을 참조하세요.

<details>
<summary>템플릿 사용 방법</summary>

이 템플릿은 Unity 6000.0.36f1을 사용한 "Universal 3D" 템플릿을 기반으로 합니다.

### 설정 방법

GitHub에서 아카이브 파일을 다운로드하여 압축을 풀거나 GitHub 템플릿 기능으로 리포지토리를 생성할 수 있습니다. 다음은 `MyApp` 디렉터리에 템플릿을 압축 해제하는 명령어 예시입니다.

<Tabs groupId="shell">
  <TabItem value="cmd" label="Windows (cmd.exe)" default>
    ```bash
    mkdir MyApp
    cd MyApp
    curl.exe -L -o - https://github.com/Cysharp/MagicOnion.Template.Unity/archive/refs/heads/main.tar.gz | tar xz -C . --strip-component 1
    ```
  </TabItem>
    <TabItem value="pwsh" label="Windows (PowerShell)" default>
    ```powershell
    mkdir MyApp
    cd MyApp
    curl.exe -L -o - https://github.com/Cysharp/MagicOnion.Template.Unity/archive/refs/heads/main.tar.gz | tar xz -C . --strip-component 1
    ```
  </TabItem>
  <TabItem value="unix-shell" label="Bash, zsh">
    ```bash
    mkdir MyApp
    cd MyApp
    curl -L -o - https://github.com/Cysharp/MagicOnion.Template.Unity/archive/refs/heads/main.tar.gz | tar xz -C . --strip-component 1
    ```
  </TabItem>
</Tabs>

소스 코드를 압축 해제한 후, `init.cmd` 또는 `init.sh`를 임의의 프로젝트 이름(예: `MyApp`)과 함께 실행하세요. 이 스크립트는 리포지토리 루트에서 프로젝트와 파일의 이름 변경과 같은 준비 작업을 수행합니다.

<Tabs groupId="shell">
  <TabItem value="cmd" label="Windows (cmd.exe)" default>
    ```bash
    init.cmd MyApp
    ```
  </TabItem>
    <TabItem value="pwsh" label="Windows (PowerShell)" default>
    ```bash
    init.cmd MyApp
    ```
  </TabItem>
  <TabItem value="unix-shell" label="Bash, zsh">
    ```bash
    bash init.sh MyApp
    ```
  </TabItem>
</Tabs>

스크립트 실행 후에는 `init.sh` 및 `init.cmd`, 준비 작업을 수행하는 `tools/RepoInitializer`를 삭제할 수 있습니다. 준비 작업을 완료한 후에는 Unity Hub에서 `src/MyApp.Unity` 디렉터리를 Unity 프로젝트로 열어주세요.

- Unity Hub에서 `src/MyApp.Unity`를 Unity 프로젝트로 열어주세요.
- 샘플 구현은 `SampleScene`에 포함되어 있습니다.
- 프로젝트를 열려면 Unity Editor의 메뉴에서 `Assets` -> `Open C# Project`를 선택하세요.
- 서버를 실행하려면 Visual Studio 또는 Rider에서 `MyApp.Server` 프로젝트를 시작하세요.

템플릿은 이 퀵스타트 가이드의 내용을 기반으로 구성되어 있으므로, 자세한 내용은 이 가이드를 참조하세요.

### 라이선스
이 리포지토리는 [CC0 - Public Domain](https://creativecommons.org/publicdomain/zero/1.0/) 라이선스로 제공됩니다.

</details>

## 프로젝트 준비

우선 .NET 서버와 Unity 클라이언트, 그리고 이들 간의 코드를 공유하기 위한 프로젝트를 생성합니다. .NET 서버는 일반적인 .NET 애플리케이션처럼 솔루션(`.sln`)과 프로젝트(`.csproj`)로 생성하고, Unity 클라이언트는 Unity Hub에서 Unity 프로젝트로 생성합니다.

이 가이드에서 생성할 프로젝트의 디렉토리 구조는 [프로젝트 구조](/fundamentals/project-structure)에서 설명된 구조를 따르며, 다음과 같이 구성됩니다.

```plaintext
(Repository Root)
├─ MyApp.sln
└─ src
   ├─ MyApp.Server
   │  ├─ MyApp.Server.csproj
   │  └─ ...
   ├─ MyApp.Shared
   │  ├─ MyApp.Shared.csproj
   │  └─ ...
   └─ MyApp.Unity
      ├─ Assembly-CSharp.csproj
      └─ ...
```

### .NET 서버와 공유 라이브러리 프로젝트 생성

먼저 .NET 서버와 공유 라이브러리 프로젝트를 생성합니다. 여기서는 [.NET 환경에서 클라이언트 및 서버 시작하기](/quickstart)와 같이 ASP.NET Core gRPC 서버와 클래스 라이브러리 프로젝트 및 솔루션을 생성합니다.

다음 명령어를 실행하면 솔루션, 서버 프로젝트, 공유 라이브러리 프로젝트를 생성하고, MagicOnion 관련 패키지와 프로젝트 간의 참조를 한 번에 추가할 수 있습니다.

<Tabs groupId="shell">
  <TabItem value="cmd" label="Windows (cmd.exe)" default>
    ```cmd
    Set MO_PROJECT_NAME=MyApp

    dotnet new gitignore
    dotnet new grpc -o src/%MO_PROJECT_NAME%.Server -n %MO_PROJECT_NAME%.Server
    dotnet new classlib -f netstandard2.1 -o src/%MO_PROJECT_NAME%.Shared -n %MO_PROJECT_NAME%.Shared

    dotnet new sln -n %MO_PROJECT_NAME%
    dotnet sln add src/%MO_PROJECT_NAME%.Server --in-root
    dotnet sln add src/%MO_PROJECT_NAME%.Shared --in-root

    pushd src\%MO_PROJECT_NAME%.Server
    dotnet remove package Grpc.AspNetCore
    dotnet add package MagicOnion.Server
    dotnet add reference ../%MO_PROJECT_NAME%.Shared
    popd

    pushd src\%MO_PROJECT_NAME%.Shared
    dotnet add package MagicOnion.Abstractions
    popd
    ```
  </TabItem>
  <TabItem value="pwsh" label="Windows (PowerShell)">
    ```pwsh
    $MO_PROJECT_NAME="MyApp"

    dotnet new gitignore
    dotnet new grpc -o "src/$MO_PROJECT_NAME.Server" -n "$MO_PROJECT_NAME.Server"
    dotnet new classlib -f netstandard2.1 -o "src/$MO_PROJECT_NAME.Shared" -n "$MO_PROJECT_NAME.Shared"

    dotnet new sln -n "$MO_PROJECT_NAME"
    dotnet sln add "src/$MO_PROJECT_NAME.Server" --in-root
    dotnet sln add "src/$MO_PROJECT_NAME.Shared" --in-root

    pushd "src/$MO_PROJECT_NAME.Server"
    dotnet remove package Grpc.AspNetCore
    dotnet add package MagicOnion.Server
    dotnet add reference "../$MO_PROJECT_NAME.Shared"
    popd

    pushd "src/$MO_PROJECT_NAME.Shared"
    dotnet add package MagicOnion.Abstractions
    popd
    ```
  </TabItem>
  <TabItem value="unix-shell" label="Bash, zsh">
    ```bash
    MO_PROJECT_NAME=MyApp

    dotnet new gitignore
    dotnet new grpc -o src/$MO_PROJECT_NAME.Server -n $MO_PROJECT_NAME.Server
    dotnet new classlib -f netstandard2.1 -o src/$MO_PROJECT_NAME.Shared -n $MO_PROJECT_NAME.Shared

    dotnet new sln -n $MO_PROJECT_NAME
    dotnet sln add src/$MO_PROJECT_NAME.Server --in-root
    dotnet sln add src/$MO_PROJECT_NAME.Shared --in-root

    pushd src/$MO_PROJECT_NAME.Server
    dotnet remove package Grpc.AspNetCore
    dotnet add package MagicOnion.Server
    dotnet add reference ../$MO_PROJECT_NAME.Shared
    popd

    pushd src/$MO_PROJECT_NAME.Shared
    dotnet add package MagicOnion.Abstractions
    popd
    ```
  </TabItem>
</Tabs>

:::info
여기서 보여준 명령어들은 다음 작업들을 수행하는 명령어입니다. .NET에 익숙하다면 Visual Studio나 Rider를 사용하여 이러한 작업들을 수동으로 구성할 수 있습니다.

- ASP.NET Core gRPC 서버 프로젝트 생성 (MyApp.Server)
- 공유 라이브러리용 클래스 라이브러리 프로젝트 생성 (MyApp.Shared)
- 솔루션 파일 생성 (MyApp.sln)
- MyApp.Server와 MyApp.Shared를 솔루션에 추가
- MyApp.Server
    - MagicOnion.Server 패키지 추가
    - MyApp.Shared 프로젝트 참조 추가
- MyApp.Shared
    - MagicOnion.Abstractions 패키지 추가
:::

### Unity 프로젝트 생성
다음으로 `src/MyApp.Unity` 디렉토리에 Unity 프로젝트를 생성합니다. Unity Hub에서 Unity 프로젝트를 생성합니다.

이때 사용할 템플릿은 원하는 것을 선택하시면 됩니다. 예를 들어 "Universal 2D" 또는 "Universal 3D" 등입니다.

![](/img/docs/fig-quickstart-unity-hub.png)

Unity 프로젝트를 생성한 후, 디렉토리 구조가 다음과 같이 되어 있어야 합니다.

```plaintext
(Repository Root)
│  MyApp.sln
└─src
    ├─MyApp.Server
    ├─MyApp.Shared
    └─MyApp.Unity
```

## IDE에서 프로젝트 열기

Visual Studio나 Rider에서 `MyApp.sln`을 열면 `MyApp.Server`와 `MyApp.Shared` 프로젝트를 열 수 있습니다.

:::info{title=".NET 에코시스템에 대해 잘 모르는 개발자를 위한 안내"}
`.sln` 파일은 솔루션이라고 불리며, 여러 프로젝트를 묶는 역할을 합니다. Visual Studio나 Rider 같은 개발 환경에서 솔루션을 열면 서버나 클라이언트, 클래스 라이브러리 등 여러 프로젝트를 한꺼번에 조작하고 관리할 수 있습니다.
:::

## API 서비스 정의하기

MagicOnion에서는 서버가 클라이언트에 제공하는 API 서비스가 모두 .NET 인터페이스로 정의됩니다. 정의된 서비스 인터페이스를 클라이언트와 서버 간에 공유함으로써, 각각 서버의 구현이나 클라이언트에서의 호출에 사용합니다.

서비스 정의가 될 인터페이스를 프로젝트 `MyApp.Shared`에 추가합니다. 이 프로젝트는 서버와 클라이언트 간에 코드를 공유하기 위한 프로젝트입니다.

여기서는 간단한 계산 서비스 `IMyFirstService` 인터페이스를 정의합니다. 인터페이스는 `x`와 `y` 두 개의 `int`를 받아서, 그 합계값을 반환하는 `SumAsync` 메서드를 가지도록 합니다.

```csharp title="src/MyApp.Shared/IMyFirstService.cs"
using MagicOnion;

namespace MyApp.Shared
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<int> SumAsync(int x, int y);
    }
}
```


일반적인 .NET에서의 인터페이스 정의와 거의 같지만, `IService<T>`를 구현하고 있다는 점과 반환값의 타입이 `UnaryResult`가 되어 있다는 점에 주의해 주세요.

`IService<T>`는 이 인터페이스가 Unary 서비스임을 나타내는 인터페이스입니다. Unary 서비스는 하나의 요청에 대해 하나의 응답을 반환하는 API 서비스를 말합니다. 자세한 내용은 [Unary 서비스 기본 사항](/unary/fundamentals) 페이지를 참조하시기 바랍니다.

반환값은 `UnaryResult` 또는 `UnaryResult<T>` 타입이어야 하며, 이는 `Task`나 `ValueTask`처럼 비동기 메서드로 처리됩니다. 여기서 `UnaryResult<int>`는 서버로부터 `int` 값을 받는다는 것을 의미합니다. API가 항상 비동기이기 때문에 `UnaryResult`를 사용해야 하며, 메서드 이름에 `Async` 접미사를 추가하는 것이 권장됩니다.

:::tip
템플릿으로 생성된 프로젝트에 `Class1.cs`가 이미 포함되어 있으므로 프로젝트에서 삭제해 주세요.
:::

## 서버 구현하기

서비스 인터페이스를 정의한 후에는 서버 프로젝트 `MyApp.Server`에서 서비스를 구현해야 합니다.

### 서버 초기 구성

먼저 템플릿 생성 시 기본으로 추가된 gRPC 샘플 구현을 삭제합니다. 샘플 구현은 `Protos` 폴더와 `Services` 폴더에 포함되어 있으므로 해당 폴더들을 삭제합니다.

다음으로 서버의 시작 구성을 설정합니다. ASP.NET Core 애플리케이션은 `Program.cs`에서 서버의 기능에 관한 구성을 수행합니다.

`Program.cs`에서 `builder.Services.AddMagicOnion()`과 `app.MapMagicOnionService()`를 호출하여 MagicOnion 서버 기능을 활성화합니다. 생성 시 템플릿으로 작성된 `using MyApp.Server.Services`와 `builder.Services.AddGrpc();`, `app.MapGrpcService<GreeterService>();`를 제거해 주세요.

이러한 설정들이 적용된 `Program.cs`는 다음과 같습니다.

```csharp title="src/MyApp.Server/Program.cs"
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// highlight-start
builder.Services.AddMagicOnion();
// highlight-end

var app = builder.Build();

// Configure the HTTP request pipeline.
// highlight-start
app.MapMagicOnionService();
// highlight-end
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
```

:::note
MyApp.Server.csproj에 `Protobuf` 항목이 남아있을 수 있습니다. 해당 항목이 있다면 필요하지 않으므로 삭제해 주세요.

```xml
  <ItemGroup>
    <Protobuf Include="Protos\greet.proto" GrpcServices="Server" />
  </ItemGroup>
```
:::

### API 서비스 구현

다음으로 클라이언트로부터의 요청을 받아 처리하는 API 서비스를 구현합니다.

이 가이드에서는 서비스 정의로 `IMyFirstService` 인터페이스를 정의했으므로, API 서비스(Unary 서비스) 구현 클래스는 이 인터페이스를 구현해야 합니다. 또한 구현 클래스는 `ServiceBase<T>` 기본 클래스를 상속해야 합니다.

이를 바탕으로 `Services` 폴더에 `MyFirstService` 클래스를 생성합니다. 다음은 클래스 구현의 예시입니다.

```csharp title="src/MyApp.Server/Services/MyFirstService.cs"
using MagicOnion;
using MagicOnion.Server;
using MyApp.Shared;

namespace MyApp.Server.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    // `UnaryResult<T>` allows the method to be treated as `async` method.
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        Console.WriteLine($"Received:{x}, {y}");
        return x + y;
    }
}
```

여기서는 `Services` 폴더를 생성하여 추가했지만 폴더 구조에 대한 제약은 없습니다.

:::info
`UnaryResult`는 `Task`나 `ValueTask` 등과 마찬가지로 비동기 메서드(`async`)로 취급할 수 있습니다.
:::

### 서버 시작 확인하기
서버 구현이 완료되었으므로, 서버를 시작하고 작동하는지 확인해 보겠습니다. 서버는 Visual Studio나 Rider 등의 IDE에서 디버그 실행하거나, 터미널에서 `dotnet run` 명령어를 실행하여 시작합니다.

빌드 에러가 없고 서버가 성공적으로 시작되면 다음과 같은 로그가 출력되고 서버가 시작됩니다. 이때 표시되는 `http://...`는 나중에 Unity 클라이언트에서 연결할 때 사용할 URL이므로 메모해 두는 것을 추천합니다.

```plaintext
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7071
info: Microsoft.Hosting.Lifetime[14]
# highlight-start
      Now listening on: http://localhost:5210
# highlight-end
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: D:\repo\src\MyApp.Server
```

이 시점에서 디렉토리와 파일 구조는 다음과 같아야 합니다. 이로써 서버 관련 작업이 완료되었습니다.

![](/img/docs/fig-quickstart-unity-server-sln.png)
```plaintext
(Repository Root)
├─ MyApp.sln
└─ src
   ├─ MyApp.Server
   │  ├─ MyApp.Server.csproj
   │  ├─ Program.cs
   │  └─ Services
   │     └─ MyFirstService.cs
   └─ MyApp.Shared
      ├─ MyApp.Shared.csproj
      └─ IMyFirstService.cs
```

## Unity 클라이언트 구현하기

다음으로 서버를 호출하는 Unity 클라이언트를 구현합니다. Unity 프로젝트 `MyApp.Unity`를 Unity Editor에서 엽니다.

### MagicOnion과 관련 패키지 설치
먼저 Unity 프로젝트에서 MagicOnion을 사용하기 위해, 다음 세 가지 패키지를 설치해야 합니다.

- NuGetForUnity
- MagicOnion.Client + Unity extension
- YetAnotherHttpHandler

#### NuGetForUnity 설치하기
MagicOnion은 NuGet 패키지로 제공되므로, Unity에서 NuGet 패키지를 설치하기 위한 확장 프로그램인 [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)를 설치해야 합니다.

NuGetForUnity를 설치하려면 Package Manager의 `Add package from git URL...`에 아래의 URL을 지정합니다. 자세한 내용은 NuGetForUnity의 README를 참조해 주세요.
```
https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
```

![](/img/docs/fig-quickstart-unity-nugetforunity.png)

#### MagicOnion.Client + Unity용 확장 설치
MagicOnion의 클라이언트 라이브러리인 MagicOnion.Client는 NuGet 패키지로 제공되므로, NuGetForUnity를 사용하여 설치합니다.

Unity Editor의 메뉴에서 `NuGet` → `Manage NuGet Packages`를 선택하고, NuGetForUnity 창에서 `MagicOnion.Client`를 검색하여 설치합니다.

![](/img/docs/fig-quickstart-unity-nuget-magiconionclient.png)

다음으로 Unity용 확장이 포함된 Unity 패키지를 설치합니다. Unity Package Manager의 `Add package from git URL...`에 아래의 URL을 지정해 주세요.
```
https://github.com/Cysharp/MagicOnion.git?path=src/MagicOnion.Client.Unity/Assets/Scripts/MagicOnion.Client.Unity#7.0.2
```

![](/img/docs/fig-quickstart-unity-upm-magiconion.png)

:::info
`7.0.2`는 설치 버전이며, 릴리스된 버전에 따라 더 최신 버전을 사용할 수 있습니다.
:::

#### YetAnotherHttpHandler 설치하기

마지막으로 Unity에서 gRPC(HTTP/2) 통신을 위한 통신 라이브러리인 [YetAnotherHttpHandler](https://github.com/Cysharp/YetAnotherHttpHandler)를 설치합니다. YetAnotherHttpHandler 설치는 필요한 NuGet 패키지와 YetAnotherHttpHandler 패키지, 이렇게 두 가지 설치 단계가 필요합니다.

먼저 작동에 필요한 NuGet 패키지를 설치합니다. NuGetForUnity에서 `System.IO.Pipelines`를 검색하여 설치합니다.

![](/img/docs/fig-quickstart-unity-nuget-systemiopipelines.png)

다음으로 Unity Package Manager를 통해 YetAnotherHttpHandler를 설치합니다. Unity Package Manager의 `Add package from git URL...`에 아래의 URL을 지정해 주세요.

```
https://github.com/Cysharp/YetAnotherHttpHandler.git?path=src/YetAnotherHttpHandler#1.10.0
```

:::info
`1.10.0`는 설치 버전이며, 릴리스된 버전에 따라 더 최신 버전을 사용할 수 있습니다.
:::

![](/img/docs/fig-quickstart-unity-yaha.png)

더 자세한 설치 절차는 [YetAnotherHttpHandler의 README](https://github.com/Cysharp/YetAnotherHttpHandler)를 참조해 주세요.

위의 설치를 완료하면 Unity 프로젝트의 패키지(NuGet 및 UPM) 목록은 다음과 같이 표시됩니다.

![](/img/docs/fig-quickstart-unity-installedpackages.png)

### Unity 프로젝트에서 공유된 서비스 정의를 참조할 수 있도록 설정하기

Unity 클라이언트에서 API를 호출하려면 서비스 정의를 참조해야 합니다. 서비스 정의는 `MyApp.Shared` 프로젝트에 정의되어 있지만, 이는 .NET 프로젝트이므로 Unity 프로젝트에서 직접 참조할 수 없습니다.

.NET 서버 측과 Unity 프로젝트 간의 서비스 정의 관련 소스 코드를 공유하기 위한 두 가지 방법을 설명합니다.

#### 방법1: 파일을 복사하여 공유하기
첫 번째 방법은 소스 코드 파일을 단순히 복사하여 공유하는 것입니다. 이는 `MyApp.Shared`의 `IMyFirstService.cs` 파일을 Unity 프로젝트로 복사하는 것입니다.

이 방법의 장점은 파일 복사만으로 충분하기 때문에 시도하기가 매우 쉽다는 것입니다. 단점은 파일 복사가 수동으로 이루어지므로 파일이 변경될 때마다 수동으로 복사해야 한다는 것입니다.

#### 방법2: 로컬 패키지로 공유하기
두 번째 방법은 `MyApp.Shared` 프로젝트를 Unity 패키지로 취급하는 것입니다. 이 방법은 파일을 복사하지 않고 동일한 파일을 공유할 수 있으며, 파일이 변경될 때 자동으로 업데이트되므로 권장됩니다. 이 방법은 MagicOnion 개발에서 권장되는 방법입니다.

.NET 클래스 라이브러리 프로젝트를 Unity 로컬 패키지로 취급하려면 몇 가지 추가 단계가 필요합니다.

먼저 `MyApp.Shared` 프로젝트에 `package.json`을 추가합니다. 이 파일은 Unity가 패키지로 인식하는 데 필요한 파일입니다. `MyApp.Shared` 디렉토리 아래에 다음 JSON 내용으로 `package.json` 파일을 생성합니다.

```json title="src/MyApp.Shared/package.json"
{
  "name": "com.cysharp.magiconion.samples.myapp.shared.unity",
  "version": "1.0.0",
  "displayName": "MyApp.Shared.Unity",
  "description": "MyApp.Shared.Unity"
}
```

다음으로, Unity에서 별도의 어셈블리로 만들기 위해 Assembly Definition 파일을 추가합니다. `MyApp.Shared`에 `MyApp.Shared.Unity.asmdef`라는 이름으로 다음 내용의 Assembly Definition 파일을 생성합니다.

```json title="src/MyApp.Shared/MyApp.Shared.Unity.asmdef"
{
    "name": "MyApp.Shared.Unity"
}
```

:::note
Assembly Definition 파일의 이름은 Unity에서 IDE를 열 때 프로젝트 이름으로 사용됩니다. 그러므로 서버용 `MyApp.Shared`와 구분하기 위해 `.Unity` 접미사를 붙이는 것을 권장합니다.
:::

마지막으로 `MyApp.Shared` 프로젝트에 `Directory.Build.props`와 `Directory.Build.targets`를 추가합니다. 이 파일들은 .NET 프로젝트에서 `bin`과 `obj` 폴더를 출력하지 않도록 구성하고(대신 `.artifacts`에 출력), Unity 관련 파일들(.meta 등)을 IDE에서 숨기도록 설정하는 데 사용됩니다.

```xml title="src/MyApp.Shared/Directory.Build.props"
<Project>
  <PropertyGroup>
    <!-- https://learn.microsoft.com/en-us/dotnet/core/sdk/artifacts-output -->
    <ArtifactsPath>$(MSBuildThisFileDirectory).artifacts</ArtifactsPath>
  </PropertyGroup>
</Project>
```

```xml title="src/MyApp.Shared/Directory.Build.targets"
<Project>
  <!-- Hide Unity-specific files from Visual Studio and .NET SDK -->
  <ItemGroup>
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
</Project>
```

:::warning
이러한 파일들을 추가한 후에는 프로젝트를 빌드하고, `bin`과 `obj` 폴더가 남아있다면 반드시 삭제하십시오. 이러한 파일들이 남아있으면 Unity에서 인식되어 문제가 발생할 수 있습니다.
:::

:::warning
macOS Finder에서는 기본적으로 `.`으로 시작하는 파일들이 숨겨져 있습니다. `.artifacts` 폴더가 보이지 않는 경우, Finder에서 `Command + Shift + .`를 누르거나 `defaults` 명령어를 사용하여 설정을 변경해야 합니다.
:::

이러한 단계들을 거치면 `MyApp.Shared` 프로젝트는 다음과 같은 파일 구조를 갖게 됩니다.

![](/img/docs/fig-quickstart-unity-packagize.png)

```plaintext
(Repository Root)
└─ src
   └─ MyApp.Shared
      ├─ Directory.Build.props
      ├─ Directory.Build.targets
      ├─ IMyFirstService.cs
      ├─ MyApp.Shared.Unity.asmdef
      ├─ MyApp.Shared.csproj
      └─ package.json
```

### 공유 프로젝트의 로컬 패키지 참조하기

`MyApp.Shared`가 로컬 패키지로 사용 가능하게 되었으므로, Unity 프로젝트에서 이 로컬 패키지에 대한 참조를 추가해야 합니다.

로컬 패키지 참조를 추가하기 위해 `MyApp.Unity/Packages/manifest.json`에 패키지 경로를 추가합니다. 여기서는 패키지에 대한 상대 경로로 `file:../../MyApp.Shared`를 추가합니다.

```json title="src/MyApp.Unity/Packages/manifest.json"
{
  "dependencies": {
    "com.cysharp.magiconion.client.unity": "https://github.com/Cysharp/MagicOnion.git?path=src/MagicOnion.Client.Unity/Assets/Scripts/MagicOnion.Client.Unity#7.0.1",
    // highlight-start
    "com.cysharp.magiconion.samples.myapp.shared.unity": "file:../../MyApp.Shared",
    // highlight-end
    "com.cysharp.yetanotherhttphandler": "https://github.com/Cysharp/YetAnotherHttpHandler.git?path=src/YetAnotherHttpHandler#1.10.0",
    "com.github-glitchenzo.nugetforunity": "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity",
    "com.unity.collab-proxy": "2.6.0",
    "com.unity.feature.2d": "2.0.1",
    ...
}
```

:::note
Unity Editor의 Package Manager에서 `Install package from disk...`를 사용하여 설치하면 `manifest.json`에 절대 경로로 저장되므로 주의해 주세요.
:::

![](/img/docs/fig-quickstart-unity-localpackage.png)

### MagicOnion 클라이언트 구성하기
Unity 프로젝트에서 MagicOnion을 사용하려면 gRPC 연결을 관리하는 프로바이더 호스트라는 시스템을 초기화해야 합니다.

`MyApp.Unity` 프로젝트에 다음 소스 코드를 포함하는 `MagicOnionInitializer` 클래스를 추가하세요.

```csharp title="src/MyApp.Unity/Assets/Scripts/MagicOnionInitializer.cs"
using Cysharp.Net.Http;
using Grpc.Net.Client;
using MagicOnion.Unity;
using UnityEngine;

public class MagicOnionInitializer
{
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
}
```

Unity 확장에 대한 자세한 내용은 [Unity 확장](/integration/unity) 페이지를 참조하십시오.

### 서버에 연결하여 API 호출하기

이제 MagicOnion을 사용하기 위한 준비가 완료되었으므로, Unity 클라이언트에서 서버에 연결하고 API를 호출하는 코드를 구현하겠습니다. 설명을 단순화하기 위해, `MonoBehaviour`의 `Start` 메서드에서 서버에 연결하고 API를 호출한 후 결과를 디버그 로그로 출력하는 방식으로 구현하겠습니다.

먼저 서버 연결에 필요한 gRPC 채널을 생성합니다. MagicOnion에서는 `GrpcChannelx` 클래스를 사용하여 채널을 생성할 수 있습니다. 여기서는 `GrpcChannelx.ForAddress` 메서드를 사용하여 지정된 주소로 연결하는 채널을 만듭니다. 사용할 주소는 서버 구현 섹션에서 서버를 시작할 때 표시된 `http://...`입니다.

```csharp
var channel = GrpcChannelx.ForAddress("http://localhost:5210");
```

생성된 채널을 사용하여 MagicOnion 클라이언트를 생성합니다. 이는 `MagicOnionClient` 클래스의 `Create` 메서드를 통해 수행됩니다. `Create` 메서드에 생성된 채널과 API 서비스 인터페이스를 지정합니다. 이렇게 하면 `IMyFirstService` 인터페이스를 구현하는 클라이언트가 생성됩니다.

```csharp
var client = MagicOnionClient.Create<IMyFirstService>(channel);
```

마지막으로 생성된 클라이언트를 사용하여 메서드를 호출합니다. 이는 일반적인 인터페이스 메서드를 호출하는 것과 동일한 방식으로 수행됩니다. 메서드가 비동기이므로 호출 결과인 `UnaryResult`를 `await`를 사용하여 기다려야 합니다.

```csharp
var result = await client.SumAsync(100, 200);
```

위의 단계들을 종합하여 MonoBehaviour에 구현하면 다음과 같습니다. 여기서는 SampleScene에서 사용할 MonoBehaviour로 `SampleScene` 클래스를 생성합니다.

```csharp title="src/MyApp.Unity/Assets/Scripts/SampleScene.cs"
using System;
using MagicOnion;
using MagicOnion.Client;
using MyApp.Shared;
using UnityEngine;

public class SampleScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        try
        {
            // highlight-start
            var channel = GrpcChannelx.ForAddress("http://localhost:5210");
            var client = MagicOnionClient.Create<IMyFirstService>(channel);

            var result = await client.SumAsync(100, 200);
            Debug.Log($"100 + 200 = {result}");
            // highlight-end
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
```

:::tip
`async void` 사용은 `Start`와 같은 이벤트를 제외하고는 피하는 것이 좋습니다. UniTask를 사용하고 있다면 `async void` 대신 `async UniTaskVoid`를 사용하는 것을 권장합니다.

:::

마지막으로 씬에 GameObject를 추가하고, 해당 GameObject에 `SampleScene` 스크립트를 연결합니다. 이로써 Unity 클라이언트에서 서버에 연결하고 API를 호출할 준비가 완료되었습니다.

Unity Editor에서 Play 모드를 시작하면 `Start` 메서드가 호출되어 서버 연결 및 API 호출 프로세스가 실행됩니다. Console 로그에 `100 + 200 = 300`이 표시되면 서버에 성공적으로 연결되어 API를 호출한 것입니다.

![](/img/docs/fig-quickstart-unity-unarydebuglog.png)


#### Troubleshooting
- `IOException: client error (Connect): tcp connect error: No connection could be made because the target machine actively refused it. (os error 10061)`
  - 서버에 연결할 수 없는 상태입니다. 서버가 실행 중인지, 포트 번호가 올바른지 확인해 주세요.
- `IOException: client error (Connect): invalid peer certificate: UnknownIssuer`
  - `https://`로 연결을 시도할 때 발생하는 오류입니다. 개발 인증서를 인식할 수 없어서 발생합니다. `http://...`를 사용하여 연결하세요(포트 번호를 주의해서 확인하세요).

## Unity와 .NET 서버 솔루션 통합하기

Unity Editor에서 Visual Studio나 Rider와 같은 개발 환경에서 C# 코드나 프로젝트를 열면, Unity Editor가 생성한 솔루션이 열립니다(예: `MyApp.Unity.sln`).

하지만 Unity Editor가 생성한 솔루션에는 .NET 서버 프로젝트가 포함되어 있지 않으므로, 서버 개발과 디버깅을 위해서는 별도의 솔루션(예: `MyApp.sln`)을 열어야 합니다.

[SlnMerge](https://github.com/Cysharp/SlnMerge) 에디터 확장을 사용하면 Unity와 .NET 서버 솔루션을 통합하여 원활한 개발이 가능해집니다.

![](/img/docs/fig-quickstart-unity-slnmerge.png)

### SlnMerge 설치하기

SlnMerge를 설치하려면 Package Manager의 `Add package from git URL...`에 다음 URL을 지정합니다.

```plaintext
https://github.com/Cysharp/SlnMerge.git?path=src
```

![](/img/docs/fig-quickstart-unity-upm-slnmerge.png)

### SlnMerge 설정하기

SlnMerge를 설치한 후, 솔루션 통합을 위한 SlnMerge 설정을 생성해야 합니다.

Unity Editor가 생성한 솔루션 파일 이름에 `.mergesettings`를 붙인 설정 파일을 생성해야 합니다.

예를 들어 `MyApp.Unity` 프로젝트가 있는 경우 `MyApp.Unity.sln`이 생성되므로, `MyApp.Unity.sln.mergesettings`라는 이름의 설정 파일을 생성합니다.

```xml title="src/MyApp.Unity/MyApp.Unity.sln.mergesettings"
<SlnMergeSettings>
    <MergeTargetSolution>..\..\MyApp.sln</MergeTargetSolution>
</SlnMergeSettings>
```

### 솔루션 열기

솔루션을 열려면 Unity Editor에서 C# 파일을 더블클릭하거나 메뉴에서 `Assets` → `Open C# Project`를 선택하세요.

## 다음 단계

이 가이드에서는 MagicOnion을 사용하여 API 서비스를 구축하고 통신하는 방법을 설명했습니다. 다음 단계는 다음 문서들을 참조하세요:

- [Unity 환경에서 사용하기](/installation/unity)
    - `Vector3`와 기타 Unity 특화 타입을 사용하고 싶은 경우 이 문서를 참조하세요.
- [StreamingHub 서비스 기본 사항](/streaminghub/fundamentals)
    - 서버와 클라이언트 간 실시간 통신을 위한 StreamingHub의 기본 사용법
- [AOT 지원 (IL2CPP, Native AOT)](/fundamentals/aot)
    - iOS나 Android, Windows용 AOT 빌드 시 주의사항과 대응이 필요한 부분에 대해
- [Unity 확장](/integration/unity)
    - MagicOnion용 Unity 에디터 확장 관련 정보
- [프로젝트 구성](/fundamentals/project-structure)
    - MagicOnion 프로젝트의 권장 프로젝트 구조
