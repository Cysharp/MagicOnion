# Works with Unity
MagicOnion client supports Unity versions 2022.3.0f1 (LTS) and later.

It supports `.NET Standard 2.1` profile, IL2CPP build, and platforms such as Windows, Android, iOS, and macOS. At the moment, it does not support environments such as WebGL and consoles.

You need to install the following libraries to use MagicOnion as a Unity client:

- NuGetForUnity
- YetAnotherHttpHandler
- gRPC library
- MagicOnion.Client
- MagicOnion.Client.Unity

## Install NuGetForUnity

MagicOnion is provided as a NuGet package, so you need to install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) to install NuGet packages in Unity. Please refer to the README of NuGetForUnity for installation instructions.

## Install YetAnotherHttpHandler and gRPC libraries

C-core based Unity libraries have been discontinued in the gRPC project, so you need to use [YetAnotherHttpHandler]((https://github.com/Cysharp/YetAnotherHttpHandler)). Please refer to the [README of YetAnotherHttpHandler](https://github.com/Cysharp/YetAnotherHttpHandler) for installation instructions. [It also covers how to install grpc-dotnet (Grpc.Net.Client)](https://github.com/Cysharp/YetAnotherHttpHandler#using-grpc-grpc-dotnet-library).

## Install MagicOnion.Client

When using MagicOnion as a client in Unity, you need to install two packages: a NuGet package and a Unity extension package.

At first, install the MagicOnion.Client package using NuGetForUnity.

Next, install the Unity extension package using Unity Package Manager. To install, specify the following URL in "Add package from git URL...". Specify the version tag as needed.

```
https://github.com/Cysharp/MagicOnion.git?path=src/MagicOnion.Client.Unity/Assets/Scripts/MagicOnion.Client.Unity#{Version}
```

:::note
`{Version}` is the version number you want to install (e.g. `7.0.0`).
:::

## Using the client

When creating a gRPC channel, you need to change to use YetAnotherHttpHandler as shown below.

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5000", new GrpcChannelOptions
{
    HttpHandler = new YetAnotherHttpHandler()
    {
        // If you want to use HTTP/2 over cleartext (h2c), set `Http2Only = true`.
        // Http2Only = true,
    },
    DisposeHttpClient = true,
});
var client = MagicOnionClient.Create<IMyFirstService>(channel);
```

For Unity, we also provide extensions that wrap GrpcChannel and provide more useful features for development. For more information, see the [Unity Integration](/integration/unity) page.

## Works with IL2CPP

When your Unity project uses IL2CPP as a scripting backend, additional setup is required. For more information, see the [Ahead-of-Time compilation support with Source Generator](/source-generator/client) page.

