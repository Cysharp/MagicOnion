# MagicOnionSample

Provides a sample of a simple chat app using MagicOnion.  

Please see here about MagicOnion itself.  
https://github.com/Cysharp/MagicOnion

## Getting started

Sample Serverside MagicOnion can lanunch via Visual Studio 2019, open `MagicOnion.sln` > samples > set `ChatApp.Server` project as start up and Start Debug.
If you want run MagiconOnion with telemetry containers please follow to the [README](https://github.com/Cysharp/MagicOnion#try-visualization-on-localhost)

Sample Clientside Unity can ran with Unity 2019.1.10f1, then start on unity editor.

Now unity client automatically connect to MagicOnion, try chat app!

*Notes  
The Sample does not contain a library of gRPCs in the repository that are required for operation.  
Before running the app, select the latest build ID from the link below, download the Unity support library for gRPC, and import it into Unity.  
[gRPC Packages](https://packages.grpc.io/)

## Solution configuration
![image](https://user-images.githubusercontent.com/38392460/71507978-a3ced480-28c9-11ea-9090-8f4ef4ffc306.png)
Create a Shared folder in the Unity project, and store the source code that you want to share with Server.  

Create a Shared project for common code reference and reference the source code that exists on the Unity side with a code link.  
The Server project simply references the Shared project directly.  
  
※1 Code link  
Add the following specification to `ChatApp.Shared.csproj`.
```xml
<ItemGroup>
  <Compile Include="..\ChatApp.Unity\Assets\Scripts\ServerShared\**\*.cs" />
</ItemGroup>
```


## Code generate
In order to use MagicOnion, a dedicated Formatter for each MessagePackObject implemented is required.  
I will explain how each is generated.  
  
The package reference and build tasks for automatic code generation are shown in  
Add to the Shared project and automatically generate code.  
  
Add the following specification to `ChatApp.Shared.csproj`.
```xml
<ItemGroup>
  <PackageReference Include="MagicOnion.Abstractions" Version="3.0.0" />
  <PackageReference Include="MagicOnion.MSBuild.Tasks" Version="3.0.0" PrivateAssets="All" />
  <PackageReference Include="MessagePack.MSBuild.Tasks" Version="2.0.323" PrivateAssets="All" />
</ItemGroup>

<Target Name="GenerateMessagePack" AfterTargets="Compile">
  <MessagePackGenerator Input=".\ChatApp.Shared.csproj" Output="..\ChatApp.Unity\Assets\Scripts\Generated\MessagePack.Generated.cs" />
</Target>
<Target Name="GenerateMagicOnion" AfterTargets="Compile">
  <MagicOnionGenerator Input=".\ChatApp.Shared.csproj" Output="..\ChatApp.Unity\Assets\Scripts\Generated\MagicOnion.Generated.cs" />
</Target>
```


## Registration of Resolver
When using MagicOnion, it is necessary to perform Resolver registration processing in advance when the application is started on the Unity side.  
In the sample, a method using the attribute of RuntimeInitializeOnLoadMethod is prepared, and registration processing is performed.  
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void RegisterResolvers()
{
    CompositeResolver.RegisterAndSetAsDefault
    (
        MagicOnion.Resolvers.MagicOnionResolver.Instance,
        MessagePack.Resolvers.GeneratedResolver.Instance,
        BuiltinResolver.Instance,
        PrimitiveObjectResolver.Instance
    );
}
```

## Cleaning up Hub and Channel
Hubs and Channels that are no longer needed need to be released.  
In the sample, processing is performed at the end of Scene.  
```csharp
async void OnDestroy()
{
    // Clean up Hub and channel
    await this.streamingClient.DisposeAsync();
    await this.channel.ShutdownAsync();
}
```

## Detecting a disconnect
You can detect a disconnect by waiting for `WaitForDisconnect` in the instance method of `ISteamingHub`.  
  
In the example, a separate thread waits for WaitForDisconnect to detect a disconnect on the client side.
```csharp
void Start()
{
    var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
    var streamingClient = StreamingHubClient.Connect<IChatHub, IChatHubReceiver>(this.channel, this);
    
    RegisterDisconnectEvent(streamingClient);
}

private async void RegisterDisconnectEvent(IChatHub streamingClient)
{
    try
    {
        // you can wait disconnected event
        await streamingClient.WaitForDisconnect();
    }
    finally
    {
        // try-to-reconnect? logging event? close? etc...
        Debug.Log("disconnected server.");
    }
}
```

## Dynamic coexistence of Server and Unity project files.
The Server project is removed every time Unity regenerates a solution file.  
With a class that extends `AssetPostprocessor` and a method of `OnGeneratedSlnSolution`
An arbitrary project file is automatically included when a solution file is generated.  
This ensures that the Server and Unity project files co-exist.  
  
The Sample implementation is as follows:.
https://github.com/Cysharp/MagicOnion/blob/master/samples/ChatApp/ChatApp.Unity/Assets/Editor/SolutionFileProcessor.cs

## How to run the app

1. Launch `ChatApp.Server` from VisualStudio.  
2. Run `ChatScene` from UnityEditor.  

If you want to connect simultaneously and chat, build Unity and launch it from the exe file.

### Docker

use docker-compose to build and run ChatApp.Server.

```shell
docker-compose up
```
