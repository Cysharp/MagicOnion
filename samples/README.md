# MagicOnionSample
Provides a sample of a simple chat app using MagicOnion.  
There are two types of samples, a simplified version using `Code link` and a technical version using `SharedProject`.  

Please see here about MagicOnion itself.  
https://github.com/Cysharp/MagicOnion


## ChatAppCodeLink

#### Solution configuration
Create a Shared folder in the Unity project, and store the source code that you want to share with Server.  

And from the Server side, do Code link of the folder for Shared of Unity project.  
Add the following specification to `ChatApp.Server.csproj`.  
```
<ItemGroup>
  <Compile Include="..\ChatApp.Unity\Assets\Scripts\ServerShared\**\*.cs" Link="LinkFromUnity\%(RecursiveDir)%(FileName)%(Extension)" />
</ItemGroup>
```
![image](https://user-images.githubusercontent.com/38392460/55617417-fd88ef00-57ce-11e9-96c8-d1796ce614db.PNG)


## ChatAppSharedProject

#### Solution configuration
If you set Server as `.NET Core 2.X` and Unity as `.NET 4.X`, you can not share DLLs.  
But you can share source code by using `SharedProject`.  

However, since you can not refer to SharedProject directly from Unity Project, you need to create a separate Client side project that can refer to SharedProject separately, and it is `ChatApp.Unity.Library` that is in charge of that role.  

In that case, you need to set up conditional compilation symbols.  
```
ENABLE_UNSAFE_MSGPACK UNITY_2018_3_OR_NEWER NET_4_6 CSHARP_7_OR_LATER
```

![image](https://user-images.githubusercontent.com/38392460/55394849-40528900-557b-11e9-824e-5449a8425d8a.PNG)
  



## Code generate
In order to use MagicOnion, a dedicated Formatter for each MessagePackObject implemented is required.  
I will explain how each is generated.  

`MagicOnionCodeGenerator` https://github.com/cysharp/MagicOnion/releases  
`MessagePackUniversalCodeGenerator` https://github.com/neuecc/MessagePack-CSharp/releases  
See above for the auto generation tool.  

#### CodeLink ver
In the sample, source code is automatically generated from EditorMenu as an example.  
Please check the source code in the sample directly for details.  
The following excerpt is only part.  
```
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

#### SharedProject ver
The command is set to the build event so that Formatter is automatically generated according to the build of Unity.Library.

The following commands are set in the pre-build event.  
This time, it is written for Windows, but in case of other OS, please change the executable file path of GenerateTool if necessary.  
```
$(ProjectDir)GeneratorTools\MagicOnionCodeGenerator\win-x64\moc.exe -i "$(SolutionDir)..\ChatApp.Server\ChatApp.Server.csproj" -o "$(ProjectDir)Generated\MagicOnion.Generated.cs"
$(ProjectDir)GeneratorTools\MessagePackUniversalCodeGenerator\win-x64\mpc.exe -i "$(ProjectPath)" -o "$(ProjectDir)Generated\MessagePack.Generated.cs"
```

## Registration of Resolver
When using MagicOnion, it is necessary to perform Resolver registration processing in advance when the application is started on the Unity side.  
In the sample, a method using the attribute of RuntimeInitializeOnLoadMethod is prepared, and registration processing is performed.  
```
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

## How to run the app
1. Launch `ChatApp.Server` from VisualStudio.  
2. Run `ChatScene` from UnityEditor.  

If you want to connect simultaneously and chat, build Unity and launch it from the exe file.