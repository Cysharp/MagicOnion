using MagicOnion.Client;

namespace MagicOnion.Serialization.MemoryPack.Tests;

[MagicOnionClientGeneration(typeof(MagicOnionGeneratedClientInitializer), Serializer = MagicOnionClientGenerationAttribute.GenerateSerializerType.MemoryPack)]
public partial class MagicOnionGeneratedClientInitializer
{}
