# Unary サービスを始める

このチュートリアルでは Unary サービスの実装を始めるための簡単な手順を紹介します。

## 手順

Unary サービスを定義、実装、利用するには下記の手順が必要となります。

- サーバーとクライアントの間で共有する Unary サービスインターフェースを定義する
- サーバープロジェクトで定義した Unary サービスインターフェースを実装する
- クライアントプロジェクトから定義した Unary サービスを呼び出す

## サーバーとクライアントの間で共有する Unary サービスインターフェースを定義する

共有ライブラリープロジェクトに Unary サービスのインターフェースを定義します (Unity の場合はソースコードコピーやファイルリンクで対応します)。Unary サービスのインターフェースは `IService<TSelf>` を継承する必要があります。

以下は挨拶を返す Unary サービスインターフェースの例です。

```csharp
public interface IGreeterService : IService<IGreeterService>
{
    UnaryResult<string> SayHelloAsync(string name, int age);
}
```

Unary サービスに定義するメソッドの戻り値の型は `UnaryResult` または `UnaryResult<T>` である必要があります。これは `ValueTask`, `ValueTask<T>` と同じような意味を持つ、Unary サービス特有の戻り値の型です。

## サーバープロジェクトで定義した Unary サービスインターフェースを実装する

サーバー上でクライアントから呼び出される Unary サービスを実装します。サーバーの実装は `ServiceBase<TSelf>` を継承し、定義した Unary サービスインターフェースを実装する必要があります。

```csharp
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public async UnaryResult<string> SayHelloAsync(string name, int age)
    {
        return $"Hello {name}! Your age is {age}.";
    }
}
```

`UnaryResult` および `UnaryResult<T>` 型は `ValueTask`, `Task` などと同様に非同期メソッド (`async`) として定義できます。

メソッドの処理に非同期を必要としない場合は `UnaryResult.FromResult` や `UnaryResult.CompletedResult` などを使用して同期的に処理を返すこともできます。


```csharp
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public UnaryResult<string> SayHelloAsync(string name, int age)
    {
        return UnaryResult.FromResult($"Hello {name}! Your age is {age}.");
    }
}
```

## クライアントプロジェクトから定義した Unary サービスを呼び出す

クライアントから Unary サービスのメソッドを呼び出すには `MagicOnionClient.Create<T>` メソッドを使用します。

`Create<T>` メソッドには Unary サービスインターフェースと接続先の `GrpcChannel` オブジェクトを渡します。このメソッドは指定したインターフェースに対応するクライアントプロキシーを生成します。この時点ではサーバーにリクエストは発生しません。

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = await MagicOnionClient.Create<IGreeterService>(channel, receiver);
```

生成したクライアントプロキシーを使用して Unary サービスのメソッドを呼び出します。

```csharp
var result = await client.SayHelloAsync("Alice", 18);
Console.WriteLine(result);
```
