# Raw gRPC APIs

MagicOnion은 기본적인 gRPC APIs(ClientStreaming, ServerStreaming, DuplexStreaming)를 정의하고 사용할 수 있습니다. 특히 DuplexStreaming은 StreamingHub의 기반으로 사용됩니다. 특별한 이유가 없다면 StreamingHub를 사용하는 것을 권장합니다.

## ServerStreaming

ServerStreaming은 서버에서 클라이언트로 여러 값을 전송하는 스트리밍 패턴입니다. 클라이언트는 단일 요청을 보내고, 서버는 여러 응답을 반환할 수 있습니다.

### 서버 측 구현

ServerStreaming을 구현하려면 `GetServerStreamingContext<T>()`를 사용하여 스트리밍 컨텍스트를 가져옵니다.

```csharp
public async Task<ServerStreamingResult<WeatherData>> GetWeatherUpdatesAsync(string location, int count)
{
    var stream = GetServerStreamingContext<WeatherData>();

    // 지정된 횟수만큼 날씨 데이터 전송
    for (int i = 0; i < count; i++)
    {
        var weatherData = new WeatherData
        {
            Temperature = Random.Shared.Next(-10, 35),
            Humidity = Random.Shared.Next(30, 90),
            Timestamp = DateTime.UtcNow
        };

        await stream.WriteAsync(weatherData);
        
        // 1초 대기 (실시간 데이터 시뮬레이션)
        await Task.Delay(1000);
    }

    return stream.Result();
}
```

### 클라이언트 측 구현

클라이언트 측에서는 `ResponseStream.ReadAllAsync()`를 사용하여 서버에서 전송되는 모든 값을 수신합니다.

```csharp
var client = MagicOnionClient.Create<IWeatherService>(channel);
var stream = await client.GetWeatherUpdatesAsync("Tokyo", 5);

await foreach (var weatherData in stream.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"온도: {weatherData.Temperature}°C, 습도: {weatherData.Humidity}%, 시간: {weatherData.Timestamp}");
}
```

### 사용 사례

ServerStreaming은 다음과 같은 시나리오에서 유용합니다:

- 실시간 데이터 피드 (주식 가격, 센서 데이터 등)
- 대량 데이터의 청크 단위 전송
- 진행 상황 업데이트 알림
- 로그 스트리밍

## ClientStreaming

ClientStreaming은 클라이언트에서 서버로 여러 값을 전송하는 스트리밍 패턴입니다. 클라이언트는 여러 메시지를 보내고, 서버는 단일 응답을 반환합니다.

### 서버 측 구현

ClientStreaming을 구현하려면 `GetClientStreamingContext<TRequest, TResponse>()`를 사용하여 스트리밍 컨텍스트를 가져옵니다.

```csharp
public async Task<ClientStreamingResult<SensorData, AnalysisResult>> AnalyzeSensorDataAsync()
{
    var stream = GetClientStreamingContext<SensorData, AnalysisResult>();

    var allData = new List<SensorData>();
    
    // 클라이언트로부터 모든 데이터 수신
    await foreach (var data in stream.ReadAllAsync())
    {
        Logger.Debug($"센서 데이터 수신: {data.Value} at {data.Timestamp}");
        allData.Add(data);
    }

    // 수신된 데이터 분석
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

### 클라이언트 측 구현

클라이언트 측에서는 `RequestStream.WriteAsync()`를 사용하여 여러 값을 전송하고, 마지막에 `CompleteAsync()`를 호출하여 스트림을 완료합니다.

```csharp
var client = MagicOnionClient.Create<ISensorService>(channel);
var stream = await client.AnalyzeSensorDataAsync();

// 센서 데이터 전송
for (int i = 0; i < 10; i++)
{
    var sensorData = new SensorData
    {
        Value = Random.Shared.NextDouble() * 100,
        Timestamp = DateTime.UtcNow
    };
    
    await stream.RequestStream.WriteAsync(sensorData);
    await Task.Delay(100); // 센서 읽기 간격 시뮬레이션
}

// 스트림 완료
await stream.RequestStream.CompleteAsync();

