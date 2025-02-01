# クライアント結果

:::tip
この機能は MagicOnion v7.0.0 で追加されました。
:::

"クライアント結果" は [SignalR で実装されている同名の機能](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-8.0#client-results) に触発されたものです。

従来、StreamingHub はサーバーからクライアントに対してはメッセージを一方的に送信する(Fire-and-Forget)ことしかできませんでしたが、クライアント結果はサーバーの Hub やアプリケーションロジックから特定のクライアントのメソッドを呼び出し、その結果を受け取ることができます。

```csharp
interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
{
}

interface IMyHubReveiver
{
    // The Client results method is defined in the Receiver with a return type of Task or Task<T>
    Task<string> HelloAsync(string name, int age);

    // Regular broadcast method
    void OnMessage(string message);
}

// Client implementation
class MyHubReceiver : IMyHubReceiver
{
    public async Task<string> HelloAsync(string name, int age)
    {
        Console.WriteLine($"Hello from {name} ({age})");
        var result = await ReadInputAsync();
        return result;
    }
    public void OnMessage()
    {
        Console.WriteLine($"OnMessage: {message}");
    }
}
```

サーバー上ではグループの `Client` または `Single` を介してメソッド呼び出しを行い、結果を受け取ることができます。

```csharp
var result = await Client.HelloAsync();
Console.WriteLine(result);
// or
var result2 = await _group.Single(clientId).HelloAsync();
Console.WriteLine(result2);
```

## 例外
クライアント上で例外が発生した場合、呼び出し元には `RpcException` がスローされ、接続が切断された場合やタイムアウトが発生した場合は `TaskCanceledException` (`OperationCanceledException`) がスローされます。

## タイムアウト
デフォルトではサーバーからクライアントへの呼び出しのタイムアウトは 5 秒です。タイムアウトが超過した場合、呼び出し元には `TaskCanceledException` (`OperationCanceledException`) がスローされます。デフォルトのタイムアウトは `MagicOnionOptions.ClientResultsDefaultTimeout` プロパティを介して設定できます。

メソッド呼び出しごとにタイムアウトを明示的にオーバーライドするにはメソッド引数として `CancellationToken` を指定し、任意のタイミングで `CancellationToken` を渡してタイムアウトを指定します。このキャンセルはクライアントには伝播しないことに注意してください。クライアントは常に `default(CancellationToken)` を受け取ります。

```csharp
interface IMyHubReceiver
{
    Task<string> DoLongWorkAsync(CancellationToken timeoutCancellationToken = default);
}
```
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var result = await Client.DoLongWorkAsync(cts.Token);
Console.WriteLine(result);
```

## 制限事項
- 複数のクライアントに対して呼び出しを行うことはサポートされていません。`Client` または `Single` を使用する必要があります
- グループのバックプレーンとして Redis または NAT が使用されている場合、クライアント結果はサポートされていません
- クライアント側でのクライアント結果メソッド呼び出し中に Hub メソッドを呼び出すとデッドロックが発生します
