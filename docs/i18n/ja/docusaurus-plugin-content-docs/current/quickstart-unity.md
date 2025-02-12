import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# Unity と .NET サーバーで始める

このガイドでは MagicOnion を使用したシンプルな Unity アプリケーションによるクライアントと .NET サーバーを作成する手順を解説します。ここでは .NET サーバーに実装された2つの数値を加算する API サービスを Unity クライアントから呼び出す、単純な実装を行います。

このガイドではサーバーとクライアントを作成するために下記の環境を想定しています。

- Windows または macOS
- .NET 8 SDK またはそれ以降
- Unity 2022.3 以降または Unity 6 (6000.0.34f1) 以降

:::note
Unity 6 の一部バージョンでは Source Generator に関する不具合があるため 6000.0.34f1 以降を使用してください
:::

## プロジェクトの準備

まず初めに .NET サーバーと Unity クライアント、そしてそれらでコードを共有するためのプロジェクトを作成します。.NET サーバーは一般的な .NET アプリケーションと同様にソリューション (`.sln`) とプロジェクト (`.csproj`) を作成し、Unity クライアントは Unity Hub から Unity プロジェクトとして作成します。

このガイドで作成するプロジェクトのディレクトリ構成は [プロジェクト構成](/fundamentals/project-structure) で解説する構成に従い、最終的に下記の通りとなります。

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
ここで示した一連のコマンドは下記の操作をまとめたものです。.NET に詳しい場合にはこれらの操作を Visual Studio や Rider などを使用して手動で構成できます。

- ASP.NET Core gRPC サーバープロジェクトの作成 (MyApp.Server)
- 共有ライブラリー用クラスライブラリープロジェクトの作成 (MyApp.Shared)
- ソリューションファイルの作成 (MyApp.sln)
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
│  MyApp.sln
└─src
    ├─MyApp.Server
    ├─MyApp.Shared
    └─MyApp.Unity
```

## IDE でプロジェクトを開く

Visual Studio や Rider で MyApp.sln を開くことで、MyApp.Server と MyApp.Shared プロジェクトを開けます。

:::info{title=".NET エコシステムにあまり詳しくない開発者の方向け"}
`.sln` ファイルはソリューションと呼ばれ、複数のプロジェクトを束ねる役割を担います。Visual Studio や Rider などの開発環境でソリューションを開くことで、サーバーやクライアント、クラスライブラリーなど複数のプロジェクトを一括で操作、管理できます。
:::

## API サービスを定義する

MagicOnion ではサーバーがクライアントに提供する API サービスはすべて .NET のインターフェースとして定義されます。定義したサービスのインターフェースをクライアントとサーバー間で共有することで、それぞれサーバーの実装やクライアントからの呼び出しに利用します。

サービス定義となるインターフェースをプロジェクト `MyApp.Shared` に追加します。このプロジェクトはサーバーとクライアントでコードを共有するためのプロジェクトです。

ここでは単純な計算サービス `IMyFirstService` インターフェースを定義します。インターフェースは `x` と `y` の二つの `int` を受け取り、その合計値を返す `SumAsync` メソッドを持つものとします。

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


一般的な .NET でのインターフェース定義とほとんど同じですが、`IService<T>` を実装している点と戻り値の型が `UnaryResult` となっている点に注意してください。

`IService<T>` はこのインターフェースが Unary サービスであることを表すインターフェースです。Unary サービスとは 1 つのリクエストに対して 1 つのレスポンスを返す API サービスのことです。詳しくは [Unary の基礎](/unary/fundamentals) を参照してください。

戻り値は `UnaryResult` または `UnaryResult<T>` 型である必要があります。これは Task や ValueTask と同様に非同期メソッドとして扱われる MagicOnion 固有の特殊な型で、ここでの `UnaryResult<int>` は `int` 型の値をサーバーから受け取ることを表します。また API は常に非同期であるため `UnaryResult` を使う必要があり、メソッド名には `Async` サフィックスを付けることを推奨します。

:::tip
このプロジェクトにはテンプレートで作成された `Class1.cs` があらかじめ含まれているので削除してください。
:::

## サーバーを実装する

サービスのインターフェースを定義した後はサーバープロジェクト `MyApp.Server` でサービスを実装する必要があります。

### サーバーの初期構成

まず初めにテンプレートで作成した際にデフォルトで追加される gRPC のサンプル実装を削除します。サンプル実装は `Protos` フォルダーと `Services` フォルダーに含まれているのでフォルダーごと削除します。

次にサーバーの起動構成を設定します。ASP.NET Core アプリケーションは `Program.cs` でサーバーの機能に関する構成を行っています。

`Program.cs` 内で `builder.Services.AddMagicOnion()` と `app.MapMagicOnionService()` を呼び出すことで MagicOnion のサーバー機能を有効化します。作成時に記述されている `using MyApp.Server.Services` と `builder.Services.AddGrpc();` と `app.MapGrpcService<GreeterService>();` は削除してください。

これらを踏まえた `Program.cs` は下記のようになります。

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
MyApp.Server.csproj に `Protobuf` アイテムが残ってしまう場合があります。残っている場合にはこの項目は不要ですので削除してください。

```xml
  <ItemGroup>
    <Protobuf Include="Protos\greet.proto" GrpcServices="Server" />
  </ItemGroup>
