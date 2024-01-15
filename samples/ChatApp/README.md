# MagicOnionSample

Provides a sample of a simple chat app using MagicOnion.  

Please see here about MagicOnion itself.  
https://github.com/Cysharp/MagicOnion

## Getting started

To run simple ChatApp.Server, 

1. Launch `ChatApp.Server` from VisualStudio. 
2. Run `ChatScene` from UnityEditor. 

### ChatApp.Server

This is Sample Serverside MagicOnion.
You can lanunch via Visual Studio 2022 with .NET 8, open `MagicOnion.sln` > samples > set `ChatApp.Server` project as start up and Start Debug.

### ChatApp.Unity

Sample Clientside Unity.
You can ran with Unity from 2021.3 and higher then start on unity editor. Now unity client automatically connect to MagicOnion Server, try chat app!

## Solution configuration

We will place the C# code (Service, Hub interfaces, Request/Response objects, Logic) common to both the server and client in a Shared Project(.NET Standard class library).

This project will be referenced from Unity as a local package of UPM.

First, to reference it from Unity, place a [package.json](https://github.com/Cysharp/MagicOnion/blob/main/samples/ChatApp/ChatApp.Shared/package.json) and an [asmdef](https://github.com/Cysharp/MagicOnion/blob/main/samples/ChatApp/ChatApp.Shared/ChatApp.Shared.Unity.asmdef) inside the Shared Project.

Additionally, to ignore obj and bin in Unity, please place a [Directory.Build.props](https://github.com/Cysharp/MagicOnion/blob/main/samples/ChatApp/ChatApp.Shared/Directory.Build.props) file with the following content and change the output directories for obj and bin.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!--
      prior to .NET 8
      <BaseIntermediateOutputPath>.artifacts\obj\</BaseIntermediateOutputPath>
		  <BaseOutputPath>.artifacts\bin\</BaseOutputPath>
    -->

    <!-- after .NET 8: https://learn.microsoft.com/en-us/dotnet/core/sdk/artifacts-output -->
    <!-- Unity ignores . prefix folder -->
    <ArtifactsPath>$(MSBuildThisFileDirectory).artifacts</ArtifactsPath>
  </PropertyGroup>
</Project>
```

Finally, add the following line to the [Shared csproj](https://github.com/Cysharp/MagicOnion/blob/main/samples/ChatApp/ChatApp.Shared/ChatApp.Shared.csproj) to ignore the files for Unity from the server project.

```csharp
<ItemGroup>
  <None Remove="**\package.json" />
  <None Remove="**\*.asmdef" />
  <None Remove="**\*.meta" />
</ItemGroup>
```

https://github.com/Cysharp/MagicOnion/blob/main/samples/ChatApp/ChatApp.Unity/Packages/manifest.json

In the Unity project, specify the Shared project as a file reference in [Packages/manifest.json](https://github.com/Cysharp/MagicOnion/blob/main/samples/ChatApp/ChatApp.Unity/Packages/manifest.json). Since setting it up through the GUI results in a full path, it is necessary to manually change it to a relative path.

```json
{
  "dependencies": {
    "com.cysharp.magiconion.samples.chatapp.shared.unity": "file:../../ChatApp.Shared",
  }
}
```

## Code generate

MagicOnion Client is Source Generator based but still MessagePack needs generate code by command line tool.
  
Add the following specification to `ChatApp.Shared.csproj`.

```xml
<Target Name="RestoreLocalTools" BeforeTargets="GenerateMessagePack">
  <Exec Command="dotnet tool restore" />
</Target>

<Target Name="GenerateMessagePack" AfterTargets="Build">
  <PropertyGroup>
    <_MessagePackGeneratorArguments>-i ./ChatApp.Shared.csproj -o ../ChatApp.Unity/Assets/Scripts/Generated/MessagePack.Generated.cs</_MessagePackGeneratorArguments>
  </PropertyGroup>
  <Exec Command="dotnet tool run mpc $(_MessagePackGeneratorArguments)" />
</Target>
```
