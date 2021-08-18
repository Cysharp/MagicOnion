FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE 12345
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["samples/ChatApp.Telemetry/ChatApp.Server/ChatApp.Server.csproj", "samples/ChatApp.Telemetry/ChatApp.Server/"]
COPY ["samples/ChatApp.Telemetry/ChatApp.Shared/ChatApp.Shared.csproj", "samples/ChatApp.Telemetry/ChatApp.Shared/"]
COPY ["src/MagicOnion/MagicOnion.csproj", "src/MagicOnion/"]
COPY ["src/MagicOnion.Abstractions/MagicOnion.Abstractions.csproj", "src/MagicOnion.Abstractions/"]
COPY ["src/MagicOnion.Server/MagicOnion.Server.csproj", "src/MagicOnion.Server/"]
COPY ["src/MagicOnion.Server.OpenTelemetry/MagicOnion.Server.OpenTelemetry.csproj", "src/MagicOnion.Server.OpenTelemetry/"]
COPY ["src/MagicOnion.Shared/MagicOnion.Shared.csproj", "src/MagicOnion.Shared/"]
RUN dotnet restore "samples/ChatApp.Telemetry/ChatApp.Server/ChatApp.Server.csproj"
COPY . .
WORKDIR "/src/samples/ChatApp.Telemetry/ChatApp.Server"
RUN dotnet build "ChatApp.Server.csproj" -c Debug -o /app

FROM build AS publish
RUN dotnet publish "ChatApp.Server.csproj" -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ChatApp.Server.dll"]