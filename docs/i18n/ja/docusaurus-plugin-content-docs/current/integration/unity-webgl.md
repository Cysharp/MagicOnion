# Unity WebGL

MagicOnion は Unity の WebGL プラットフォームを試験的にサポートしています。このページでは WebGL プラットフォームでの導入方法と制限事項について説明します。

## 導入方法

Unity の WebGL プラットフォームで MagicOnion を使用するには IL2CPP 対応に加えて、[GrpcWebSocketBridge](https://github.com/Cysharp/GrpcWebSocketBridge) のインストールが必要となります。

GrpcWebSocketBridge は gRPC 通信を WebSocket の上で実現するためのライブラリーです。このライブラリーをクライアントとサーバーに導入することでブラウザー上から MagicOnion サーバーと通信できるようになります。

## 制限事項
- 現時点でクライアント側からのハートビートはサポートされません
    - ハートビートの実装が依存するランタイム機能にスレッドベースのタイマー(System.Threading.Timer)を内部的に使用してる部分があり、Unity WebGL ではスレッドがサポートされていないため動作しません
