# .NET 환경에서 클라이언트 및 서버 시작하기
이 가이드에서는 간단한 MagicOnion 서버와 클라이언트를 만드는 방법을 소개합니다. 서버는 두 개의 숫자를 더하는 간단한 서비스를 제공하고, 클라이언트는 서비스를 호출하여 결과를 얻습니다.

MagicOnion은 Web API와 같은 RPC 서비스와 실시간 통신을 위한 StreamingHub를 제공합니다. 이 섹션에서는 Web API와 같은 RPC 서비스를 이용해 구현합니다.

## 서버 측: 서비스 정의 및 구현

먼저 MagicOnion 서버 프로젝트를 생성하고 서비스 인터페이스를 정의하고 구현합니다.

### 1. MagicOnion용 gRPC 서버 프로젝트 설정하기


Minimal API 프로젝트부터 시작하기 위해 (상세: [ASP.NET Core를 사용하여 최소 API 만들기](https://learn.microsoft.com/ko-kr/aspnet/core/tutorials/min-web-api)), **ASP.NET Core Empty** 템플릿에서 프로젝트를 생성합니다. 프로젝트에 NuGet 패키지 `MagicOnion.Server`를 추가합니다. .NET CLI 도구를 사용하여 추가하는 경우 다음 명령을 실행합니다.

```bash
dotnet add package MagicOnion.Server
```

`Program.cs`를 열고 Services와 App에 몇 가지 설정을 추가합니다.

```csharp
using MagicOnion;
using MagicOnion.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMagicOnion(); // Add this line(MagicOnion.Server)

var app = builder.Build();

app.MapMagicOnionService(); // Add this line

app.Run();
```

이제 서버 프로젝트에서 MagicOnion을 사용할 준비가 되었습니다.

### 2. 서비스 구현

`IMyFirstService` 인터페이스를 추가하여 서버와 클라이언트 간에 공유합니다. 여기서는 공유할 인터페이스를 포함하는 네임스페이스를 MyApp.Shared로 지정합니다.

반환값은 `UnaryResult` 또는 `UnaryResult<T>` 타입이어야 하며, `Task`나 `ValueTask`와 마찬가지로 비동기 메서드로 취급됩니다.

```csharp
using System;
using MagicOnion;

namespace MyApp.Shared
{
    // Defines .NET interface as a Server/Client IDL.
    // The interface is shared between server and client.
    public interface IMyFirstService : IService<IMyFirstService>
    {
        // The return type must be `UnaryResult<T>` or `UnaryResult`.
        UnaryResult<int> SumAsync(int x, int y);
    }
}
```

`IMyFirstService` 인터페이스를 구현하는 클래스를 추가합니다. 클라이언트로부터의 호출은 이 클래스에서 처리한다.

```csharp
using MagicOnion;
using MagicOnion.Server;
using MyApp.Shared;

namespace MyApp.Services;

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

이제 서비스를 정의하고 구현할 수 있게 되었습니다.

MagicOnion 서버를 시작할 준비가 완료되었습니다. F5 키를 누르거나 `dotnet run` 명령을 사용하여 MagicOnion 서버를 시작할 수 있습니다. 이때 서버가 시작될 때 표시되는 URL은 클라이언트에서 연결할 주소가 되므로 메모해 두시기 바랍니다.

## 클라이언트 측: 서비스 호출

**콘솔 애플리케이션** 프로젝트를 생성하고 NuGet 패키지 `MagicOnion.Client`를 추가합니다.

`IMyFirstService` 인터페이스를 공유하여 클라이언트에서 사용합니다. 파일 링크, 공유 라이브러리 또는 복사 및 붙여넣기 등 다양한 방법으로 인터페이스를 공유할 수 있습니다.

클라이언트 코드에서는 `MagicOnionClient`에서 공유 인터페이스를 기반으로 클라이언트 프록시를 생성하여 서비스를 투명하게 호출합니다.

먼저 gRPC 채널을 생성합니다. gRPC 채널은 연결을 추상화한 것으로, 앞서 메모한 URL을 `GrpcChannel.ForAddress` 메서드에 전달하여 생성합니다. 생성된 채널을 사용하여 MagicOnion의 클라이언트 프록시를 생성합니다.


```csharp
using Grpc.Net.Client;
using MagicOnion.Client;
using MyApp.Shared;

// Connect to the server using gRPC channel.
var channel = GrpcChannel.ForAddress("https://localhost:5001");

// Create a proxy to call the server transparently.
var client = MagicOnionClient.Create<IMyFirstService>(channel);

// Call the server-side method using the proxy.
var result = await client.SumAsync(123, 456);
Console.WriteLine($"Result: {result}");
```

:::tip
MagicOnion 클라이언트를 Unity 환경에서 사용하려면 [Unity 환경에서 사용하기](installation/unity)를 참고하세요.
:::
