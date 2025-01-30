# Unary と StreamingHub

MagicOnion では Unary サービスと StreamingHub サービスの2種類の API 実装方式を提供します。これらのどちらを使用しても RPC スタイルの API を定義できます。

Unary と StreamingHub の違いは次の通りです:

- Unary は一度に1つのリクエストと1つのレスポンスを処理するシンプルな HTTP の POST リクエストです
    - 詳細: [Unary サービスの基礎](../unary/)
- StreamingHub は継続した接続を使用してクライアントとサーバー間でメッセージを送りあう双方向通信です
    - 詳細: [StreamingHub の基礎](../streaminghub/)

![](/img/docs/fig-unary-streaminghub.png)

すべてを StreamingHub で実装することも可能ですが、サーバーからの通知を必要としない一般的な API (REST や Web API の代わり) では Unary を使用することを推奨します。

## Unary のメリット

- ロードバランシングと Observability
    - Unary は実質 HTTP POST 呼び出しなので ASP.NET Core をはじめ、ロードバランサーや CDN、WAF のような既存の仕組みとの親和性があります
    - StreamingHub は一つの長時間リクエストであり、内部の Hub メソッド呼び出しをログに残すことはインフラストラクチャーレベルでは困難です
        - これは Hub メソッドの呼び出しはロードバランスできないことを意味します
- StreamingHub のオーバーヘッド
    - Unary の実体はシンプルな HTTP POST であり、StreamingHub のような接続の確立が不要です
    - StreamingHub は接続時にメッセージループの起動やハートビートのセットアップといった追加の処理を行うオーバーヘッドがあります

## StreamingHub のメリット

- サーバーからクライアント(複数含む)へのリアルタイムメッセージ送信
    - サーバーからクライアントに対しての通知が必要な場合は StreamingHub の使用を検討してください
    - 例えばチャットのメッセージ通知やゲームの位置同期などが該当します
    - Unary や通常の HTTP リクエストの場合に必要なポーリング/ロングポーリングの代わりとなります
