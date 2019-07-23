# MagicOnionSample
Provides a sample of a simple chat app using MagicOnion.  

Please see here about MagicOnion itself.  
https://github.com/Cysharp/MagicOnion

## Getting started

Sample Serverside MagicOnion can lanunch via Visual Studio 2019, open `MagicOnion.sln` > samples > set `ChatApp.Server` project as start up and Start Debug.
If you want run MagiconOnion with telemetry containers please run with docker-compose, run `docker-compose build` then run `docker-compose up`. (make sure docker is installed and file system sharing is enabled.)

Sample Clientside Unity can ran with Unity 2019.1.10f1, then start on unity editor.

Now unity client automatically connect to MagicOnion, try chat app!

## Solution configuration
Create a Shared folder in the Unity project, and store the source code that you want to share with Server.  

And from the Server side, do Code link of the folder for Shared of Unity project.  
Add the following specification to `ChatApp.Server.csproj`.  
```xml
<ItemGroup>
  <Compile Include="..\ChatApp.Unity\Assets\Scripts\ServerShared\**\*.cs" LinkBase="LinkFromUnity" />
</ItemGroup>
```
![image](https://user-images.githubusercontent.com/38392460/55617417-fd88ef00-57ce-11e9-96c8-d1796ce614db.PNG)

Other than that, You can also copy files by creating a CopyTask.
```xml
  <ItemGroup>
    <SourceFiles Include="$(ProjectDir)..\ChatApp.Unity\Assets\Scripts\ServerShared\**\*.cs" Exclude="**\bin\**\*.*;**\obj\**\*.*" />
  </ItemGroup>
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Copy SourceFiles="@(SourceFiles)" DestinationFiles="$(ProjectDir)\LinkFromUnity\%(RecursiveDir)%(Filename)%(Extension)" SkipUnchangedFiles="true" />
  </Target>
```

## Code generate
In order to use MagicOnion, a dedicated Formatter for each MessagePackObject implemented is required.  
I will explain how each is generated.  

`MagicOnionCodeGenerator` https://github.com/cysharp/MagicOnion/releases  
`MessagePackUniversalCodeGenerator` https://github.com/neuecc/MessagePack-CSharp/releases  
See above for the auto generation tool.  

In the sample, source code is automatically generated from EditorMenu as an example.  
Please check the source code in the sample directly for details.  
The following excerpt is only part.  
```csharp
var psi = new ProcessStartInfo()
{
    CreateNoWindow = true,
    WindowStyle = ProcessWindowStyle.Hidden,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    FileName = filePath + exeFileName,
    Arguments = $@"-i ""{rootPath}/ChatApp.Server/ChatApp.Server.csproj"" -o ""{Application.dataPath}/Scripts/Generated/MagicOnion.Generated.cs""",
};

var p = Process.Start(psi);
```
![image](https://user-images.githubusercontent.com/38392460/55618800-5908ac00-57d2-11e9-9238-10dc13a1dbfe.png)


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

## Telemetry

Sample offers Telemetly collected via OpenTelemetry.

When you launch docker-compose, followings set of service will launch for you.

* **MagicOnion** stats export on http://localhost:9182/metrics/.
* **cAdvisor** launch on http://localhost:8080.
* **Prometheus** launch on http://localhost:9090.
* **Grafana** launch on http://localhost:3000. (default username: `admin`, password: `admin`)
* optional: if you want **node_exporter**, uncomment in `docker-compose.yml` and it launch on http://localhost:9100. make sure host volume is mounted to container.

To configure grafana dashboard, follow the steps.

* add DataSource: Data Souces> add > Prometheus (prometheus URL will be https://prometheus:9090)
* add Dashboard:
    * **Prometheus 2.0 Stats** dashboard: open Data Source > prometheus > dashboard tab > add Prometheus 2.0 Stats
    * **Docker and Host Monitoring w/ Prometheus** dashboard (cAdvisor): open Dashboard > Manage > Import > https://grafana.com/grafana/dashboards/179
    * **MagicOnion Overview** dashboard (MagicOnion & cAdvisor): open Dashboard > Manage > Import > https://grafana.com/grafana/dashboards/10584
    * optional: **node_exporter 1.8** dashboard: open Dashboard > Manage > Import > https://grafana.com/grafana/dashboards/1860

Now you can observe MagicOnion metrics throw Grafana.

![image](https://user-images.githubusercontent.com/3856350/61683238-c58ec300-ad4f-11e9-9057-1cfb9c30cd67.png)

To configure your metrics, open `MagicOnion.sln` > samples > `ChatApp.Server` project > `MagicOnionCollector.cs`.

OpenTelemetryCollectorLogger implements IMagicOnionLogger and hook it's timing to collect metrics.

```csharp
namespace MagicOnion.Server
{
    public interface IMagicOnionLogger
    {
        void BeginBuildServiceDefinition();
        void BeginInvokeHubMethod(StreamingHubContext context, ArraySegment<byte> request, Type type);
        void BeginInvokeMethod(ServiceContext context, byte[] request, Type type);
        void EndBuildServiceDefinition(double elapsed);
        void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type type, double elapsed, bool isErrorOrInterrupted);
        void EndInvokeMethod(ServiceContext context, byte[] response, Type type, double elapsed, bool isErrorOrInterrupted);
        void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount);
        void ReadFromStream(ServiceContext context, byte[] readData, Type type, bool complete);
        void WriteToStream(ServiceContext context, byte[] writeData, Type type);
    }
}
```

To implement your own metrics, define `IView` and register it `Stats.ViewManager.RegisterView(YOUR_VIEW);`, then send metrics.
There are several way to send metrics.

* Send each metrics each line.

```chsarp
statsRecorder.NewMeasureMap().Put(YOUR_METRICS, 1).Record(CreateTag(context));
```

* Put many metrics and send at once: 

```csharp
var map = statsRecorder.NewMeasureMap(); map.Put(UnaryElapsed, elapsed);
map.Put(UnaryElapsed, elapsed);
map.Put(UnaryResponseSize, response.LongLength);
if (isErrorOrInterrupted)
{
    map.Put(UnaryErrorCount, 1);
}

map.Record(CreateTag(context));
```

* create tag scope and set number of metrics.

```csharp
var tagContextBuilder = Tagger.CurrentBuilder.Put(FrontendKey, TagValue.Create("mobile-ios9.3.5"));
using (var scopedTags = tagContextBuilder.BuildScoped())
{
    StatsRecorder.NewMeasureMap().Put(VideoSize, values[0] * MiB).Record();
}
```

Make sure your View's column, and metrics tag is matched. Otherwise none of metrics will shown.