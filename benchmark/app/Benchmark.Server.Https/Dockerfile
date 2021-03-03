#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Benchmark.Server/Benchmark.Server.Https.csproj", "Benchmark.Server.Https/"]
RUN dotnet restore "Benchmark.Server.Https/Benchmark.Server.csproj"
COPY . .
WORKDIR "/src/Benchmark.Server.Https"
RUN dotnet build "Benchmark.Server.Https.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Benchmark.Server.Https.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Benchmark.Server.Https.dll"]