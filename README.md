MagicOnion
===
[![Releases](https://img.shields.io/github/release/neuecc/MagicOnion.svg)](https://github.com/neuecc/MagicOnion/releases)

gRPC based HTTP/2 RPC Streaming Framework for .NET, .NET Core and Unity.

Work in progress, stay tuned.

Quick Start
---
alpha-version is available in NuGet(Currently only work on .NET 4.5, .NET Core is not yet, Unity supports see [Unity Supports](https://github.com/neuecc/MagicOnion#unity-supports) section, HttpGateway + Swagger Intergarion supports see [Swagger](https://github.com/neuecc/MagicOnion#swagger) section)

* Install-Package MagicOnion -Pre

Let's implements Server, Server has two parts, interface and implementation.

```csharp
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using System;

// define interface as Server/Client IDL.
// implements T : IService<T>.
public interface IMyFirstService : IService<IMyFirstService>
{
    UnaryResult<int> SumAsync(int x, int y);
}

// implement RPC service.
// inehrit ServiceBase<interface>, interface
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    public UnaryResult<int> SumAsync(int x, int y)
    {
        Logger.Debug($"Received:{x}, {y}");

        return UnaryResult(x + y);
    }
}
```

and, launch the server.

```csharp
class Program
{
    static void Main(string[] args)
    {
        GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

        var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

        var server = new global::Grpc.Core.Server
        {
            Services = { service },
            Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
        };
        
        // launch gRPC Server.
        server.Start();

        // sample, launch server/client in same app.
        Task.Run(() =>
        {
            ClientImpl();
        });

        Console.ReadLine();
    }

    // Blank, used by next section 
    static async void ClientImpl()
    {
    }
}
```

write the client.

```csharp
static async void ClientImpl()
{
    // standard gRPC channel
    var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

    // create MagicOnion dynamic client proxy
    var client = MagicOnionClient.Create<IMyFirstService>(channel);

    // call method.
    var result = await client.SumAsync(100, 200);
    Console.WriteLine("Client Received:" + result);
}
```

MagicOnion allows primitive, multiple request value. Complex type is serialized by LZ4 Compressed MsgPack by [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) so type should follow MessagePack for C# rules. 

Project Structure
---
If creates Server-Client project, I recommend make three projects. `Server`, `ServerDefinition`, `Client`.

![image](https://cloud.githubusercontent.com/assets/46207/21081857/e0f6dfce-c012-11e6-850d-358c5b928a82.png)

ServerDefinition is only defined interface(`IService<T>`)(and some share request/response types).

If debugging, I recommend use [SwitchStartupProject](https://marketplace.visualstudio.com/items?itemName=vs-publisher-141975.SwitchStartupProject) and launch both Server and Client.

```json
"MultiProjectConfigurations": {
    "Server + Client": {
        "Projects": {
            "FooService": {},
            "FooClient": {}
        }
    }
}
```

It can step-in/out seamlessly in server and client.

Streaming
---


```csharp
// Definitions
public interface IMyFirstService : IService<IMyFirstService>
{
    UnaryResult<string> SumAsync(int x, int y);
    Task<UnaryResult<string>> SumLegacyTaskAsync(int x, int y);
    Task<ClientStreamingResult<int, string>> ClientStreamingSampleAsync();
    Task<ServerStreamingResult<string>> ServertSreamingSampleAsync(int x, int y, int z);
    Task<DuplexStreamingResult<int, string>> DuplexStreamingSampleAync();
}

// Server
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    // VisualStudio 2017(C# 7.0) supports return `async UnaryResult` directly
    public async UnaryResult<string> SumAsync(int x, int y)
    {
        Logger.Debug($"Called SumAsync - x:{x} y:{y}");

        return (x + y).ToString();
    }

    // VS2015(C# 6.0), use Task
    public async Task<UnaryResult<string>> SumLegacyTaskAsync(int x, int y)
    {
        Logger.Debug($"Called SumAsync - x:{x} y:{y}");

        // use UnaryResult method.
        return UnaryResult((x + y).ToString());
    }

    public async Task<ClientStreamingResult<int, string>> ClientStreamingSampleAsync()
    {
        Logger.Debug($"Called ClientStreamingSampleAsync");

        // If ClientStreaming, use GetClientStreamingContext.
        var stream = GetClientStreamingContext<int, string>();

        // receive from client asynchronously
        await stream.ForEachAsync(x =>
        {
            Logger.Debug("Client Stream Received:" + x);
        });

        // StreamingContext.Result() for result value.
        return stream.Result("finished");
    }

    public async Task<ServerStreamingResult<string>> ServertSreamingSampleAsync(int x, int y, int z)
    {
        Logger.Debug($"Called ServertSreamingSampleAsync - x:{x} y:{y} z:{z}");

        var stream = GetServerStreamingContext<string>();

        var acc = 0;
        for (int i = 0; i < z; i++)
        {
            acc = acc + x + y;
            await stream.WriteAsync(acc.ToString());
        }

        return stream.Result();
    }

    public async Task<DuplexStreamingResult<int, string>> DuplexStreamingSampleAync()
    {
        Logger.Debug($"Called DuplexStreamingSampleAync");

        // DuplexStreamingContext represents both server and client streaming.
        var stream = GetDuplexStreamingContext<int, string>();

        var waitTask = Task.Run(async () =>
        {
            // ForEachAsync(MoveNext, Current) can receive client streaming.
            await stream.ForEachAsync(x =>
            {
                Logger.Debug($"Duplex Streaming Received:" + x);
            });
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

Client sample.

```csharp
static async Task UnaryRun(IMyFirstService client)
{
    // await
    var vvvvv = await client.SumAsync(10, 20);
    Console.WriteLine("SumAsync:" + vvvvv);
    
    // if use Task<UnaryResult>, use await await
    var vvvv2 = await await client.SumLegacyTaskAsync(10, 20);
}

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
    var stream = await client.ServertSreamingSampleAsync(10, 20, 3);

    await stream.ResponseStream.ForEachAsync(x =>
    {
        Console.WriteLine("ServerStream Response:" + x);
    });
}

static async Task DuplexStreamRun(IMyFirstService client)
{
    var stream = await client.DuplexStreamingSampleAync();

    var count = 0;
    await stream.ResponseStream.ForEachAsync(async x =>
    {
        Console.WriteLine("DuplexStream Response:" + x);

        await stream.RequestStream.WriteAsync(count++);
        if (x == "finish")
        {
            await stream.RequestStream.CompleteAsync();
        }
    });
}
```

Swagger
---
MagicOnion has built-in Http1 JSON Gateway and [Swagger](http://swagger.io/) integration for Unary operation.

* Install-Package MagicOnion.HttpGateway -Pre

HttpGateway is built on ASP.NET Core. for example, with `Microsoft.AspNetCore.Server.WebListener`.

```csharp
static void Main(string[] args)
{
    // gRPC definition.
    GrpcEnvironment.SetLogger(new ConsoleLogger());
    var service = MagicOnionEngine.BuildServerServiceDefinition(new MagicOnionOptions(true)
    {
        MagicOnionLogger = new MagicOnionLogToGrpcLogger()
    });
    var server = new global::Grpc.Core.Server
    {
        Services = { service },
        Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
    };
    server.Start();

    // ASP.NET Core definition.
    var webHost = new WebHostBuilder()
        .ConfigureServices(collection =>
        {
            // Add MagicOnionServiceDefinition for reference from Startup.
            collection.Add(new ServiceDescriptor(typeof(MagicOnionServiceDefinition), service));
        })
        .UseWebListener()
        .UseStartup<Startup>()
        .UseUrls("http://localhost:5432")
        .Build();

    webHost.Run();
}
```

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        // Take from builder.
        var magicOnion = app.ApplicationServices.GetService<MagicOnionServiceDefinition>();

        // Optional:Summary to Swagger
        // var xmlName = "Sandbox.ConsoleServerDefinition.xml";
        // var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

        // HttpGateway has two middlewares.
        // One is SwaggerView(MagicOnionSwaggerMiddleware)
        // One is Http1-JSON to gRPC-MagicOnion gateway(MagicOnionHttpGateway)
        app.UseMagicOnionSwagger(magicOnion.MethodHandlers, new SwaggerOptions("MagicOnion.Server", "Swagger Integration Test", "/")
        {
            // XmlDocumentPath = xmlPath
        });
        app.UseMagicOnionHttpGateway(magicOnion.MethodHandlers, new Channel("localhost:12345", ChannelCredentials.Insecure));
    }
}
```

Open `http://localhost:5432`, you can see swagger view.

![image](https://cloud.githubusercontent.com/assets/46207/21295663/6a9d3e28-c59d-11e6-8081-18d14e359567.png)

Filter
---
Filter example.

```
public class SampleFilterAttribute : MagicOnionFilterAttribute
{
    // constructor convention rule. requires Func<ServiceContext, Task> next.
    public SampleFilterAttribute(Func<ServiceContext, Task> next) : base(next) { }

    // other constructor, use base(null)
    public SampleFilterAttribute() : base(null) { }

    public override async Task Invoke(ServiceContext context)
    {
        try
        {
            /* on before */
            await Next(context); // next
            /* on after */
        }
        catch
        {
            /* on exception */
            throw;
        }
        finally
        {
            /* on finally */
        }
    }
}
```

Unity Supports
---
Code is located in `src/MagicOnion.Client.Unity/Assets/Scripts/gRPC` (port of Grpc.Core) and `src/MagicOnion.Client.Unity/Assets/Scripts/MagicOnion` (MagicOnion Runtime). There are require [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) and [UniRx](https://github.com/neuecc/UniRx).

MagicOnion's Unity client works on all platforms(PC, Android, iOS, etc...). But it can 'not' use dynamic client generation due to IL2CPP issue. But pre code generate helps it. `moc.exe`is using Roslyn so analyze source code, pass the target csproj.

```
moc arguments help:
  -i, --input=VALUE          [required]Input path of analyze csproj
  -o, --output=VALUE         [required]Output path(file) or directory base(in separated mode)
  -u, --unuseunityattr       [optional, default=false]Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer
  -c, --conditionalsymbol=VALUE [optional, default=empty]conditional compiler symbol
  -n, --namespace=VALUE      [optional, default=MagicOnion]Set namespace root name
  -a, asyncsuffix      [optional, default=false]Use methodName to async suffix
```

moc.exe is located in `packages\MagicOnion.*.*.*\tools\moc.exe`.