```
:::

### API サービスの実装

次にクライアントからのリクエストを受けて処理を行う API サービスを実装します。

ここではサービスの定義として `IMyFirstService` インターフェースを定義しているので、API サービス (Unary サービス) の実装クラスは `IMyFirstService` インターフェースを実装する必要があり、同時に基底のクラスとして `ServiceBase<T>` を継承する必要もあります。

この二点を踏まえて `MyFirstService` クラスを作成します。下記はクラスの実装例です。

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

ここでは `Services` フォルダーを作成して追加していますがフォルダーの構成に特に制約はありません。

:::info
`UnaryResult` は `Task` や `ValueTask` などと同様に非同期メソッド (`async`) として扱えます。
:::

### サーバーの起動を確認する
サーバーの実装は完了したので、サーバーを起動して動作を確認します。サーバーの起動は Visual Studio や Rider などの IDE からデバッグ実行するか、ターミナルから `dotnet run` コマンドを実行して行います。

ビルドエラーがなく正常に起動できると下記のようなログが出力されて、サーバーが起動します。この時表示される `http://...` は後程 Unity クライアントから接続する際に使用するためメモしておくことをお勧めします。

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

ここまででディレクトリーとファイルは下記のような構成となっているはずです。これでサーバーに関する作業は完了です。

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

## Unity クライアントを実装する

次にサーバーを呼び出す Unity クライアントを実装します。Unity プロジェクト `MyApp.Unity` を Unity Editor で開きます。

### MagicOnion と関連するパッケージのインストール
初めに Unity プロジェクトから MagicOnion を使用するためにいくつかのパッケージをインストールする必要があります。大きく分けて下記の 3 つのパッケージをインストールします。

- NuGetForUnity
- MagicOnion.Client + Unity 向け拡張
- YetAnotherHttpHandler

#### NuGetForUnity のインストール
MagicOnion は NuGet パッケージで提供されているため、Unity で NuGet パッケージをインストールするための拡張である [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) をインストールする必要があります。

NuGetForUnity をインストールするには Package Manager の `Add package from git URL...` に下記の URL を指定します。詳しくは NuGetForUnity の README を参照してください。
```
https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
```

![](/img/docs/fig-quickstart-unity-nugetforunity.png)

#### MagicOnion.Client + Unity 向け拡張のインストール
MagicOnion のクライアントライブラリーである MagicOnion.Client パッケージは NuGet パッケージで提供されているので、NuGetForUnity を使用してインストールします。

メニューから `NuGet` → `Manage NuGet Packages` を選択し、NuGetForUnity のウィンドウで `MagicOnion.Client` を検索してインストールします。

![](/img/docs/fig-quickstart-unity-nuget-magiconionclient.png)

次に Unity 向けの拡張が含まれる Unity パッケージをインストールします。Unity Package Manager の `Add package from git URL...` に下記の URL を指定してください。

```
https://github.com/Cysharp/MagicOnion.git?path=src/MagicOnion.Client.Unity/Assets/Scripts/MagicOnion.Client.Unity#7.0.2
```

![](/img/docs/fig-quickstart-unity-upm-magiconion.png)

:::info
`7.0.2` の部分はインストールバージョンとなり、リリースされているバージョンによってはより新しいものを利用できる場合があります。
:::

#### YetAnotherHttpHandler のインストール

