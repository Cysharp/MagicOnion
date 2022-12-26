setlocal
cd ..\..\src\MagicOnion.Generator
dotnet run --framework net7.0 -- --verbose --disable-auto-register --namespace MagicOnion.Serialization.MemoryPack.Tests.Generated -i ../../tests/MagicOnion.Serialization.MemoryPack.Tests/MagicOnion.Serialization.MemoryPack.Tests.csproj -o ../../tests/MagicOnion.Serialization.MemoryPack.Tests/_GeneratedClient.cs --serializer MemoryPack