// 서버로부터 분석 결과 수신
var result = await stream.ResponseAsync;
Console.WriteLine($"평균: {result.Average}, 최대: {result.Max}, 최소: {result.Min}, 개수: {result.Count}");
```

### 사용 사례

ClientStreaming은 다음과 같은 시나리오에서 유용합니다:

- 파일 업로드 (청크 단위)
- 배치 데이터 제출
- 센서 데이터 수집
- 대량 로그 제출

## DuplexStreaming

DuplexStreaming은 클라이언트와 서버가 동시에 여러 메시지를 주고받을 수 있는 양방향 스트리밍 패턴입니다. 이는 MagicOnion의 StreamingHub를 위한 기반 기술입니다.

### 서버 측 구현

DuplexStreaming을 구현하려면 `GetDuplexStreamingContext<TRequest, TResponse>()`를 사용하여 스트리밍 컨텍스트를 가져옵니다.

```csharp
public async Task<DuplexStreamingResult<ChatMessage, ChatMessage>> ChatAsync()
{
    var stream = GetDuplexStreamingContext<ChatMessage, ChatMessage>();

    // 클라이언트로부터 메시지를 수신하는 태스크
    var receiveTask = Task.Run(async () =>
    {
        await foreach (var message in stream.ReadAllAsync())
        {
            Logger.Debug($"수신: {message.User}: {message.Content}");
            
            // 에코백 (수신한 메시지에 서버 응답을 추가하여 반환)
            var response = new ChatMessage
            {
                User = "Server",
                Content = $"Echo: {message.Content}",
                Timestamp = DateTime.UtcNow
            };
            
            await stream.WriteAsync(response);
        }
    });

    // 환영 메시지 전송
    await stream.WriteAsync(new ChatMessage
    {
        User = "Server",
        Content = "채팅에 오신 것을 환영합니다!",
        Timestamp = DateTime.UtcNow
    });

    await receiveTask;

    return stream.Result();
}
```

### 클라이언트 측 구현

클라이언트 측에서는 송신과 수신을 병렬로 처리합니다.

```csharp
var client = MagicOnionClient.Create<IChatService>(channel);
var stream = await client.ChatAsync();

// 서버로부터 메시지를 수신하는 태스크
var receiveTask = Task.Run(async () =>
{
    await foreach (var message in stream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[{message.Timestamp}] {message.User}: {message.Content}");
    }
});

// 사용자 입력 전송
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

// 스트림 완료
await stream.RequestStream.CompleteAsync();
await receiveTask;
```

### 사용 사례

DuplexStreaming은 다음과 같은 시나리오에서 유용합니다:

- 실시간 채팅
- 양방향 게임 통신
- 협업 도구
- 실시간 모니터링 시스템

### 주의사항

1. **StreamingHub 고려**: DuplexStreaming이 필요한 경우, 많은 경우에 StreamingHub가 더 적합합니다. StreamingHub는 DuplexStreaming 위에 구축된 더 높은 수준의 API를 제공합니다.

2. **오류 처리**: 스트리밍 중 예외는 적절히 처리되어야 합니다. 연결 끊김 및 타임아웃에 대한 대책을 구현하세요.

3. **리소스 관리**: 장시간 실행되는 스트리밍 연결은 리소스를 적절히 관리하고 필요에 따라 타임아웃을 설정해야 합니다.

4. **동시 처리**: DuplexStreaming에서는 송신과 수신이 동시에 발생하므로 스레드 안전성에 주의하세요.

## 샘플 코드

### 서버 샘플

```csharp
// Definitions
public interface IMyFirstService : IService<IMyFirstService>
{
    UnaryResult<string> SumAsync(int x, int y);
    Task<UnaryResult<string>> SumLegacyTaskAsync(int x, int y);
    Task<ClientStreamingResult<int, string>> ClientStreamingSampleAsync();
    Task<ServerStreamingResult<string>> ServerStreamingSampleAsync(int x, int y, int z);
    Task<DuplexStreamingResult<int, string>> DuplexStreamingSampleAsync();
}

// Server
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

        // If ClientStreaming, use GetClientStreamingContext.
        var stream = GetClientStreamingContext<int, string>();

        // receive from client asynchronously
        await foreach (var x in stream.ReadAllAsync())
        {
            Logger.Debug("Client Stream Received:" + x);
        }

        // StreamingContext.Result() for result value.
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

        // DuplexStreamingContext represents both server and client streaming.
        var stream = GetDuplexStreamingContext<int, string>();

        var waitTask = Task.Run(async () =>
        {
            // ForEachAsync(MoveNext, Current) can receive client streaming.
            await foreach (var x in stream.ReadAllAsync())
            {
                Logger.Debug($"Duplex Streaming Received:" + x);
            }
        });

        // WriteAsync is ServerStreaming.
        await stream.WriteAsync("test1");
        await stream.WriteAsync("test2");
        await stream.WriteAsync("finish");

        await waitTask;

        return stream.Result();
    }
}
```

### 클라이언트 샘플

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