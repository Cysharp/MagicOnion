FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 12345
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ChatApp.Server/ChatApp.Server.csproj", "ChatApp.Server/"]
COPY ["ChatApp.Shared/ChatApp.Shared.csproj", "ChatApp.Shared/"]
RUN dotnet restore "ChatApp.Server/ChatApp.Server.csproj"
COPY . .
WORKDIR "/src/ChatApp.Server"
RUN dotnet build "ChatApp.Server.csproj" -c Debug -o /app

FROM build AS publish
RUN dotnet publish "ChatApp.Server.csproj" -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ChatApp.Server.dll"]