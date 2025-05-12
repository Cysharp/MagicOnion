# Project Structure

This page describes the recommended project structure for projects using MagicOnion.

.NET applications typically consist of a large unit called a solution, which contains .NET projects such as applications and libraries.
Applications using MagicOnion typically have a server and client project, and are configured to share interfaces through some means.

## Typical structure of a .NET application

Here is an example of a typical project (solution) structure for a .NET application (console app, WPF or .NET MAUI etc).

- **MyApp.sln**: Solution file
- **MyApp.Server**: MagicOnion server application (ASP.NET Core gRPC Server)
    - This project provides MagicOnion APIs and processes requests from clients
- **MyApp.Client**: MagicOnion client application (Console, WPF, WinForms, .NET MAUI, etc...)
    - This project connects to the MagicOnion server and sends requests
- **MyApp.Shared**: Shared interface library (.NET library project)
    - This project contains interface definitions for MagicOnion, such as Hub and Service interface definitions, MessagePack types used in requests/responses, etc.

:::note
Replace `MyApp` with your project name or solution name.
:::

The following is an example of directory structure for the above project structure.

```plaintext
MyApp.sln
|─ src
|  ├─ MyApp.Server
|  │  ├─ MyApp.Server.csproj
|  │  ├─ Program.cs
|  |  ├─ Hubs
|  |  |  ├─ ChatHub.cs
|  |  └─ Services
|  |     └─ GreeterService.cs
|  ├─ MyApp.Client
|  │  ├─ MyApp.Client.csproj
|  │  └─ Program.cs
|  └─ MyApp.Shared
|     ├─ MyApp.Shared.csproj
|     ├─ Hubs
|     |  ├─ IChatHub.cs
|     |─ Services
|     |  └─ IGreeterService.cs
|     |─ Requests
|     |  └─ HelloRequest.cs
|     └─ Responses
|        └─ HelloResponse.cs
└─ test
   └─ MyApp.Server.Test
      └─ ...
```

`MyApp.Shared` project is created as a .NET class library, references the `MagicOnion.Abstractions` package, and defines only pure interface definitions, data types, and enum types.
And the `MyApp.Server` and `MyApp.Client` projects share interfaces by referencing the `MyApp.Shared` project.

This is a minimal configuration example, and in actual projects, you may have projects or hierarchies such as models, domains, and ViewModels as needed.

## Unity application structure

Here is an example of a project structure for sharing interfaces between Unity client and server projects.

The structure of a Unity application is different from a typical .NET project. This is because Unity projects cannot reference .NET library projects.

The recommended configuration is as follows:

- **MyApp.sln**: Solution file
- **MyApp.Server**: MagicOnion server application (ASP.NET Core gRPC Server)
- **MyApp.Unity**: Unity client application
- **MyApp.Shared**: Shared interface library (.NET library project and Unity local package)

```plaintext
MyApp.sln
|─ src
|   ├─ MyApp.Server
|   │  ├─ MyApp.Server.csproj
|   │  ├─ Program.cs
|   │  ├─ Hubs
|   │  │  └─ ChatHub.cs
|   │  └─ Services
|   │     └─ GreeterHub.cs
|   ├─ MyApp.Shared
|   │  ├─ MyApp.Shared.csproj
|   │  ├─ package.json
|   │  ├─ Hubs
|   │  │  └─ IChatHub.cs
|   │  └─ Services
|   │     └─ IGreeterHub.cs
|   └─ MyApp.Unity
|      ├─ Assembly-CSharp.csproj
|      ├─ MyApp.Unity.sln
|      └─ Assets
|         └─ ...
└─ test
   └─ MyApp.Server.Test
      └─ ...
```

`MyApp.Shared` project is created as a .NET class library, references the `MagicOnion.Abstractions` package, and defines only pure interface definitions, data types, and enum types.
In addition, it includes a `package.json` to make it available to Unity projects via the Unity Package Manager, and is configured not to output folders such as `bin` and `obj`. This allows C# source code contained in `MyApp.Shared` to be referenced from Unity projects.

Here is an example of a minimal `package.json` file for a Unity local package.

```json title="src/MyApp.Shared/package.json"
{
  "name": "com.cysharp.magiconion.samples.myapp.shared.unity",
  "version": "1.0.0",
  "displayName": "MyApp.Shared.Unity",
  "description": "MyApp.Shared.Unity"
}
```

We recommend creating a `MyApp.Shared.Unity.asmdef` (Assembly Definition) file to define the Unity-specific assembly and to avoid naming conflicts with `MyApp.Shared`. Be careful not to use the same name as `MyApp.Shared` by adding a `.Unity` suffix, for example.

Next, add the following two setting files (`Directory.Build.props` and `Directory.Bulid.targets`) to `MyApp.Shared` to prevent the creation of `bin` and `obj` directories and to hide Unity-specific files from the IDE.

```xml title="src/MyApp.Shared/Directory.Build.props"
<Project>
  <PropertyGroup>
    <!-- https://learn.microsoft.com/en-us/dotnet/core/sdk/artifacts-output -->
    <ArtifactsPath>$(MSBuildThisFileDirectory).artifacts</ArtifactsPath>
  </PropertyGroup>
</Project>
```

```xml title="src/MyApp.Shared/Directory.Build.targets"
<Project>
  <!-- Hide Unity-specific files from Visual Studio and .NET SDK -->
  <ItemGroup>
    <None Remove="**\*.meta" />
  </ItemGroup>

  <!-- Hide build artifacts from Visual Studio and .NET SDK -->
  <ItemGroup>
    <None Remove=".artifacts\**\**.*" />
    <None Remove="obj\**\*.*;bin\**\*.*" />
    <Compile Remove=".artifacts\**\**.*" />
    <Compile Remove="bin\**\*.*;obj\**\*.*" />
    <EmbeddedResource Remove=".artifacts\**\**.*" />
    <EmbeddedResource Remove="bin\**\*.*;obj\**\*.*" />
  </ItemGroup>
</Project>
```

With this configuration, intermediate files and built files are output to `.artifacts`, so if `bin` and `obj` directories are created, delete them.

Next, add a reference to `MyApp.Shared` in the Unity project's `Packages/manifest.json`.

```json title="src/MyApp.Unity/Packages/manifest.json"
{
  "dependencies": {
    "com.cysharp.magiconion.samples.myapp.shared.unity": "file:../../MyApp.Shared",
    ...
  }
}
```

`MyApp.Server` project references the `MyApp.Shared` project to share interfaces. This is similar to a typical .NET application.

### Merging solutions with SlnMerge

By using [SlnMerge](https://github.com/Cysharp/SlnMerge), you can merge the solutions generated by Unity and .NET projects.

For example, MyApp.sln contains MyApp.Server and MyApp.Shared projects, but the solution generated by Unity contains only Unity projects (Assembly-CSharp, Assembly-CSharp-Editor, etc.). By using SlnMerge, you can merge these solutions so that the server project can be seamlessly referenced even when the solution is opened from Unity.

This allows for reference searches and debugger step-ins between Unity and .NET projects, which can improve development experiences.
