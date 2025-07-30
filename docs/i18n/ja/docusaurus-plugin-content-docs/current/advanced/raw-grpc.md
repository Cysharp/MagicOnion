# Raw gRPC API

MagicOnion はプリミティブな gRPC API（ClientStreaming、ServerStreaming、DuplexStreaming）を定義して使用することができます。特に DuplexStreaming は StreamingHub の基礎となって使用されています。特別な理由がない限り、StreamingHub の使用を推奨します。

## ServerStreaming の使い方

ServerStreaming はサーバーからクライアントへ複数の値を送信するストリーミングパターンです。クライアントは単一のリクエストを送信し、サーバーは複数のレスポンスを返すことができます。

### サーバー側の実装

ServerStreaming を実装するには、`GetServerStreamingContext<T>()` を使用してストリーミングコンテキストを取得します。

```csharp
public async Task<ServerStreamingResult<WeatherData>> GetWeatherUpdatesAsync(string location, int count)
{
    var stream = GetServerStreamingContext<WeatherData>();

    // 指定された回数だけ天気データを送信
    for (int i = 0; i < count; i++)
    {
        var weatherData = new WeatherData
        {
            Temperature = Random.Shared.Next(-10, 35),
            Humidity = Random.Shared.Next(30, 90),
            Timestamp = DateTime.UtcNow
        };

        await stream.WriteAsync(weatherData);
        
        // 1秒待機（リアルタイムデータのシミュレーション）
        await Task.Delay(1000);
    }

    return stream.Result();
}
```

### クライアント側の実装

クライアント側では、`ResponseStream.ReadAllAsync()` を使用してサーバーから送信されるすべての値を受信します。

```csharp
var client = MagicOnionClient.Create<IWeatherService>(channel);
var stream = await client.GetWeatherUpdatesAsync("Tokyo", 5);

await foreach (var weatherData in stream.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"気温: {weatherData.Temperature}°C, 湿度: {weatherData.Humidity}%, 時刻: {weatherData.Timestamp}");
}
```

### 使用例

ServerStreaming は以下のようなシナリオで有用です：

- リアルタイムデータフィード（株価、センサーデータなど）
- 大量データの分割送信
- プログレス更新の通知
- ログのストリーミング

## ClientStreaming の使い方

ClientStreaming はクライアントからサーバーへ複数の値を送信するストリーミングパターンです。クライアントは複数のメッセージを送信し、サーバーは単一のレスポンスを返します。

### サーバー側の実装

ClientStreaming を実装するには、`GetClientStreamingContext<TRequest, TResponse>()` を使用してストリーミングコンテキストを取得します。

```csharp
public async Task<ClientStreamingResult<SensorData, AnalysisResult>> AnalyzeSensorDataAsync()
{
    var stream = GetClientStreamingContext<SensorData, AnalysisResult>();

    var allData = new List<SensorData>();
    
    // クライアントからのすべてのデータを受信
    await foreach (var data in stream.ReadAllAsync())
    {
        Logger.Debug($"Received sensor data: {data.Value} at {data.Timestamp}");
        allData.Add(data);
    }

    // 受信したデータを分析
    var result = new AnalysisResult
    {
        Average = allData.Average(d => d.Value),
        Max = allData.Max(d => d.Value),
        Min = allData.Min(d => d.Value),
        Count = allData.Count
    };

    return stream.Result(result);
}
```

### クライアント側の実装

クライアント側では、`RequestStream.WriteAsync()` を使用して複数の値を送信し、最後に `CompleteAsync()` を呼び出してストリームを完了します。

```csharp
var client = MagicOnionClient.Create<ISensorService>(channel);
var stream = await client.AnalyzeSensorDataAsync();

// センサーデータを送信
for (int i = 0; i < 10; i++)
{
    var sensorData = new SensorData
    {
        Value = Random.Shared.NextDouble() * 100,
        Timestamp = DateTime.UtcNow
    };
    
    await stream.RequestStream.WriteAsync(sensorData);
    await Task.Delay(100); // センサー読み取り間隔のシミュレーション
}

// ストリームを完了
await stream.RequestStream.CompleteAsync();

// サーバーからの分析結果を受信
var result = await stream.ResponseAsync;
Console.WriteLine($"平均: {result.Average}, 最大: {result.Max}, 最小: {result.Min}, 件数: {result.Count}");
```

### 使用例

ClientStreaming は以下のようなシナリオで有用です：

- ファイルアップロード（チャンク単位）
- バッチデータの送信
- センサーデータの収集
- ログの一括送信

## DuplexStreaming の使い方

DuplexStreaming は双方向のストリーミングパターンで、クライアントとサーバーが同時に複数のメッセージを送受信できます。これは MagicOnion の StreamingHub の基礎となる技術です。

### サーバー側の実装

DuplexStreaming を実装するには、`GetDuplexStreamingContext<TRequest, TResponse>()` を使用してストリーミングコンテキストを取得します。

```csharp
public async Task<DuplexStreamingResult<ChatMessage, ChatMessage>> ChatAsync()
{
    var stream = GetDuplexStreamingContext<ChatMessage, ChatMessage>();

    // クライアントからのメッセージを受信するタスク
    var receiveTask = Task.Run(async () =>
    {
        await foreach (var message in stream.ReadAllAsync())
        {
            Logger.Debug($"Received: {message.User}: {message.Content}");
            
            // エコーバック（受信したメッセージにサーバー応答を付けて返す）
            var response = new ChatMessage
            {
                User = "Server",
                Content = $"Echo: {message.Content}",
                Timestamp = DateTime.UtcNow
            };
            
            await stream.WriteAsync(response);
        }
    });

    // ウェルカムメッセージを送信
    await stream.WriteAsync(new ChatMessage
    {
        User = "Server",
        Content = "チャットへようこそ！",
        Timestamp = DateTime.UtcNow
    });

    await receiveTask;

    return stream.Result();
}
```

