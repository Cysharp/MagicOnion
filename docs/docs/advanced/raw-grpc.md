# Raw gRPC APIs

MagicOnion can define and use primitive gRPC APIs (ClientStreaming, ServerStreaming, DuplexStreaming). Especially DuplexStreaming is used underlying StreamingHub. If there is no reason, we recommend using StreamingHub.

## ServerStreaming

ServerStreaming is a streaming pattern where the server sends multiple values to the client. The client sends a single request, and the server can return multiple responses.

### Server-side implementation

To implement ServerStreaming, use `GetServerStreamingContext<T>()` to get the streaming context.

```csharp
public async Task<ServerStreamingResult<WeatherData>> GetWeatherUpdatesAsync(string location, int count)
{
    var stream = GetServerStreamingContext<WeatherData>();

    // Send weather data for the specified count
    for (int i = 0; i < count; i++)
    {
        var weatherData = new WeatherData
        {
            Temperature = Random.Shared.Next(-10, 35),
            Humidity = Random.Shared.Next(30, 90),
            Timestamp = DateTime.UtcNow
        };

        await stream.WriteAsync(weatherData);
        
        // Wait for 1 second (simulating real-time data)
        await Task.Delay(1000);
    }

    return stream.Result();
}
```

### Client-side implementation

On the client side, use `ResponseStream.ReadAllAsync()` to receive all values sent from the server.

```csharp
var client = MagicOnionClient.Create<IWeatherService>(channel);
var stream = await client.GetWeatherUpdatesAsync("Tokyo", 5);

await foreach (var weatherData in stream.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"Temperature: {weatherData.Temperature}°C, Humidity: {weatherData.Humidity}%, Time: {weatherData.Timestamp}");
}
```

### Use cases

ServerStreaming is useful in scenarios such as:

- Real-time data feeds (stock prices, sensor data, etc.)
- Sending large amounts of data in chunks
- Progress update notifications
- Log streaming

## ClientStreaming

ClientStreaming is a streaming pattern where the client sends multiple values to the server. The client sends multiple messages, and the server returns a single response.

### Server-side implementation

To implement ClientStreaming, use `GetClientStreamingContext<TRequest, TResponse>()` to get the streaming context.

```csharp
public async Task<ClientStreamingResult<SensorData, AnalysisResult>> AnalyzeSensorDataAsync()
{
    var stream = GetClientStreamingContext<SensorData, AnalysisResult>();

    var allData = new List<SensorData>();
    
    // Receive all data from the client
    await foreach (var data in stream.ReadAllAsync())
    {
        Logger.Debug($"Received sensor data: {data.Value} at {data.Timestamp}");
        allData.Add(data);
    }

    // Analyze the received data
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

### Client-side implementation

On the client side, use `RequestStream.WriteAsync()` to send multiple values and call `CompleteAsync()` at the end to complete the stream.

```csharp
var client = MagicOnionClient.Create<ISensorService>(channel);
var stream = await client.AnalyzeSensorDataAsync();

// Send sensor data
for (int i = 0; i < 10; i++)
{
    var sensorData = new SensorData
    {
        Value = Random.Shared.NextDouble() * 100,
        Timestamp = DateTime.UtcNow
    };
    
    await stream.RequestStream.WriteAsync(sensorData);
    await Task.Delay(100); // Simulate sensor reading interval
}

// Complete the stream
await stream.RequestStream.CompleteAsync();

// Receive analysis result from server
var result = await stream.ResponseAsync;
Console.WriteLine($"Average: {result.Average}, Max: {result.Max}, Min: {result.Min}, Count: {result.Count}");
```

### Use cases

ClientStreaming is useful in scenarios such as:

- File uploads (in chunks)
- Batch data submission
- Sensor data collection
- Bulk log submission

## DuplexStreaming

DuplexStreaming is a bidirectional streaming pattern where both client and server can send and receive multiple messages simultaneously. This is the underlying technology for MagicOnion's StreamingHub.

### Server-side implementation

To implement DuplexStreaming, use `GetDuplexStreamingContext<TRequest, TResponse>()` to get the streaming context.

```csharp
public async Task<DuplexStreamingResult<ChatMessage, ChatMessage>> ChatAsync()
{
    var stream = GetDuplexStreamingContext<ChatMessage, ChatMessage>();

    // Task to receive messages from the client
    var receiveTask = Task.Run(async () =>
    {
        await foreach (var message in stream.ReadAllAsync())
        {
            Logger.Debug($"Received: {message.User}: {message.Content}");
            
            // Echo back (return received message with server response)
            var response = new ChatMessage
            {
                User = "Server",
                Content = $"Echo: {message.Content}",
                Timestamp = DateTime.UtcNow
            };
            
            await stream.WriteAsync(response);
        }
    });

    // Send welcome message
    await stream.WriteAsync(new ChatMessage
    {
        User = "Server",
        Content = "Welcome to the chat!",
        Timestamp = DateTime.UtcNow
    });

    await receiveTask;

    return stream.Result();
}
```

### Client-side implementation

On the client side, handle sending and receiving in parallel.

```csharp
var client = MagicOnionClient.Create<IChatService>(channel);
var stream = await client.ChatAsync();

// Task to receive messages from the server
var receiveTask = Task.Run(async () =>
{
    await foreach (var message in stream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[{message.Timestamp}] {message.User}: {message.Content}");
    }
});

// Send user input
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

// Complete the stream
await stream.RequestStream.CompleteAsync();
await receiveTask;
```

### Use cases

DuplexStreaming is useful in scenarios such as:

- Real-time chat
- Bidirectional game communication
- Collaboration tools
- Real-time monitoring systems

### Notes

1. **Consider StreamingHub**: When you need DuplexStreaming, StreamingHub is often more suitable in many cases. StreamingHub provides a higher-level API built on top of DuplexStreaming.

2. **Error handling**: Exceptions during streaming need to be handled properly. Implement measures for connection drops and timeouts.

3. **Resource management**: Long-running streaming connections should manage resources properly and set timeouts as needed.

4. **Concurrent processing**: In DuplexStreaming, sending and receiving happen concurrently, so pay attention to thread safety.

## Sample Code

### Server Sample

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

### Client sample

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