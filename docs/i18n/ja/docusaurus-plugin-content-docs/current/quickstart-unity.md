import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# Unity と .NET サーバーで始める

このガイドでは MagicOnion を使用したシンプルな Unity アプリケーションによるクライアントと .NET サーバーを作成する手順を解説します。ここで解説する構成は .NET サーバーに実装された2つの数値を加算する API サービスを Unity クライアントから呼び出すごく単純なものです。

ここではサーバーとクライアントを作成するために下記の環境を使用しています。

- .NET 8 SDK またはそれ以降
- Unity 6000.0.36f (Standalone Player)

:::note
Unity 6 の一部バージョンでは Source Generator に関する不具合があるため 6000.0.34f1 以降を使用してください
:::

## プロジェクトの準備

まず初めに .NET サーバーと Unity クライアント、そしてそれらでコードを共有するためのプロジェクトを作成します。.NET サーバーは一般的な .NET アプリケーションと同様にソリューション (`.sln`) とプロジェクト (`.csproj`) を作成し、Unity クライアントは Unity Hub から Unity プロジェクトとして作成します。

このガイドで作成するプロジェクトのディレクトリ構成は [プロジェクト構成](/fundamentals/project-structure) で解説する構成に従い、最終的に下記の通りとなります。

```plaintext
(Repository Root)
├─ MyApp.Server.sln
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

### .NET サーバーと共有ライブラリープロジェクトの作成

初めに .NET のサーバーと共有ライブラリープロジェクトを作成します。ここでは [.NET クライアントとサーバーで始めるクイックスタート](/quickstart) と同様に ASP.NET Core gRPC サーバーとクラスライブラリーのプロジェクトおよびソリューションを作成します。

下記のコマンドを実行することでソリューション、サーバープロジェクト、共有ライブラリープロジェクトの作成とMagicOnion 関連のパッケージの参照、プロジェクト間の参照設定を一度に行えます。

<Tabs>
  <TabItem value="cmd" label="Windows (cmd.exe)" default>
    ```cmd
    Set MO_PROJECT_NAME=MyApp

    dotnet new gitignore
    dotnet new grpc -o src/%MO_PROJECT_NAME%.Server -n %MO_PROJECT_NAME%.Server
    dotnet new classlib -f netstandard2.1 -o src/%MO_PROJECT_NAME%.Shared -n %MO_PROJECT_NAME%.Shared

    dotnet new sln -n %MO_PROJECT_NAME%.Server
    dotnet sln add src/%MO_PROJECT_NAME%.Server
    dotnet sln add src/%MO_PROJECT_NAME%.Shared

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

    dotnet new sln -n "$MO_PROJECT_NAME.Server"
    dotnet sln add "src/$MO_PROJECT_NAME.Server"
    dotnet sln add "src/$MO_PROJECT_NAME.Shared"

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

    dotnet new sln -n $MO_PROJECT_NAME.Server
    dotnet sln add src/$MO_PROJECT_NAME.Server
    dotnet sln add src/$MO_PROJECT_NAME.Shared

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
ここで示した一連のコマンドは下記の操作をまとめたものです。.NET に詳しい場合にはこれらの操作を Visual Studio や Rider などを使用して手動で構成できます。

- ASP.NET Core gRPC サーバープロジェクトの作成 (MyApp.Server)
- 共有ライブラリー用クラスライブラリープロジェクトの作成 (MyApp.Shared)
- ソリューションファイルの作成 (MyApp.Server.sln)
- MyApp.Server と MyApp.Shared をソリューションに追加
- MyApp.Server
  - MagicOnion.Server のパッケージの追加
  - MyApp.Shared プロジェクトへの参照追加
- MyApp.Shared
  - MagicOnion.Abstractions パッケージの追加
:::

### Unity プロジェクトの作成
次に `src/MyApp.Unity` ディレクトリに Unity プロジェクトを作成します。Unity Hub から Unity プロジェクトを作成します。

この際に使用するテンプレートはお好みのものを選択してください。例えば "Universal 2D" や "Universal 3D" などです。

![](/img/docs/fig-quickstart-unity-hub.png)

この時点でディレクトリは下記の通りとなっていることを確認してください。

```plaintext
(Repository Root)
│  MyApp.Server.sln
└─src
    ├─MyApp.Server
    ├─MyApp.Shared
    └─MyApp.Unity
```

## API サービスを定義する

MagicOnion ではサーバーがクライアントに提供する API サービスはすべて .NET のインターフェースとして定義されます。定義したサービスのインターフェースをクライアントとサーバー間で共有することで、それぞれサーバーの実装やクライアントからの呼び出しに利用します。

ここでは単純な計算サービス `IMyFirstService` インターフェースを定義します。インターフェースは `x` と `y` の二つの `int` を受け取り、その合計値を返す `SumAsync` メソッドを持つものとします (API は常に非同期メソッド)。

このインターフェースをサーバーとクライアントで共有するコードを含めるためのプロジェクト `MyApp.Shared` に追加します。

```csharp title="src/MyApp.Shared/IMyFirstService.cs"
using System;
using MagicOnion;

public interface IMyFirstService : IService<IMyFirstService>
{
    UnaryResult<int> SumAsync(int x, int y);
}
```


一般的な .NET でのインターフェース定義とほとんど同じですが、`IService<T>` を実装している点と戻り値の型が `UnaryResult` となっている点に注意してください。

`IService<T>` はこのインターフェースが Unary サービスであることを表すインターフェースです。Unary サービスとは 1 つのリクエストに対して 1 つのレスポンスを返す API サービスのことです。詳しくは [Unary の基礎](/unary/fundamentals) を参照してください。

戻り値は `UnaryResult` または `UnaryResult<T>` 型である必要があります。これは Task や ValueTask と同様に非同期メソッドとして扱われる MagicOnion 固有の特殊な型です。ここでの `UnaryResult<int>` は `int` 型の値を返すことを表します。

:::tip
このプロジェクトにはテンプレートで作成された `Class1.cs` があらかじめ含まれているので削除してください。
:::

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

```xml title="src/MyApp.Shared/MyApp.Shared.csproj"

```

```json title="src/MyApp.Shared/package.json"
{
  "name": "com.cysharp.magiconion.samples.myapp.shared.unity",
  "version": "1.0.0",
  "displayName": "MyApp.Shared.Unity",
  "description": "MyApp.Shared.Unity",
}
```
```json title="src/MyApp.Shared/MyApp.Shared.Unity.asmdef"
{
    "name": "MyApp.Shared.Unity",
    "references": [
        "MessagePack.Annotations",
        "MagicOnion.Abstractions"
    ],
    "optionalUnityReferences": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": []
}
```