### クライアント側の実装

クライアント側では、送信と受信を並行して処理します。

```csharp
var client = MagicOnionClient.Create<IChatService>(channel);
var stream = await client.ChatAsync();

// サーバーからのメッセージを受信するタスク
var receiveTask = Task.Run(async () =>
{
    await foreach (var message in stream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[{message.Timestamp}] {message.User}: {message.Content}");
    }
});

// ユーザー入力を送信
while (true)
{
    var input = Console.ReadLine();
    if (input == "exit") break;

    var message = new ChatMessage
    {
        User = "Client",
        Content = input,
        Timestamp = DateTime.UtcNow
    };

    await stream.RequestStream.WriteAsync(message);
}

// ストリームを完了
await stream.RequestStream.CompleteAsync();
await receiveTask;
```

### 使用例

DuplexStreaming は以下のようなシナリオで有用です：

- リアルタイムチャット
- ゲームの双方向通信
- コラボレーションツール
- リアルタイム監視システム

### 注意事項

1. **StreamingHub の検討**: DuplexStreaming が必要な場合、多くのケースで StreamingHub の方が適しています。StreamingHub は DuplexStreaming の上に構築された、より高レベルな API を提供します。

2. **エラーハンドリング**: ストリーミング中の例外は適切に処理する必要があります。接続の切断やタイムアウトに対する対策を実装してください。

3. **リソース管理**: 長時間実行されるストリーミング接続は、適切にリソースを管理し、必要に応じてタイムアウトを設定してください。

4. **並行処理**: DuplexStreaming では送信と受信が並行して行われるため、スレッドセーフティに注意してください。

## サンプルコード

### サーバーのサンプル

```csharp
// 定義
public interface IMyFirstService : IService<IMyFirstService>
{
    UnaryResult<string> SumAsync(int x, int y);
    Task<UnaryResult<string>> SumLegacyTaskAsync(int x, int y);
    Task<ClientStreamingResult<int, string>> ClientStreamingSampleAsync();
    Task<ServerStreamingResult<string>> ServerStreamingSampleAsync(int x, int y, int z);
    Task<DuplexStreamingResult<int, string>> DuplexStreamingSampleAsync();
}

// サーバー
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    public async UnaryResult<string> SumAsync(int x, int y)
    {
        Logger.Debug($"Called SumAsync - x:{x} y:{y}");

        return (x + y).ToString();
    }

    public async Task<ClientStreamingResult<int, string>> ClientStreamingSampleAsync()
    {
        Logger.Debug($"Called ClientStreamingSampleAsync");

        // ClientStreaming の場合は、GetClientStreamingContext を使用します。
        var stream = GetClientStreamingContext<int, string>();

        // クライアントから非同期で受信
        await foreach (var x in stream.ReadAllAsync())
        {
            Logger.Debug("Client Stream Received:" + x);
        }

        // StreamingContext.Result() で結果値を返します。
        return stream.Result("finished");
    }

    public async Task<ServerStreamingResult<string>> ServerStreamingSampleAsync(int x, int y, int z)
    {
        Logger.Debug($"Called ServerStreamingSampleAsync - x:{x} y:{y} z:{z}");

        var stream = GetServerStreamingContext<string>();

        var acc = 0;
        for (int i = 0; i < z; i++)
        {
            acc = acc + x + y;
            await stream.WriteAsync(acc.ToString());
        }

        return stream.Result();
    }

    public async Task<DuplexStreamingResult<int, string>> DuplexStreamingSampleAsync()
    {
        Logger.Debug($"Called DuplexStreamingSampleAsync");

        // DuplexStreamingContext はサーバーとクライアントの両方のストリーミングを表します。
        var stream = GetDuplexStreamingContext<int, string>();

        var waitTask = Task.Run(async () =>
        {
            // ForEachAsync(MoveNext, Current) でクライアントストリーミングを受信できます。
            await foreach (var x in stream.ReadAllAsync())
            {
                Logger.Debug($"Duplex Streaming Received:" + x);
            }
        });

        // WriteAsync は ServerStreaming です。
        await stream.WriteAsync("test1");
        await stream.WriteAsync("test2");
        await stream.WriteAsync("finish");

        await waitTask;

        return stream.Result();
    }
}
```

### クライアントのサンプル

```csharp
static async Task ClientStreamRun(IMyFirstService client)
{
    var stream = await client.ClientStreamingSampleAsync();

    for (int i = 0; i < 3; i++)
    {
        await stream.RequestStream.WriteAsync(i);
    }
    await stream.RequestStream.CompleteAsync();

    var response = await stream.ResponseAsync;

    Console.WriteLine("Response:" + response);
}

static async Task ServerStreamRun(IMyFirstService client)
{
    var stream = await client.ServerStreamingSampleAsync(10, 20, 3);

    await foreach (var x in stream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine("ServerStream Response:" + x);
    }
}

static async Task DuplexStreamRun(IMyFirstService client)
{
    var stream = await client.DuplexStreamingSampleAsync();

    var count = 0;
    await foreach (var x in stream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine("DuplexStream Response:" + x);

        await stream.RequestStream.WriteAsync(count++);
        if (x == "finish")
        {
            await stream.RequestStream.CompleteAsync();
        }
    }
}
```
