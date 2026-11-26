# Pre-generated client codes

MagicOnion supports Unity, NativeAOT, and other platforms that require pre-generated client code with Source Generator (MagicOnion.Client.SourceGenerator).

MagicOnion.Client.SourceGenerator is shipped with MagicOnion.Client package.
This means that you no longer need to the install generator tool (moc) and setup additional build steps.

## Supported development environments
- Unity 2022.3.12f1 or later
- .NET 6 or later
- Visual Studio 2022 version 17.2 or later
- Rider 2023.1 or later

## Usage
Define a `partial` class with any name of your choosing within the application. Mark it with the `MagicOnionClientGeneration` attribute, and specify any service type found within the assembly where you want to search for the service interface.

For example, if the `MyApp.Shared` assembly contains `MyApp.Shared.Services.IGreeterService` and `MyApp.Shared.Hubs.IChatHub`, specify one of them.

```csharp
using MagicOnion.Client;

[MagicOnionClientGeneration(typeof(MyApp.Shared.Services.IGreeterService))]
partial class MagicOnionGeneratedClientInitializer {}
```

At startup, MagicOnion registers formatter factories for the generated client's argument and return types. These factories are available through MessagePack's `Default` and `DefaultAot` options. When using a custom generic object as a serialization root in Native AOT or IL2CPP, declare the required closed types with `MessagePackSerializable`. For details, see the [MessagePack v4 migration guide](https://github.com/MessagePack-CSharp/MessagePack-CSharp/tree/v4.0.0-rc.1#migrating-from-v3).

## Source generation options
You can specify options in the named constructor of the attribute.

- **`DisableAutoRegistration`**: Sets whether to disable automatically calling `Register` during start-up. (Automatic registration requires .NET 5+ or Unity)
- **`Serializer`**: Sets the serializer used for message serialization. The default value is `GenerateSerializerType.MessagePack`.

### Additional options

You can specify additional options with the `MagicOnionClientGenerationOption` attribute.

The MessagePack options control registration of custom `MessagePackFormatterFactory` classes.

- **`MessagePack.FormatterNamespace`** specifies the root namespace for custom formatter factories. It defaults to `MessagePack.Formatters`.
- **`MessagePack.GenerateResolverForCustomFormatter`** enables registration of custom factories named `<TypeName>FormatterFactory<T>` for generic RPC types. It defaults to `false`.
