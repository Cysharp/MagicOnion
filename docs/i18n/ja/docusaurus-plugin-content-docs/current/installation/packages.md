# パッケージインストールガイド

MagicOnion は 4 つの NuGet パッケージで提供されています。必要に応じていずれかのパッケージをインストールしてください。

## MagicOnion.Server
`MagicOnion.Server` パッケージはサーバーを実装するためのものです。サーバー上でサービスを実装するためには、このパッケージをインストールする必要があります。

```bash
dotnet add package MagicOnion.Server
```

## MagicOnion.Client
`MagicOnion.Client` パッケージはクライアントを実装するためのものです。マイクロサービスや WPF、.NET MAUI などのクライアントを実装するためには、このパッケージをインストールする必要があります。

```bash
dotnet add package MagicOnion.Client
```

:::tip
Unity アプリケーションで MagicOnion クライアントを使用する場合は、[Unity での利用](unity) ページも参照してください。
:::

## MagicOnion.Abstractions
`MagicOnion.Abstractions` パッケージは、サーバーとクライアントで共通に使用されるインターフェースと属性を提供します。サーバーとクライアント間で共有されるクラスライブラリプロジェクトを作成する場合は、このパッケージをインストールする必要があります。

```bash
dotnet add package MagicOnion.Abstractions
```

## MagicOnion (メタパッケージ)
`MagicOnion` パッケージはサーバーとクライアント両方の役割を実装する場合に使用できるメタパッケージです。
マイクロサービスのようなサーバーとサーバーの通信を実装する、サーバーとクライアントどちらも実装する場合にはこのパッケージのインストールを推奨します。

```bash
dotnet add package MagicOnion
```
