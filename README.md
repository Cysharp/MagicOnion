MagicOnion
===
[![Releases](https://img.shields.io/github/release/neuecc/MagicOnion.svg)](https://github.com/neuecc/MagicOnion/releases)

Unified Realtime/API Engine for .NET Core and Unity.

What is it?
---
MagicOnion is an Realtime Network Engine like [SignalR](https://github.com/aspnet/AspNetCore/tree/master/src/SignalR), [Socket.io](https://socket.io/) and RPC-Web API Framework like any web-framework.

MagicOnion is built on [gRPC](https://grpc.io/) so fast(HTTP/2) and compact(binary) network transport. It does not requires `.proto` and generate unlike plain gRPC. Protocol schema can share a C# interface and classes.

![image](https://user-images.githubusercontent.com/46207/50965239-c4fdb000-1514-11e9-8365-304c776ffd77.png)

> Share interface as schema and request as API Service seems like normal C# code

![image](https://user-images.githubusercontent.com/46207/50965825-7bae6000-1516-11e9-9501-dc91582f4d1b.png)

> StreamingHub realtime service, broadcast data to many connected clients

MagicOnion is for Microservices(communicate between .NET Core Servers), API Service(for WinForms/WPF like WCF), Native Client's API(for Xamarin, Unity) and Realtime Server that replacement like Socket.io, SignalR, Photon, UNet, etc.

Quick Start
---
for .NET 4.6, 4.7 and .NET Standard 2.0(.NET Core) available in NuGet. Unity supports see [Unity Supports](https://github.com/cysharp/MagicOnion#unity-supports) section. HttpGateway + Swagger Intergarion supports see [Swagger](https://github.com/cysharp/MagicOnion#swagger) section.

```
Install-Package MagicOnion
```
MagicOnion has two sides, `Service` for like web-api and `StreamingHub` for relatime communication. At first, see define `Service`.

```csharp
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using System;

// define interface as Server/Client IDL.
// implements T : IService<T> and share this type between server and client.
public interface IMyFirstService : IService<IMyFirstService>
{
    // Return type must be `UnaryResult<T>` or `Task<UnaryResult<T>>`.
    // If you can use C# 7.0 or newer, recommend to use `UnaryResult<T>`.
    UnaryResult<int> SumAsync(int x, int y);
}

// implement RPC service to Server Project.
// inehrit ServiceBase<interface>, interface
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    // You can use async syntax directly.
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        Logger.Debug($"Received:{x}, {y}");

        return x + y;
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

        // setup MagicOnion and option.
        var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

        var server = new global::Grpc.Core.Server
        {
            Services = { service },
            Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
        };
        
        // launch gRPC Server.
        server.Start();

        // and wait.
        Console.ReadLine();
    }
}
```

write the client.

```csharp
// standard gRPC channel
var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

// get MagicOnion dynamic client proxy
var client = MagicOnionClient.Create<IMyFirstService>(channel);

// call method.
var result = await client.SumAsync(100, 200);
Console.WriteLine("Client Received:" + result);
```

MagicOnion allows primitive, multiple request value. Complex type is serialized by LZ4 Compressed MsgPack by [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) so type should follow MessagePack for C# rules. 

StreamingHub
---
StreamingHub is a fully-typed realtime server<->client communication framework.

```csharp
// Server -> Client definition
public interface IGamingHubReceiver
{
    // return type shuold be `void` or `Task`, parameters are free.
    void OnJoin(Player player);
    void OnLeave(Player player);
    void OnMove(Player player);
}
 
// Client -> Server definition
// implements `IStreamingHub<TSelf, TReceiver>`  and share this type between server and client.
public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
{
    // return type shuold be `Task` or `Task<T>`, parameters are free.
    Task<Player[]> JoinAsync(string roomName, string userName, Vector3 position, Quaternion rotation);
    Task LeaveAsync();
    Task MoveAsync(Vector3 position, Quaternion rotation);
}
 
// for example, request object by MessagePack.
[MessagePackObject]
public class Player
{
    [Key(0)]
    public string Name { get; set; }
    [Key(1)]
    public Vector3 Position { get; set; }
    [Key(2)]
    public Quaternion Rotation { get; set; }
}
```

```csharp
// Server implementation
// implements : StreamingHubBase<THub, TReceiver>, THub
public class GamingHub : StreamingHubBase<IGamingHub, IGamingHubReceiver>, IGamingHub
{
    // this class is instantiated per connected so fields are cache area of connection.
    IGroup room;
    Player self;
    IInMemoryStorage<Player> storage;

    public async Task<Player[]> JoinAsync(string roomName, string userName, Vector3 position, Quaternion rotation)
    {
        self = new Player() { Name = userName, Position = position, Rotation = rotation };

        // Group can bundle many connections and it has inmemory-storage so add any type per group. 
        (room, storage) = await Group.AddAsync(roomName, self);

        // Typed Server->Client broadcast.
        Broadcast(room).OnJoin(self);

        return storage.AllValues.ToArray();
    }

    public async Task LeaveAsync()
    {
        await room.RemoveAsync(this.Context);
        Broadcast(room).OnLeave(self);
    }

    public async Task MoveAsync(Vector3 position, Quaternion rotation)
    {
        self.Position = position;
        self.Rotation = rotation;
        Broadcast(room).OnMove(self);
    }

    // You can hook OnConnecting/OnDisconnected by override.
    protected override async ValueTask OnDisconnected()
    {
        // on disconnecting, if automatically removed this connection from group.
        return CompletedTask;
    }
}
```

Filter
---
MagicOnion filter is powerful feature to hook before-after invoke. It is useful than gRPC server interceptor.

![image](https://user-images.githubusercontent.com/46207/50969421-cb465900-1521-11e9-8824-8a34cc52bbe4.png)

```csharp
// You can attach per class/method like [SampleFilter]
// for StreamingHub methods, implement StreamingHubFilterAttribute instead.
public class SampleFilterAttribute : MagicOnionFilterAttribute
{
    // constructor convention rule. requires Func<ServiceContext, Task> next.
    public SampleFilterAttribute(Func<ServiceContext, Task> next) : base(next) { }

    // other constructor, use base(null)
    public SampleFilterAttribute() : base(null) { }

    public override async ValueTask Invoke(ServiceContext context)
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

Here is example of what kind of filter can be stacked.

![image](https://user-images.githubusercontent.com/46207/50969539-2bd59600-1522-11e9-84ab-15dd85e3dcac.png)

GlobalFilter can attach to MagicOnionOptions.

ServiceContext and Lifecycle
---
Service/StreamingHub's method or `MagicOnionFilter` can access `this.Context` it is 

| Property | Description |
| --- | --- |
| `ConcurrentDictionary<string, object>` Items | Object storage per request/connection. |
| `Guid` ContextId | Unieuq ID per request(Service)/connection(StreamingHub). |
| `DateTime` Timestamp | Timestamp that request/connection is started time. |
| `Type` ServiceType | Invoked Class. |
| `MethodInfo` MethodInfo | Invoked Method. |
| `ILookup<Type, Attribute> AttributeLookup | Cached Attributes that merged both service and method. |
| `ServerCallContext` CallContext | Raw gRPC Context. |
| `IFormatterResolver` FormatterResolver | Using MessagePack resolver. |

`Items` is useful, for example authentication filter add UserId to Items and take out from service method.

> If using StreamingHub, ServiceContext means per connected context so `Items` is not per method invoke. `StreamingHubContext.Items` supports per streaming hub method request but currently can not take from streaming hub method(only use in StreamingHubFilter). [Issue:#67](https://github.com/Cysharp/MagicOnion/issues/67), it will fix.

MagicOnion supports get current context globaly like HttpContext.Current. `ServiceContext.Current` can get it but it requires `MagicOnionOptions.EnableCurrentContext = true`, default is false.

Lifecycle image of ServiceBase

```
gRPC In(
    var context = new ServiceContext();
    Filter.Invoke(context, 
        var service = new ServiceImpl();
        service.ServiceContext = context;
        service.MethodInvoke(
            /* method impl */
        )
    )
)
```

Lifecycle image of StreamingHub(StreamingHub is inherited from ServiceBase)

```
gRPC In(
    var context = new ServiceContext();
    Filter.Invoke(context, 
        var hub = new StreamingHubImpl();
        hub.ServiceContext = context;
        hub.Connect(
            while (connecting) {
                Streaming In(
                    var streamingHubContext = new StreamingHubContext(context);
                    StreamingHubFilter.Invoke(streamingHubContext,
                        hub.MethodInvoke(
                            /* method impl */
                        )
                    )
                )
            }
        )
    )
)
```

StreamingHub instance is shared while connecting so StreamingHub's field can use cache area of connection.

MagicOnionOption/Loggin
---
`MagicOnionOption` can pass to `MagicOnionEngine.BuildServerServiceDefinition(MagicOnionOptions option)`.

| Property | Description |
| --- | --- |
| `IMagicOnionLogger` MagicOnionLogger | Set the diagnostics info logger. |
| `bool` DisableEmbeddedService | Disable embedded service(ex:heartbeat), default is false. |
| `MagicOnionFilterAttribute[]` GlobalFilters | Global MagicOnion filters. |
| `bool` EnableCurrentContext | Enable ServiceContext.Current option by AsyncLocal, default is false. |
| `StreamingHubFilterAttribute[]` Global StreamingHub filters. | GlobalStreamingHubFilters |
| `IGroupRepositoryFactory` DefaultGroupRepositoryFactory | Default GroupRepository factory for StreamingHub, default is ``. |
| `IServiceLocator` ServiceLocator | Add the extra typed option. |
| `bool` IsReturnExceptionStackTraceInErrorDetail | If true, MagicOnion handles exception ownself and send to message. If false, propagate to gRPC engine. Default is false. |
| `IFormatterResolver` FormatterResolver | MessagePack serialization resolver. Default is used ambient default(MessagePackSerialzier.Default). |

`IMagicOnionLogger` is structured logger of MagicOnion. Implements your custom logging code and append it, default is `NullMagicOnionLogger`(do nothing). MagicOnion has some built in logger, `MagicOnionLogToGrpcLogger` that structured log to string log and send to `GrpcEnvironment.Logger`. `MagicOnionLogToGrpcLoggerWithDataDump` is includes data dump it is useful for debugging(but slightly heavy, recommended to only use debugging). `MagicOnionLogToGrpcLoggerWithNamedDataDump` is more readable than simple WithDataDump logger.

If you want to add many loggers, you can use `CompositeLogger`(for gRPC logging), `CompositeMagicOnionLogger`(for MagicOnion structured logging) to composite many loggers.

ExceptionHandling and StatusCode
---
If you are return custom status code from server to client, you can use `throw new ReturnStatusException`.

```csharp
public Task SendMessageAsync(string message)
{
    if (message.Contains("foo"))
    {
        //
        throw new ReturnStatusException((Grpc.Core.StatusCode)99, "invalid");
    }

    // ....
```

Client can receive exception as gRPC's `RpcException`. If performance centric to avoid exception throw, you can use raw gRPC CallContext.Status(`ServiceContext.CallCaontext.Status`) and set status directly.

MagicOnion's engine catched exception(except ReturnStatusException), set `StatusCode.Unknown` and client received gRPC's `RpcException`. If `MagicOnionOption.IsReturnExceptionStackTraceInErrorDetail` is true, client can receive StackTrace of server exception, it is very useful for debugging but has critical issue about sercurity so should only to enable debug build.

Group and GroupConfiguration
---
StreamingHub's broadcast system is called Group. It can get from StreamingHub impl method, `this.Group`(this.Group type is `HubGroupRepository`, not `IGroup`).

Current connection can add to group by `this.Group.AddAsync(string groupName)`, return value(`IGroup`) is joined group broadcaster so cache to field. It is enable per connection(if disconnected, automaticaly leaved from group). If you want to use some restriction, you can use `TryAddAsync(string groupName, int incluciveLimitCount, bool createIfEmpty)`.

`IGroup` can pass to StreamingHub.`Broadcast`, `BroadcastExceptSelf`, `BroadcastExcept` and calls client proxy.

```csharp
public class ChatHub : StreamingHubBase<IChatHub, IMessageReceiver>, IChatHub
{
    string userName;
    IGroup room;

    public async Task JoinAsync(string userName, string roomName)
    {
        this.userName = userName;
        this.room = await Group.AddAsync(roomName);
    }

    public async Task SendMessageAsync(string message)
    {
        Broadcast(room).OnReceiveMessage(userName, message);
    }
}
```

> GroupRepository is created per StreamingHub type

> If you want to create ServerSide loop and broadcast out of StreamingHub, you can pass Broadcast(room) result but it is unnatural, I'll add support kit of create server-side loop  

Group has in-memory storage, it can store extra data to group member. It can set `Group.AddAsync(string groupName, TStorage data)` instead of standard AddAsync.

Data is can get from `group.GetInMemoryStorage<T>` and can invoke `AllValues`, `Set(Guid connectionId, T Value)`, `Get(Guid connectionId)`.

> StreamingHub's ConnectionId is ServiceContext.ContextId

Default MagicOnion's group is inmemory and using `ImmutableArrayGroup`. This group implementation is tuned for small room, not enter/leave frequently. If large room and enter/leave frequently design, you can use `ConcurrentDictionaryGroup`. It can configure by `GroupConfigurationAttribute` or `MagicOnionOptions.DefaultGroupRepositoryFactory`.

```csharp
// use ***GroupRepositoryFactory type.
[GroupConfiguration(typeof(ConcurrentDictionaryGroupRepositoryFactory))]
public class ChatHub : StreamingHubBase<IChatHub, IMessageReceiver>, IChatHub
{
    // ...
}
```

MagicOnion has distribute system called redis-backplane for group broadcast.

![image](https://user-images.githubusercontent.com/46207/50974777-5f6aed00-152f-11e9-97f3-ba2a0c97f0eb.png)

* Install-Package MagicOnion.Redis

```csharp
// set RedisGroupRepositoryFactory
[GroupConfiguration(typeof(RedisGroupRepositoryFactory))]
public class ...
{
}

// configure ConnectionMultiplexer(StackExchange.Redis) to MagicOnionOption.ServiceLocator
var option = new MagicOnionOption();
option.ServiceLocator.Register(new ConnectionMultiplexer(...));
```

Zero deserialization mapping
---
In RPC, especially in real-time communication involving frequent transmission of data, it is often the serialization process where data is converted before being sent that limits the performance. In MagicOnion, serialization is done by my MessagePack for C#, which is the fastest binary serializer for C#, so it cannot be a limiting factor. Also, in addition to performance, it also provides flexibility regarding data in that variables of any type can be sent as long as they can be serialized by MessagePack for C#.

Also, taking advantage of the fact that both the client and the server run on C# and data stored on internal memory are expected to share the same layout, I added an option to do mapping through memory copy without serialization/deserialization in case of a value-type variable.

Especialy in Unity, this is can combinate with `MessageaPack.UnityShims` package of NuGet.

```csharp
// It supports standard struct-type variables that are provided by Unity, such as Vector3, and arrays containing them, as well as custom struct-type variables and their arrays.
// I recommend doing this explicitly using [StructLayout(LayoutKind.Explicit)] to accurately match the size.
public struct CustomStruct
{
    public long Id;
    public int Hp;
    public int Mp;
    public byte Status;
}
 
// ---- Register the following code when initializing.
 
// By registering it, T and T[] can be handled using zero deserialization mapping.
UnsafeDirectBlitResolver.Register<CustomStruct>();
 
// The struct-type above as well as Unity-provided struct-types (Vector2, Rect, etc.), and their arrays are registered as standards.
CompositeResolver.RegisterAndSetAsDefault(
    UnsafeDirectBlitResolver.Instance,
    MessagePack.Unity.Extension.UnityBlitResolver.Instance
    );
 
// --- Now the communication will be in the format above when they are used for transmission.
await client.SendAsync(new CustomStruct { Hp = 99 });
```

Nothing needs to be processed here, so it promises the best performance theoretically possible in terms of transmission speed. However, since these struct-type variables need to be copied, I recommend handling everything as ref as a rule when you need to define a large struct-type, or it might slow down the process.

I believe that this can be easily and effectively applied to sending a large number of Transforms, such as an array of Vector3 variables.

Project Structure
---
If creates Server-Client project, I recommend make three projects. `Server`, `ServerDefinition`, `Client`.

![image](https://cloud.githubusercontent.com/assets/46207/21081857/e0f6dfce-c012-11e6-850d-358c5b928a82.png)

ServerDefinition is only defined interface(`IService<>`, `IStreamingHub<,>`)(and some share request/response types).

If debugging, I recommend use [SwitchStartupProject](https://marketplace.visualstudio.com/items?itemName=vs-publisher-141975.SwitchStartupProjectforVS2017) exteinson of VisualStudio and launch both Server and Client.

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

for Unity, you can't share by DLL(because can't share `IServer<>` because it is different reference both Unity and Server), so I recommends use [Shared Project](https://docs.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects), this technique is for xamarin but it works for Unity. Project structure is like this.

```
Server
- ref SharedProject
SharedProject
ClientLib(build to Unity dir)
- ref SharedProject
Assembly.CSharp(Unity)
- ref ClientLib(by dll)
```

Alternatively, using the definition of the interface placed on the Unity side, if you use a reference as a link of csproj(or refer to the directory, directory can link by `<Compile Include="target_dir", Link="your_path\%(RecursiveDir)%(FileName)%(Extension)">` to write csproj directly), you can simply configure it.

Hosting
---
I've recommend to use [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-2.2) to host .NET Core app. `MagicOnion.Hosting` package helps to build to use MagicOnion.

* Install-Package MagicOnion.Hosting

```csharp
// with the `Microsoft.Extensions.Hosting` package.
static async Task Main(string[] args)
{
    // IHostBuilder.UseMagicOnion to enable MagicOnion
    await new HostBuilder()
        .UseMagicOnion(new[] { new ServerPort("localhost", 12345, ServerCredentials.Insecure) })
        .RunConsoleAsync();
}
```

If you can want to load configuration, set logging, etc, see .NET Generic Host documantation.

Swagger
---
MagicOnion has built-in Http1 JSON Gateway and [Swagger](http://swagger.io/) integration for Unary operation. It can execute and debug RPC-API easily.

* Install-Package MagicOnion.HttpGateway

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

    // NuGet: Microsoft.AspNetCore.Server.Kestrel
    var webHost = new WebHostBuilder()
        .ConfigureServices(collection =>
        {
            // Add MagicOnionServiceDefinition for reference from Startup.
            collection.Add(new ServiceDescriptor(typeof(MagicOnionServiceDefinition), service));
        })
        .UseKestrel()
        .UseStartup<Startup>()
        .UseUrls("http://localhost:5432")
        .Build();

    webHost.Run(); // Hosting HTTP/1
}

// WebAPI Startup configuration.
public class Startup
{
    // Inject MagicOnionServiceDefinition from DIl
    public void Configure(IApplicationBuilder app, MagicOnionServiceDefinition magicOnion)
    {
        // Optional:Add Summary to Swagger
        // var xmlName = "Sandbox.NetCoreServer.xml";
        // var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

        // HttpGateway requires two middlewares.
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

Unity Supports
---
You can download `MagicOnion.Unity.*.*.*.package` and `moc.zip`(MagicOnionCompiler) in the [releases page](https://github.com/cysharp/MagicOnion/releases). But MagicOnion has no dependency so download gRPC lib from [gRPC daily builds](https://packages.grpc.io/), click Build ID and download `grpc_unity_package.*.*.*-dev.zip`. One more, requires MessagePack for C# for serialization, you can download `MessagePack.Unity.*.*.*.unitypackage` and `mpc.zip`(MessagePackCompiler) from [MessagePack-CSharp/releases](https://github.com/neuecc/MessagePack-CSharp/releases).

MagicOnion only supports `.NET 4.x` runtime and recommend to supports C# 7.0(Unity 2018.3) version. Allow unsafe Code and add `ENABLE_UNSAFE_MSGPACK`, you can use `UnsafeDirectBlitResolver` to extremely fast serialization.

Default MagicOnion's Unity client works well on Unity Editor or not IL2CPP env. But for IL2CPP environment, you need client code generation. `moc` is cross-platform standalone application.

```
moc arguments help:
  -i, --input=VALUE          [required]Input path of analyze csproj
  -o, --output=VALUE         [required]Output path(file) or directory base(in separated mode)
  -u, --unuseunityattr       [optional, default=false]Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer
  -c, --conditionalsymbol=VALUE [optional, default=empty]conditional compiler symbol
  -n, --namespace=VALUE      [optional, default=MagicOnion]Set namespace root name
  -a, asyncsuffix      [optional, default=false]Use methodName to async suffix
```

Please try it to run iOS/Android etc.

Raw gRPC APIs
---
MagicOnion can define and use primitive gRPC APIs(ClientStreaming, ServerStreaming, DuplexStreaming). Especialy DuplexStreaming is used underlying StreamingHub. If there is no reason, we recommend using StreamingHub.

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
    // VisualStudio 2017(C# 7.0), Unity 2018.3 supports return `async UnaryResult` directly
    // I recommend disable async-warning on project level. <NoWarn>1998</NoWarn>
    public async UnaryResult<string> SumAsync(int x, int y)
    {
        Logger.Debug($"Called SumAsync - x:{x} y:{y}");

        return (x + y).ToString();
    }

    // VS2015(C# 6.0), Unity 2018.2 use Task
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
    // await(C# 7.0, Unity 2018.3)
    var vvvvv = await client.SumAsync(10, 20);
    Console.WriteLine("SumAsync:" + vvvvv);
    
    // if use Task<UnaryResult>(Unity 2018.2), use await await
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

Author Info
---
This library is mainly developed by Yoshifumi Kawai(a.k.a. neuecc).  
He is the CEO/CTO of Cysharp which is a subsidiary of [Cygames](https://www.cygames.co.jp/en/).  
He is awarding Microsoft MVP for Developer Technologies(C#) since 2011.  
He is known as the creator of [UniRx](https://github.com/neuecc/UniRx/) and [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp/).

License
---
This library is under the MIT License.
