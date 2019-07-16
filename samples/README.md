# MagicOnionSample
Provides a sample of a simple chat app using MagicOnion.  

Please see here about MagicOnion itself.  
https://github.com/Cysharp/MagicOnion


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

## Pack to Docker and deploy
If you hosting the samples on a server, recommend to use container. Add Dockerfile like below.

```dockerfile
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS sdk
COPY . ./workspace

RUN dotnet publish ./workspace/samples/ChatApp/ChatApp.Server/ChatApp.Server.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:2.2
COPY --from=sdk /app .
ENTRYPOINT ["dotnet", "ChatApp.Server.dll"]

# Expose ports.
EXPOSE 12345
```

And docker build, send to any container registory.

Here is the sample of deploy AWS [ECR](https://us-east-2.console.aws.amazon.com/ecr/) and [ECS](https://us-east-2.console.aws.amazon.com/ecs) by CircleCI.
```yml
version: 2.1
orbs:
# see: https://circleci.com/orbs/registry/orb/circleci/aws-ecr
# use Environment Variables : AWS_ECR_ACCOUNT_URL
#                             AWS_ACCESS_KEY_ID	
#                             AWS_SECRET_ACCESS_KEY
#                             AWS_REGION  
  aws-ecr: circleci/aws-ecr@4.0.1
# see: https://circleci.com/orbs/registry/orb/circleci/aws-ecs
# use Environment Variables : AWS_ACCESS_KEY_ID	
#                             AWS_SECRET_ACCESS_KEY
#                             AWS_REGION
  aws-ecs: circleci/aws-ecs@0.0.7
workflows:
  build-push:
    jobs:
      - aws-ecr/build_and_push_image:
          repo: sample-magiconion
      - aws-ecs/deploy-service-update:
          requires:
            - aws-ecr/build_and_push_image
          family: 'sample-magiconion-service'
          cluster-name: 'sample-magiconion-cluster'
          container-image-name-updates: 'container=sample-magiconion-service,tag=latest'
          
```

Here is the sample of deploy [Google Cloud Platform(GCP)](https://console.cloud.google.com/) by CircleCI.
```yml
version: 2.1
orbs:
  # see: https://circleci.com/orbs/registry/orb/circleci/gcp-gcr
  # use Environment Variables : GCLOUD_SERVICE_KEY
  #                             GOOGLE_PROJECT_ID
  #                             GOOGLE_COMPUTE_ZONE
    gcp-gcr: circleci/gcp-gcr@0.6.0
workflows:
    build_and_push_image:
        jobs:
            - gcp-gcr/build-and-push-image:
                image: sample-magiconion
                registry-url: asia.gcr.io # other: gcr.io, eu.gcr.io, us.gcr.io
```

Depending on the registration information of each environment and platform, fine tuning may be necessary, so please refer to the platform documentation and customize your own.


## How to run the app
1. Launch `ChatApp.Server` from VisualStudio.  
2. Run `ChatScene` from UnityEditor.  

If you want to connect simultaneously and chat, build Unity and launch it from the exe file.
