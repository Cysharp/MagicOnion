# Raw gRPC APIs
MagicOnion은 기본적인 gRPC APIs(ClientStreaming, ServerStreaming, DuplexStreaming)를 정의하고 사용할 수 있습니다. 특히 DuplexStreaming은 StreamingHub의 기반으로 사용됩니다. 특별한 이유가 없다면 StreamingHub를 사용하는 것을 권장합니다.

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

클라이언트 샘플

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
