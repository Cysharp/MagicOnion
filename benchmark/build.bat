## build

dotnet publish Benchmark.Server/Benchmark.Server.csproj -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained -o out/linux/server
dotnet publish Benchmark.Client/Benchmark.Client.csproj -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained -o out/linux/client


dotnet publish Benchmark.Server/Benchmark.Server.csproj -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained -o out/win/server
dotnet publish Benchmark.Client/Benchmark.Client.csproj -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained -o out/win/client


## run

ASPNETCORE_ENVIRONMENT=Development ./out/linux/server/Benchmark.Server
ASPNETCORE_ENVIRONMENT=Production sudo ./out/linux/server/Benchmark.Server

./out/linux/server/Benchmark.Client
