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

### ChatApp.Server

This is Sample Serverside MagicOnion.
You can lanunch via Visual Studio 2019, open `MagicOnion.sln` > samples > set `ChatApp.Server` project as start up and Start Debug.

### ChatApp.Server.Telemetry

This is Sample Serverside MagicOnion with OpenTelemetry implementation.
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

There are docker and kubernetes samples for you.
You can confirm MagicOnion on container usage.

### Kubernetes

**Preprequisites**

Make sure you are installed follows.

* kubectl 1.14 and higher
* kubectx
* wsl

This sample can be run on local k8s.
If you are using Windows, you can try with WSL with Docker for Windows.

**Getting started**

Let's try with local kubernetes cluster running on Docker for Windows.

Put `Hosts` entry to access prometheus and grafana via ingress.

> TIPS: Windows user better set these hosts on Windows side, not WSL.

```txt
127.0.0.1 grafana.chatapp.magiconion.local
127.0.0.1 prometheus.chatapp.magiconion.local
```

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

Everything is done, your kubernetes resources will be follows.

```
$ kubectl get deploy,svc,daemonset,ingress

NAME                                            READY   UP-TO-DATE   AVAILABLE   AGE
deployment.apps/chatapp                         1/1     1            1           28m
deployment.apps/grafana                         1/1     1            1           12m
deployment.apps/nginx-ingress-controller        1/1     1            1           64m
deployment.apps/nginx-ingress-default-backend   1/1     1            1           64m
deployment.apps/prometheus-alertmanager         1/1     1            1           59m
deployment.apps/prometheus-kube-state-metrics   1/1     1            1           59m
deployment.apps/prometheus-server               1/1     1            1           59m

NAME                                    TYPE           CLUSTER-IP       EXTERNAL-IP   PORT(S)                      AGE
service/chatapp-prometheus-svc          ClusterIP      10.101.87.162    <none>        9184/TCP                     42m
service/chatapp-svc                     LoadBalancer   10.98.216.37     localhost     12345:30809/TCP              50m
service/grafana                         ClusterIP      10.110.57.106    <none>        80/TCP                       12m
service/nginx-ingress-controller        LoadBalancer   10.111.154.227   localhost     80:31687/TCP,443:32279/TCP   64m
service/nginx-ingress-default-backend   ClusterIP      10.103.10.115    <none>        80/TCP                       64m
service/prometheus-alertmanager         ClusterIP      10.96.190.143    <none>        80/TCP                       59m
service/prometheus-kube-state-metrics   ClusterIP      10.98.178.7      <none>        8080/TCP                     59m
service/prometheus-node-exporter        ClusterIP      None             <none>        9100/TCP                     59m
service/prometheus-server               ClusterIP      10.107.13.135    <none>        80/TCP                       59m

NAME                                      DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE
daemonset.apps/prometheus-node-exporter   1         1         1       1            1           <none>          59m

NAME                                   HOSTS                                 ADDRESS        PORTS   AGE
ingress.extensions/grafana             grafana.chatapp.magiconion.local      192.168.65.3   80      12m
ingress.extensions/prometheus-server   prometheus.chatapp.magiconion.local   192.168.65.3   80      59m
```

Now your pods are ready.

**Access to ChatApp.Server on Kubernetes**

ChatApp.Unity can access to ChatApp.Unity on k8s with `localhost:12345`.
Just launch ChatApp.Unity and enjoy chat.

**Access to the Dashboard**

Let's access to your dashboard.

> Make sure you already put hosts entry.

* http://prometheus.chatapp.magiconion.local
* http://grafana.chatapp.magiconion.local

Main dashboard is Grafana, let's login with user `admin`, password will be show via below command.

```shell
kubectl get secret --namespace chatapp grafana -o jsonpath="{.data.admin-password}" | base64 --decode && echo
```

![image](https://user-images.githubusercontent.com/3856350/83566667-57b04c00-a55b-11ea-986e-eeaa4af35c21.png)

**Clean up**

after all, you can clean up your resources.

```shell
helm uninstall nginx-ingress -n chatapp
helm uninstall prometheus -n chatapp
helm uninstall grafana -n chatapp
kubectl kustomize ./k8s/common | kubectl delete -f -
```


### Docker with already built image.

You can confirm MagicOnion on container running with already build docker image.
Use docker-compose to build with docker, grafana user/password will be `admin/admin` by default.

> TIPS: make sure you are locate at ./samples/ChatApp/

```shell
docker-compose up
```

If you want try ChatApp.Server.Telemery, use followings.

```shell
docker-compose -f docker-compose.telemetry.yaml up
```

### Docker with self build image

You can confirm MagicOnion on container running with actual csproj.

```shell
docker-compose -f docker-compose.self.yaml up
```

If you want try ChatApp.Server.Telemery, use followings.
This will provision grafana datasource and dashboard for you.

```shell
docker-compose -f docker-compose.telemetry.self.yaml up --build
```

## docker push

cysharp/magiconion_sample_chatapp

```shell
docker-compose -f docker-compose.self.yaml build
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp:latest
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp:3.0.13
docker push cysharp/magiconion_sample_chatapp:latest
docker push cysharp/magiconion_sample_chatapp:3.0.13
```

cysharp/magiconion_sample_chatapp_telemetry

```shell
docker-compose -f docker-compose.self.telemetry.yaml build magiconion
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:latest
docker tag chatapp_magiconion:latest cysharp/magiconion_sample_chatapp_telemetry:3.0.13
docker push cysharp/magiconion_sample_chatapp_telemetry:latest
docker push cysharp/magiconion_sample_chatapp_telemetry:3.0.13
```

