FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE 12345
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["samples/ChatApp.Telemetry/MicroServer/MicroServer.csproj", "samples/ChatApp.Telemetry/MicroServer/"]
COPY ["samples/ChatApp.Telemetry/ChatApp.Shared/ChatApp.Shared.csproj", "samples/ChatApp.Telemetry/ChatApp.Shared/"]
COPY ["src/MagicOnion/MagicOnion.csproj", "src/MagicOnion/"]
COPY ["src/MagicOnion.Abstractions/MagicOnion.Abstractions.csproj", "src/MagicOnion.Abstractions/"]
COPY ["src/MagicOnion.Server/MagicOnion.Server.csproj", "src/MagicOnion.Server/"]
COPY ["src/MagicOnion.Server.OpenTelemetry/MagicOnion.Server.OpenTelemetry.csproj", "src/MagicOnion.Server.OpenTelemetry/"]
COPY ["src/MagicOnion.Shared/MagicOnion.Shared.csproj", "src/MagicOnion.Shared/"]
RUN dotnet restore "samples/ChatApp.Telemetry/MicroServer/MicroServer.csproj"
COPY . .
WORKDIR "/src/samples/ChatApp.Telemetry/MicroServer"
RUN dotnet build "MicroServer.csproj" -c Debug -o /app

FROM build AS publish
RUN dotnet publish "MicroServer.csproj" -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MicroServer.dll"]