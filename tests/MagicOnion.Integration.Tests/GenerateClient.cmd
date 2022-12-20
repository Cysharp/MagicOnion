setlocal
cd ..\..\src\MagicOnion.Generator
dotnet run --framework net7.0 -- --verbose --disable-auto-register --namespace MagicOnion.Integration.Tests.Generated -i ../../tests/MagicOnion.Integration.Tests/MagicOnion.Integration.Tests.csproj -o ../../tests/MagicOnion.Integration.Tests/_GeneratedClient.cs
