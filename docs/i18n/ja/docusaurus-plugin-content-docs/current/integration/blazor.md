# ASP.NET Core Blazor

MagicOnion はクライアントに ASP.NET Core Blazor を使用したシナリオをサポートします。ここでは MagicOnion を Blazor で利用するための方法や注意点、制限について説明します。

## ホスティングモデルとレンダーモード
Blazor には Blazor WebAssembly と Blazor Server の2つのホスティングモデルがあり、MagicOnion の一部の機能はサポートされない場合があります。

また、.NET 8 以降の Blazor はレンダーモードにより実際のホスティングモデルが決定されます。例えば、対話型サーバーモードであれば Blazor Server であり、対話型 WebAssembly であれば Blazor WebAssembly です。両方の特性を併せ持つ、自動モードもあります。レンダーモードによって決定されるホスティングモデルによってサポートされる MagicOnion の機能や対応が異なります。

## 静的サーバー (Blazor Server)
静的サーバーモード (静的 SSR) はサーバー上でレンダリングした HTML を返すモードです。このモードでは ASP.NET Core が動作するサーバー上でコードを実行するため、MagicOnion クライアントは特別な制限はなく動作します。

ただし静的サーバーモードはその特性上 StreamingHub のようなリアルタイム通信の処理を扱うことには適さないことに注意が必要です。

## 対話型サーバー (Blazor Server)

対話型サーバーモード (対話型 SSR) はブラウザーとサーバーを継続的に接続し、クライアントの操作をサーバーに伝え、サーバー上でレンダリングした HTML を返す対話的なモードです。このモードでは ASP.NET Core が動作するサーバー上でコードを実行するため、MagicOnion クライアントは特別な制限はなく動作します。

StreamingHub を使用したリアルタイム通信での表示など、対話型サーバーモードであれば問題なく動作します。

## 対話型 WebAssembly (Blazor WebAssembly)

対話型 WebAssembly はクライアント、つまりブラウザー側でコードを実行するモードです。このモードでは MagicOnion クライアントはブラウザー上で動作するため HTTP/2 を使用した gRPC を直接利用できません。クライアントとサーバーにこの制限を回避するための追加の対応が必要となります。MagicOnion では下記のいずれかの対応をサポートします。

- gRPC-Web
- GrpcWebSocketBridge

### gRPC-Web を使用する

gRPC-Web は gRPC プロジェクトで開発されている Web ブラウザーから gRPC を利用するための仕組みです。これは .NET では Grpc.AspNetCore.Web と Grpc.Net.Client.Web パッケージで利用できます。詳しくは [ASP.NET Core gRPC アプリでの gRPC-Web](https://learn.microsoft.com/ja-jp/aspnet/core/grpc/grpcweb?view=aspnetcore-9.0) を参照してください。

:::warning
gRPC-Web を使用する場合、 MagicOnion で利用できるのは Unary のみで StreamingHub はサポートされません。これは gRPC-Web が Duplex Streaming に対応していないことによる制限です。
:::

### GrpcWebSocketBridge を使用する

[GrpcWebSocketBridge](https://github.com/Cysharp/GrpcWebSocketBridge) は Cysharp の提供する WebSocket を使用して gRPC 通信を行うためのライブラリーです。これは gRPC-Web と異なり StreamingHub の動作をサポートしています。

GrpcWebSocketBridge は gRPC-Web をベースとしていますが、gRPC-Web エコシステムとは互換性がないため、他言語からの接続や gRPC-Web を前提としたプロキシーといった仕組みとの連携はサポートされません。

## 対話型自動 (Blazor Server + WebAssembly)

対話型自動モードは接続時にはサーバー対話型として開始され、その後クライアント側でアプリケーションコードの読み込みが完了すると WebAssembly 対話型に切り替えるモードです。

このモードでは Blazor Server と Blazor WebAssembly の特性の両方を持つため、それぞれのレンダリングでの対応が必要となります。特に MagicOnion や gRPC を取り扱う場合は WebAssembly かどうかで利用可能な通信方式が異なるため両方への対応が必要となります。

:::info
MagicOnion でインタラクティブなアプリケーションを実装する場合にはアーキテクチャーの複雑性を減らすために、レンダーモードには対話型サーバーまたは WebAssembly モードのどちらかを利用することを推奨します。
:::
