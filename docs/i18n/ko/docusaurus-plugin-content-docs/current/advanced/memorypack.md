# MemoryPack 지원 (Preview)
MagicOnion은 메시지 시리얼라이저로 MemoryPack도 지원합니다. (Preview)

```
dotnet add package MagicOnion.Serialization.MemoryPack
```

클라이언트와 서버에서 `MagicOnionSerializerProvider`에 `MemoryPackMagicOnionSerializerProvider`를 설정하여 MemoryPack을 사용한 직렬화를 수행할 수 있습니다.

```csharp
MagicOnionSerializerProvider.Default = MemoryPackMagicOnionSerializerProvider.Instance;

// 또는

await StreamingHubClient.ConnectAsync<IMyHub, IMyHubReceiver>(channel, receiver, serializerProvider: MemoryPackMagicOnionSerializerProvider.Instance);
MagicOnionClient.Create<IMyService>(channel, MemoryPackMagicOnionSerializerProvider.Instance);
```

MagicOnion.Client.SourceGenerator를 사용하려면 속성에 `Serializer = GenerateSerializerType.MemoryPack`을 지정해야 합니다. 생성된 코드는 MessagePack 대신 MemoryPack을 사용합니다.

애플리케이션은 시작 시 `MagicOnionMemoryPackFormatterProvider.RegisterFormatters()`도 호출해야 합니다.