最後に Unity で gRPC (HTTP/2) 通信を行うための通信ライブラリーである [YetAnotherHttpHandler](https://github.com/Cysharp/YetAnotherHttpHandler) をインストールします。YetAnotherHttpHandler のインストールは動作に必要な NuGet パッケージと YetAnotherHttpHandler 本体の2つのインストール手順が必要となります。

はじめに動作に必要な NuGet パッケージをインストールします。これは NuGetForUnity から `System.IO.Pipelines` を検索し、インストールします。

![](/img/docs/fig-quickstart-unity-nuget-systemiopipelines.png)

次に YetAnotherHttpHandler を Unity Package Manager でインストールします。Package Manager の `Add package from git URL...` に下記の URL を指定してください。

```
https://github.com/Cysharp/YetAnotherHttpHandler.git?path=src/YetAnotherHttpHandler#1.10.0
```

:::info
`1.10.0` の部分はインストールバージョンとなり、リリースされているバージョンによってはより新しいものを利用できる場合があります。
:::

![](/img/docs/fig-quickstart-unity-yaha.png)

より詳しいインストール手順については [YetAnotherHttpHandler の README](https://github.com/Cysharp/YetAnotherHttpHandler) を参照してください。

ここまでのインストールを行うと Unity プロジェクトのパッケージ(NuGet および UPM)一覧は下記のようになります。

![](/img/docs/fig-quickstart-unity-installedpackages.png)

### 共有されたサービス定義を Unity プロジェクトから参照できるようにする

Unity クライアントから API を呼び出すためにサービス定義を参照する必要があります。サービス定義は `MyApp.Shared` プロジェクトに定義していますがこれは .NET のプロジェクトであり Unity プロジェクトから直接参照することはできません。

ここでは .NET サーバー側と Unity プロジェクトでサービス定義に関するソースコードを共有するための手法を2通り説明します。

#### 手法1: ファイルをコピーして共有する
一つ目の共有方法は単純にソースコードファイルをコピーして共有する方法です。これは単純に `MyApp.Shared` の `IMyFirstService.cs` ファイルを Unity プロジェクトにコピーします。

この手法のメリットはファイルコピーで済むことから試すことがとても簡単であることです。デメリットはファイルのコピーが手動で行われるため、ファイルの変更があった場合に手動でコピーする必要があります。

#### 手法2: ローカルパッケージとして共有する
2つ目の共有方法は `MyApp.Shared` プロジェクトを Unity のパッケージとして取り扱えるようにする方法です。これは1つ目の方法と異なり実体が同じファイルを指すことになるのでファイルの変更があった場合に同期をとる必要がありません。MagicOnion の開発ではこの手法を推奨しているため、このガイドではこの手法による共有を採用します。

.NET クラスライブラリープロジェクトを Unity のローカルパッケージとして扱うためにはいくつかの追加の手順が必要です。

初めに `MyApp.Shared` プロジェクトに `package.json` を追加します。これは Unity からパッケージとして認識できるようにするためのファイルです。下記の JSON ファイルを `MyApp.Shared` 配下に `package.json` として作成します。

```json title="src/MyApp.Shared/package.json"
{
  "name": "com.cysharp.magiconion.samples.myapp.shared.unity",
  "version": "1.0.0",
  "displayName": "MyApp.Shared.Unity",
  "description": "MyApp.Shared.Unity"
}
```

また、Unity 上で個別のアセンブリーとなるように Assembly Definition ファイルも追加します。Assembly Definition ファイルは `MyApp.Shared` に `MyApp.Shared.Unity.asmdef` という名前で下記の内容で作成します。

```json title="src/MyApp.Shared/MyApp.Shared.Unity.asmdef"
{
    "name": "MyApp.Shared.Unity"
}
```

:::note
Assembly Definition ファイルのファイル名は Unity から IDE を開く際のプロジェクト名として使用されます。そのためサーバー向けの `MyApp.Shared` と区別がつくよう `.Unity` サフィックスを付けることを推奨します。
:::

最後に `MyApp.Shared` プロジェクトに `Directory.Build.props` と `Directory.Build.targets` を追加します。これは .NET プロジェクトで `bin`, `obj` フォルダーを出力しないように構成(代わりに `.artifacts` に出力)し、Unity 向けの .meta などのファイルを IDE 上から非表示にします。


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
これらのファイルを追加した後、プロジェクトをビルドして `bin` および `obj` フォルダーが残っている場合は必ず削除してください。ファイルが残り続けていると Unity から認識されて問題を引き起こす可能性があります。
:::

:::warning
macOS の Finder ではデフォルトで `.` から始まるファイルを非表示にする設定がされているため、`.artifacts` フォルダーが表示されない場合があります。Finder で表示するには `Command + Shift + .` を押すか、`defaults` コマンドで設定を変更する必要があります。
:::

ここまでの手順で `MyApp.Shared` プロジェクトは下記のようなファイル構成となります。

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

### 共有プロジェクトのローカルパッケージを参照する

`MyApp.Shared` をローカルパッケージとして共有できるようにしたので Unity プロジェクトでローカルパッケージの参照を追加する必要があります。

ローカルパッケージの参照を追加するには `MyApp.Unity/Packages/manifest.json` にパッケージへのパスを追加します。ここでは `file:../../MyApp.Shared` として相対パスで追加します。

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
Unity Editor の Package Manager の `Install package from disk...` を使用してインストールすると `manifest.json` に絶対パスで保存されるため注意してください。
:::

![](/img/docs/fig-quickstart-unity-localpackage.png)

### MagicOnion クライアントを構成する
Unity プロジェクトから MagicOnion を使用するためには gRPC の接続を管理するプロバイダーホストと呼ばれる仕組みの初期化が必要です。

`MyApp.Unity` プロジェクトに下記のソースコードを含む `MagicOnionInitializer` クラスを追加してください。

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

Unity との統合について詳しくは [Unity との統合](/integration/unity) を参照してください。

### サーバーに接続して API を呼び出す

ここまでで MagicOnion を使用するための準備が整ったので、Unity クライアントからサーバーに接続して API を呼び出す処理を実装します。ここでは説明を簡単にするため、`MonoBehaviour` の `Start` メソッドでサーバーに接続して API を呼び出して、デバッグログとして出力するといった実装を行います。

初めにサーバーに接続するのに必要な gRPC のチャンネルを作成します。MagicOnion では `GrpcChannelx` クラスを使用してチャンネルを作成できます。ここでは `GrpcChannelx.ForAddress` メソッドを使用して指定したアドレスに接続するチャンネルを作成します。指定するアドレスはサーバーを実装するセクションで起動した際に表示された `http://...` を使用します。

```csharp
var channel = GrpcChannelx.ForAddress("http://localhost:5210");
```

作成したチャンネルを使用して MagicOnion のクライアントを作成します。これは `MagicOnionClient` クラスの `Create` メソッドで行います。`Create` メソッドには作成したチャンネルと API サービスのインターフェースを指定します。これによって `IMyFirstService` インターフェースを実装したクライアントが作成されます。

```csharp
var client = MagicOnionClient.Create<IMyFirstService>(channel);
```

最後に作成されたクライアントを使用してメソッドを呼び出します。これは通常のインターフェースのメソッド呼び出しと同様に行えます。メソッドは非同期メソッドなので呼び出し結果の `UnaryResult` を `await` で待機する必要があります。

```csharp
var result = await client.SumAsync(100, 200);
```

ここまでの手順をまとめ、MonoBehaviour に実装すると下記のようになります。ここでは SampleScene で使用する MonoBehaviour として `SampleScene` クラスを作成しています。

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
`async void` の使用については `Start` のようなイベントなど一部を除いて使用を避けることをお勧めします。また UniTask を導入している場合には `async void` の代わりに `async UniTaskVoid` を使用することをお勧めします。
:::

最後にシーンに GameObject を追加し、その GameObject に `SampleScene` スクリプトをアタッチします。これで Unity クライアントからサーバーに接続して API を呼び出す準備が整いました。

Unity Editor で Play モードに入ることで `Start` メソッドが呼び出され、サーバーに接続して API を呼び出す処理が実行されます。Console ログに `100 + 200 = 300` と表示されれば正常にサーバーに接続できて API を呼び出すことができています。

![](/img/docs/fig-quickstart-unity-unarydebuglog.png)


#### トラブルシューティング
- `IOException: client error (Connect): tcp connect error: No connection could be made because the target machine actively refused it. (os error 10061)`
  - サーバーに接続できない状態です。サーバーが起動しているか、ポート番号が正しい確認してください
- `IOException: client error (Connect): invalid peer certificate: UnknownIssuer`
  - `https://` に接続しようとしてる場合に発生するエラーです。開発向け証明書を認識できないため発生します。`http://...` で接続してください(その際ポート番号には注意してください)。

## 関連リソース
- [Unity での利用](/installation/unity): Unity でのセットアップ手順
- [プロジェクト構成](/fundamentals/project-structure): Unity と .NET のプロジェクト構成について
- [IL2CPP ビルドでの注意事項](/fundamentals/aot)
- [Unity 統合](/integration/unity): Unity エディター拡張について
