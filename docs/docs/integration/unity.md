---
title: Unity
---
# Unity
## gRPC channel management integration for Unity
Wraps gRPC channels and provides a mechanism to manage them with Unity's lifecycle.
This prevents your application and the Unity Editor from freezing by releasing channels and StreamingHub in one place.

The editor extension also provides the ability to display the communication status of channels.

![](https://user-images.githubusercontent.com/9012/111609638-da21a800-881d-11eb-81b2-33abe80ea497.gif)

> **NOTE**: The data rate is calculated only for the message body of methods, and does not include Headers, Trailers, or Keep-alive pings.

### New APIs
- `MagicOnion.GrpcChannelx` class
  - `GrpcChannelx.ForTarget(GrpcChannelTarget)` method
  - `GrpcChannelx.ForAddress(Uri)` method
  - `GrpcChannelx.ForAddress(string)` method
- `MagicOnion.Unity.GrpcChannelProviderHost` class
  - `GrpcChannelProviderHost.Initialize(IGrpcChannelProvider)` method
- `MagicOnion.Unity.IGrpcChannelProvider` interface
  - `DefaultGrpcChannelProvider` class
  - `LoggingGrpcChannelProvider` class

### Usages
#### 1. Prepare to use `GrpcChannelx` in your Unity project.
Before creating a channel in your application, you need to initialize the provider host to be managed.

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
public static void OnRuntimeInitialize()
{
    // Initialize gRPC channel provider when the application is loaded.
    GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(() => new GrpcChannelOptions()
    {
        HttpHandler = new YetAnotherHttpHandler()
        {
            Http2Only = true,
        },
        DisposeHttpClient = true,
    }));
}
```

GrpcChannelProviderHost will be created as DontDestroyOnLoad and keeps existing while the application is running. DO NOT destroy it.

![image](https://user-images.githubusercontent.com/9012/111586444-2eb82980-8804-11eb-8a4f-a898c86e5a60.png)

#### 2. Use `GrpcChannelx.ForTarget` or `GrpcChannelx.ForAddress` to create a channel.
Use `GrpcChannelx.ForTarget` or `GrpcChannelx.ForAddress` to create a channel instead of `new Channel(...)`.

```csharp
var channel = GrpcChannelx.ForTarget(new GrpcChannelTarget("localhost", 12345, ChannelCredentials.Insecure));
// or
var channel = GrpcChannelx.ForAddress("http://localhost:12345");
```

#### 3. Use the channel instead of `Grpc.Core.Channel`.
```csharp
var channel = GrpcChannelx.ForAddress("http://localhost:12345");

var serviceClient = MagicOnionClient.Create<IGreeterService>(channel);
var hubClient = StreamingHubClient.ConnectAsync<IGreeterHub, IGreeterHubReceiver>(channel, this);
```

### Extensions for Unity Editor (Editor Window & Inspector)
![image](https://user-images.githubusercontent.com/9012/111585700-0d0a7280-8803-11eb-8ce3-3b8f9d968c13.png)
