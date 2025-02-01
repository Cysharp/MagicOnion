# クイックスタート
このガイドではシンプルな MagicOnion サーバーとクライアントを作成する方法を示します。サーバーは、2 つの数値を加算するシンプルなサービスを提供し、クライアントはサービスを呼び出して結果を取得します。

MagicOnion は Web API のような RPC サービスとリアルタイム通信の StreamingHub を提供します。このセクションでは Web API のような RPC サービスを実装します。

## サーバーサイド: サービスの定義と実装

初めに、MagicOnion サーバープロジェクトを作成し、サービスインターフェイスを定義して実装します。

### 1. MagicOnion 用の gRPC サーバープロジェクトのセットアップ

Minimal API プロジェクトから始める（詳細: [ASP.NET Core で最小限の Web API を作成するチュートリアル](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api)）ため、**ASP.NET Core Empty** テンプレートからプロジェクトを作成します。プロジェクトには NuGet パッケージ `MagicOnion.Server` を追加します。.NET CLI ツールを使用して追加する場合は、次のコマンドを実行します。

```bash
dotnet add package MagicOnion.Server
```

`Program.cs` を開いて、Services と App にいくつかの設定を追加します。

```csharp
using MagicOnion;
using MagicOnion.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMagicOnion(); // Add this line(MagicOnion.Server)

var app = builder.Build();

app.MapMagicOnionService(); // Add this line

app.Run();
```

以上で、サーバープロジェクトで MagicOnion を使用する準備が整いました。

### 2. サービスの実装

`IMyFirstService` インターフェースを追加して、サーバーとクライアント間で共有します。ここでは共有するインターフェースを含む名前空間を MyApp.Shared としています。

戻り値は `UnaryResult` または `UnaryResult<T>` 型である必要があり、これは Task や ValueTask と同様に非同期メソッドとして扱われます。

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

`IMyFirstService` インターフェースを実装するクラスを追加します。クライアントからの呼び出しはこのクラスで処理します。

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

これでサービスの定義と実装が行えました。

MagicOnion サーバーを開始する準備が整いました。F5 キーを押すか `dotnet run` コマンドを使用して MagicOnion サーバーを開始できます。この際にサーバーが起動したときに表示される URL はクライアントから接続する先となるためメモしておいてください。

## クライアントサイド: サービスの呼び出し

**コンソール アプリケーション** プロジェクトを作成し、NuGet パッケージ `MagicOnion.Client` を追加します。

`IMyFirstService` インターフェースを共有し、クライアントで使用します。ファイルリンク、共有ライブラリ、またはコピー＆ペーストなど、様々な方法でインターフェースを共有できます。

クライアントコードでは `MagicOnionClient` で共有インターフェースを元にクライアントプロキシをに作成し、サービスを透過的に呼び出します。

初めに gRPC のチャンネルを作成します。gRPC チャンネルは接続を抽象化したものであり先ほどメモした URL を `GrpcChannel.ForAddress` メソッドに渡して作成します。作成されたチャンネルを使用して MagicOnion のクライアントプロキシーを作成します。


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
MagicOnion クライアントを Unity アプリケーションで使用する場合は、[Unity での利用](installation/unity)も参照してください。
:::
