# MemoryPack support
MagicOnion also supports MemoryPack as a message serializer. (preview)

```
dotnet add package MagicOnion.Serialization.MemoryPack
```

Set `MemoryPackMagicOnionSerializerProvider` to `MagicOnionSerializerProvider` on the client and server to serialize using MemoryPack.

```csharp
MagicOnionSerializerProvider.Default = MemoryPackMagicOnionSerializerProvider.Instance;

// or

await StreamingHubClient.ConnectAsync<IMyHub, IMyHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);
MagicOnionClient.Create<IMyService>(channel, MemoryPackMagicOnionSerializerProvider.Instance);
```

If you want to use MagicOnion.Client.SourceGenerator, you need to specify `Serializer = GenerateSerializerType.MemoryPack` to the attribute. The generated code will use MemoryPack instead of MessagePack.

The application must also call `MagicOnionMemoryPackFormatterProvider.RegisterFormatters()` on startup.
