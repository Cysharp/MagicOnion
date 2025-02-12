# プロジェクト構成

このページでは MagicOnion を使用したプロジェクトの推奨構成について解説します。

.NET アプリケーションでは一般的にソリューションという大きな単位があり、その中にアプリケーションやライブラリーなど .NET のプロジェクトが含まれる形で構成されます。
MagicOnion を使用するアプリケーションはその性質上、最低限サーバーとクライアントのプロジェクトが存在し、何らかの手段でインターフェースを共有するといった構成がとられます。

## 典型的な .NET アプリケーションの構成

ここでは典型的な .NET クライアント (コンソールアプリケーションや WPF、.NET MAUI など) とサーバー、そしてインターフェースを共有する構成例について解説します。

典型的な .NET アプリケーションでのプロジェクト(ソリューション)構成は以下のようになります。

- **MyApp.sln**: ソリューションファイル
- **MyApp.Server**: MagicOnion サーバーアプリケーション (ASP.NET Core gRPC Server)
    - このプロジェクトは MagicOnion の API を提供し、クライアントからのリクエストを処理するサーバーアプリケーションです
- **MyApp.Client**: MagicOnion クライアントアプリケーション (Console, WPF, WinForms, .NET MAUI, etc...)
    - このプロジェクトは MagicOnion サーバーに接続し、リクエストを送信するクライアントアプリケーションです
- **MyApp.Shared**: インターフェース共有ライブラリー (.NET ライブラリープロジェクト)
    - このプロジェクトには MagicOnion 向けのインターフェース定義が含まれます。例えば Hub や Service のインターフェース定義、リクエスト/レスポンスで使用される MessagePack の型などです

:::note
MyApp は任意の名前で、プロジェクト名やソリューション名に置き換えてください。
:::

下記は上記の構成でのファイルの配置例です。

```plaintext
MyApp.sln
|─ src
|  ├─ MyApp.Server
|  │  ├─ MyApp.Server.csproj
|  │  ├─ Program.cs
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

`MyApp.Shared` プロジェクトは .NET クラスライブラリーとして作成し、`MagicOnion.Abstractions` パッケージを参照し、純粋なインターフェースの定義やデータ型、列挙型のみを定義します。
そして `MyApp.Server` と `MyApp.Client` プロジェクトはそれぞれ `MyApp.Shared` プロジェクトをプロジェクト参照することでインターフェースを共有します。

これは最低限の構成の例であり、実際のプロジェクトでは必要に応じてモデルやドメイン、ViewModel などのプロジェクトや階層を持つことがあります。

## Unity アプリケーションの構成

ここでは Unity クライアントとサーバー、そしてインターフェースを共有する構成例について解説します。

Unity アプリケーションでの構成は Unity プロジェクトとサーバー (.NET) プロジェクトでのインターフェースを共有する方法が典型的な .NET プロジェクトと異なります。
これは Unity プロジェクトからは .NET ライブラリープロジェクトを参照できないといった理由によります。

推奨する構成は以下のようになります。

- **MyApp.sln**: ソリューションファイル
- **MyApp.Server**: MagicOnion サーバーアプリケーション (ASP.NET Core gRPC Server)
- **MyApp.Unity**: Unity クライアントアプリケーション
- **MyApp.Shared**: インターフェース共有ライブラリー (.NET ライブラリープロジェクト兼 Unity ローカルパッケージ)

```plaintext
MyApp.sln
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

MyApp.Shared プロジェクトは .NET クラスライブラリーとして作成し、`MagicOnion.Abstractions` パッケージを参照し、純粋なインターフェースの定義やデータ型、列挙型のみを定義します。加えて Unity プロジェクトから Unity Package Manager によって参照できるように `package.json` を含め、`bin`, `obj` といったフォルダーを出力しないように構成します。これにより Unity プロジェクトからも MyApp.Shared に含まれる C# ソースコードを参照できるようになります。

`package.json` の内容は下記のような最小構成の JSON ファイルです。

```json title="src/MyApp.Shared/package.json"
{
  "name": "com.cysharp.magiconion.samples.myapp.shared.unity",
  "version": "1.0.0",
  "displayName": "MyApp.Shared.Unity",
  "description": "MyApp.Shared.Unity"
}
```

アセンブリーを分割するために `MyApp.Shared.Unity.asmdef` (Assembly Definition) ファイルを追加することもお勧めします。`.Unity` サフィックスを付けるなど `MyApp.Shared` と完全に同じ名前にならないように注意してください。

次に `MyApp.Shared` に下記の2つの設定ファイル(`Directory.Build.props` と `Directory.Build.targets`) を追加します。`bin`, `obj` ディレクトリーを作成しないようにする設定と Unity 向けのファイルを IDE 上に表示しないようにする設定です。

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

この設定により中間ファイルやビルド済みファイルは `.artifacts` 以下に出力されるようになるため、`bin`, `obj` ディレクトリーが作成されている場合は削除してください。

次に Unity プロジェクトの `Packages/manifest.json` に MyApp.Shared への参照を追加します。

```json title="src/MyApp.Unity/Packages/manifest.json"
{
  "dependencies": {
    "com.cysharp.magiconion.samples.myapp.shared.unity": "file:../../MyApp.Shared",
    ...
  }
}
```

MyApp.Server プロジェクトからは MyApp.Shared プロジェクトをプロジェクト参照することでインターフェースを共有します。これは典型的な .NET アプリケーションの場合と同様です。

### SlnMerge によるソリューションの統合

Unity 向けのエディター拡張の [SlnMerge](https://github.com/Cysharp/SlnMerge) を使用することで Unity が生成するソリューションと .NET プロジェクトのソリューションを統合できます。

例えば MyApp.sln には MyApp.Server と MyApp.Shared プロジェクトが含まれますが、Unity で生成されるソリューションには Unity 向けのプロジェクト (Assembly-CSharp, Assembly-CSharp-Editor など) のみが含まれます。SlnMerge を使用することでこれらのソリューションを統合し、Unity からソリューションを開いた場合でもシームレスにサーバープロジェクトを参照できるようになります。

これにより Unity と .NET プロジェクト間での参照検索やデバッガーステップインなどが可能となりより良い開発体験を得られます。
