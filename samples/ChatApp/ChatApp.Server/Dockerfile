FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
#FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
EXPOSE 12345
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ChatApp.Server/ChatApp.Server.csproj", "ChatApp.Server/"]
RUN dotnet restore "ChatApp.Server/ChatApp.Server.csproj"
COPY . .
WORKDIR "/src/ChatApp.Server"
RUN dotnet build "ChatApp.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ChatApp.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ChatApp.Server.dll"]