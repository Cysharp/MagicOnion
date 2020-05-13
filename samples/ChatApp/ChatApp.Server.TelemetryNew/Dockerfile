FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
EXPOSE 12345
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["ChatApp.Server.Telemetry/ChatApp.Server.Telemetry.csproj", "ChatApp.Server.Telemetry/"]
RUN dotnet restore "ChatApp.Server.Telemetry/ChatApp.Server.Telemetry.csproj"
COPY . .
WORKDIR "/src/ChatApp.Server.Telemetry"
RUN dotnet build "ChatApp.Server.Telemetry.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ChatApp.Server.Telemetry.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ChatApp.Server.Telemetry.dll"]