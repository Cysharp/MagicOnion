---
title: MagicOnion について
---

# MagicOnion

Unified Realtime/API framework for .NET platform and Unity.

## MagicOnion について

MagicOnion は SignalR や Socket.io、WCF、Web ベースの API などの RPC メカニズムと同様に、.NET プラットフォーム向けの双方向リアルタイム通信を提供するモダンな RPC フレームワークです。

このフレームワークは gRPC を元にしており、高速でコンパクトなネットワークトランスポートである HTTP/2 に基づいています。ただし、通常の gRPC とは異なり、C# インターフェースをプロトコルスキーマとして扱い、`.proto` (Protocol Buffers IDL) なしで C# プロジェクト間でのコード共有を実現します。

インターフェースはスキーマであり、プレーンな C# コードと同様に API サービスを提供します。

![image](https://user-images.githubusercontent.com/46207/50965239-c4fdb000-1514-11e9-8365-304c776ffd77.png)

StreamingHub リアルタイム通信サービスを使用することで、サーバーは複数のクライアントにデータを配信できます。

![image](https://user-images.githubusercontent.com/46207/50965825-7bae6000-1516-11e9-9501-dc91582f4d1b.png)

MagicOnion は [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) を使用して呼び出しの引数と戻り値をシリアライズします。MessagePack オブジェクトにシリアライズできる NET プリミティブおよびその他の複雑な型を使用できます。シリアライゼーションの詳細については MessagePack for C# を参照してください。

## ユースケース

MagicOnion は以下のユースケースで採用または置換できます:

- RPC サービス (Microservices で使用される gRPC や WinForms/WPF で一般的な WCF)
- Windows での WPF アプリケーション、Unity ゲームや .NET for iOS, Android, .NET MAUI など様々なプラットフォームとクライアントをターゲットとする ASP.NET Core Web API がカバーする API サービス
- Socket.io、SignalR、Photon、UNet などの双方向リアルタイム通信

MagicOnion は API サービスとリアルタイム通信のどちらもサポートしているため、様々なユースケースに適しています。これらの機能のどちらかのみを使用することもできますが、両方を組み合わせた構成もサポートされます。

![](/img/docs/fig-usecase.png)

## 技術スタック

MagicOnion は様々なモダンな技術の上に構築されています。

![](/img/docs/fig-technology-stack.png)

サーバーは ASP.NET Core 上に実装された gRPC サーバー (grpc-dotnet) 上に実装され、ASP.NET Core とその上の Grpc.AspNetCore.Server の機能を活用しています。これには DI やロギング、メトリクスや Hosting API なども含まれます。

ネットワークは HTTP/2 プロトコルを使用し、gRPC によるバイナリーメッセージングを活用しています。バイナリー表現には gRPC でよく使用される Protocol Buffers ではなく .NET との親和性や表現力の高い MessagePack を採用しています。

クライアントは .NET ランタイムだけでなく Unity ゲームエンジンのランタイムもサポートしています。これらのランタイムの上に .NET 標準の HttpClient を基盤とした gRPC クライアント (grpc-dotnet) を使用し、それを元に MagicOnion のクライアントライブラリを構築しています。
