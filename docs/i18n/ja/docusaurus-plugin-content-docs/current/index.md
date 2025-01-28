---
title: MagicOnion について
---

# MagicOnion

Unified Realtime/API framework for .NET platform and Unity.

## MagicOnion について

MagicOnion は SignalR や Socket.io、WCF、Web ベースの API などの RPC メカニズムと同様に、.NET プラットフォーム向けの双方向リアルタイム通信を提供するモダンな RPC フレームワークです。

このフレームワークは gRPC を元にしており、高速でコンパクトなネットワークトランスポートである HTTP/2 に基づいています。ただし、通常の gRPC とは異なり、C# インターフェイスをプロトコルスキーマとして扱い、`.proto` (Protocol Buffers IDL) なしで C# プロジェクト間でのコード共有を実現します。

インターフェースはスキーマであり、プレーンな C# コードと同様に API サービスを提供します。

![image](https://user-images.githubusercontent.com/46207/50965239-c4fdb000-1514-11e9-8365-304c776ffd77.png)

StreamingHub リアルタイム通信サービスを使用することで、サーバーは複数のクライアントにデータを配信できます。

![image](https://user-images.githubusercontent.com/46207/50965825-7bae6000-1516-11e9-9501-dc91582f4d1b.png)

MagicOnion は [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) を使用して呼び出しの引数と戻り値をシリアライズします。MessagePack オブジェクトにシリアライズできる NET プリミティブおよびその他の複雑な型を使用できます。シリアライゼーションの詳細については MessagePack for C# を参照してください。

## ユースケース

MagicOnion は以下のユースケースで採用または置換できます:

- RPC サービス (Microservices で使用される gRPC や WinForms/WPF で一般的な WCF)
- Unity、.NET MAUI、Windows クライアントをターゲットとする ASP.NET Core MVC の API サービス
- Socket.io、SignalR、Photon、UNet などの双方向リアルタイム通信

MagicOnion は API サービスとリアルタイム通信のどちらもサポートしているため、様々なユースケースに適しています。これらの機能のどちらかのみを使用することもできますが、両方を組み合わせた構成もサポートされます。

![](/img/docs/fig-usecase.png)
