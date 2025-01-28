# 例外のハンドリングとステータスコード

MagicOnion ではサーバーの処理結果を戻り値とは別にステータスコードとして返す仕組みがあります。これは HTTP のステータスコードや gRPC のステータスコードと同様です。このステータスコードにはアプリケーション固有のステータスコードを含めることもできます。

## サーバーからクライアントへのステータスコードの通知
サーバーからクライアントへカスタムステータスコードを返す場合は `ReturnStatusException` を使用できます。

```csharp
public Task SendMessageAsync(string message)
{
    if (message.Contains("foo"))
    {
        //
        throw new ReturnStatusException((Grpc.Core.StatusCode)MyStatusCode.SomethingWentWrong, "invalid");
    }

    // ....
```

`ReturnStatusException` はステータスコードである `Grpc.Core.StatusCode` 列挙型を持ち、この値をクライアントへ通知します。アプリケーションのカスタムステータスコードを返すには独自の `int` 値または列挙型を `Grpc.Core.StatusCode` 列挙型にキャストしてください。

例外のスローを避けるためにパフォーマンスを重視する場合は `CallContext.Status` (`ServiceContext.CallContext.Status`) を使用してステータスを直接設定できます。

## クライアントでの例外ハンドリング
クライアントではメソッド呼び出し時の例外はすべて gRPC の `RpcException` として受信されます。

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel);
try
{
    var result = await client.SayHelloAsync("Alice", 18);
}
catch (RpcException ex)
{
    // handle exception ...
    if (((MyStatusCode)ex.Status.StatusCode) == MyStatusCode.SomethingWentWrong)
    {
        // ...
    }
}
```

ネットワークに問題がある場合は `RpcException` が `StatusCode.Unavailable` でスローされます。詳しいエラー内容は `InnerException` プロパティーで調査できます。

## サーバー上での未処理の例外
サーバーのメソッド呼び出し中にエラーが発生し `ReturnStatusException` を除くハンドルされない例外がスローされた場合、クライアントでは `StatusCode.Unknown` がセットされた `RpcException` として取り扱われます。

この際 `MagicOnionOption.IsReturnExceptionStackTraceInErrorDetail` が `true` の場合、クライアントはサーバーの例外のスタックトレースを受信できます。デバッグには非常に便利ですがセキュリティ上の重大な問題があるためデバッグビルドでのみ有効にする必要があります。
