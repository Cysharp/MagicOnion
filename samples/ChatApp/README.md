# MagicOnionSample

Provides a sample of a simple chat app using MagicOnion.  

Please see here about MagicOnion itself.  
https://github.com/Cysharp/MagicOnion

## Getting started

To run simple ChatApp.Server, 

1. Launch `ChatApp.Server` from VisualStudio.  
2. Run `ChatScene` from UnityEditor.  

To run ChatApp.Server with OpenTelemetry,

1. Launch `ChatApp.Server.Telemetry` from VisualStudio.  
2. Run `ChatScene` from UnityEditor.  

If you want launch on Container, see [Container support](#container-support) section.

### ChatApp.Server

This is Sample Serverside MagicOnion.
You can lanunch via Visual Studio 2019, open `MagicOnion.sln` > samples > set `ChatApp.Server` project as start up and Start Debug.

### ChatApp.Server.Telemetry

This is Sample Serverside MagicOnion with OpenTelemetry implementation for Prometheus and Zipkin exporters.
You can lanunch via Visual Studio 2019, open `MagicOnion.sln` > samples > set `ChatApp.Server.Telemetry` project as start up and Start Debug.

> Addtional note: If you want run MagiconOnion with telemetry containers please follow to the [README](https://github.com/Cysharp/MagicOnion#try-visualization-on-localhost)

### ChatApp.Unity

Sample Clientside Unity.
You can ran with Unity from 2018.4.5f1 and higher then start on unity editor.

> TIPS: confirmed run on 2019.1.10f1

Now unity client automatically connect to MagicOnion Server, try chat app!

> TIPS: ChatApp.Unity contains a gRPCs library in the repository that are required for MagicOnion operation. If you want other version of gRPC lib, go [gRPC Packages](https://packages.grpc.io/), select the latest build ID from the link below, download the Unity support library for gRPC, and import it into Unity will replace existing.

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

## Container support

We prepare container sample with kubernetes and docker.
You may find MagicOnion on works fine on container environment.

Let's see how it work on Kubernetes, then docker.

### Kubernetes

This instruction in written under following environment.

* kubectl 1.16.8
* kubectx
* wsl

> ProTips: If you are using Windows, you can try k8s on WSL with Docker for Windows installed.

**Getting started**

Let's try run ChatApp on kubernetes cluster.

Deploy your manifests to the cluster.

```shell
kubectx docker-desktop
kubectl kustomize ./k8s/common | kubectl apply -f -
helm upgrade --install nginx-ingress --namespace chatapp stable/nginx-ingress
helm upgrade --install prometheus --namespace chatapp -f ./k8s/prometheus/values.yaml stable/prometheus
helm upgrade --install grafana --namespace chatapp -f ./k8s/grafana/values.yaml stable/grafana
```

wait until resources are launch complete.

```shell
kubectl rollout status deploy chatapp -n chatapp
kubectl rollout status deploy nginx-ingress-controller -n chatapp
kubectl rollout status deploy nginx-ingress-default-backend -n chatapp
kubectl rollout status deploy prometheus-server -n chatapp
kubectl rollout status deploy grafana -n chatapp
```

Everything is done, check kubernetes resources is up and running.

```shell
kubectl get deploy,svc,daemonset,ingress
```

Now you are ready to accept ChatApp.Unity requests.

**Access to ChatApp.Server on Kubernetes**

ChatApp.Unity can access to ChatApp.Unity on k8s with `localhost:12345`.
Just launch ChatApp.Unity and enjoy chat.

**hosts file for ingress access**

Before accesing Grafana dashboard, put `Hosts` entry to your OS, this enable your to access prometheus and grafana via ingress.

```txt
127.0.0.1 grafana.chatapp.magiconion.local
127.0.0.1 prometheus.chatapp.magiconion.local
```

**Access to the Grafana dashboard**

Let's access to your dashboard.

> Make sure you already put hosts entry.

* http://prometheus.chatapp.magiconion.local
* http://grafana.chatapp.magiconion.local

Main dashboard is Grafana, let's login with user `admin`, password will be show via below command.

```shell
kubectl get secret --namespace chatapp grafana -o jsonpath="{.data.admin-password}" | base64 --decode && echo
```

Grafana dashboard [MagicOnion Overview](https://grafana.com/grafana/dashboards/10584) will be automatically loaded into grafana, you may see your magiconion metrics.

![image](https://user-images.githubusercontent.com/3856350/83670579-5d1a9e80-a60e-11ea-9289-89a412dd5877.png)

**Clean up**

after all, clean up your k8s resources.

```shell
kubectx docker-desktop
helm uninstall nginx-ingress -n chatapp
helm uninstall prometheus -n chatapp
helm uninstall grafana -n chatapp
kubectl kustomize ./k8s/common | kubectl delete -f -
```

### Docker

You can confirm MagicOnion running on container.

```shell
docker-compose up
```

If you want build current ChatApp.Server with current csproj, use follows.

```shell
docker-compose -f docker-compose.self.yaml up
```

### Docker (self build)

use following to try ChatApp.Server.Telemery.

```shell
docker-compose -f docker-compose.telemetry.yaml up --build
```

You can access to dashboard with following urls.

* [prometheus](http://localhost:9090/)
* [zipkin](http://localhost:9411/)
* [grafana](http://localhost:3000/)

Grafana user/password will be `admin/admin` by default.

If you want build current ChatApp.Server.Telemetry with current csproj, use follows.

```shell
docker-compose -f docker-compose.telemetry.self.yaml up --build
```
