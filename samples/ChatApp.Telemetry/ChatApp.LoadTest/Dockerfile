#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ChatApp.LoadTest/ChatApp.LoadTest.csproj", "ChatApp.LoadTest/"]
RUN dotnet restore "ChatApp.LoadTest/ChatApp.LoadTest.csproj"
COPY . .
WORKDIR "/src/ChatApp.LoadTest"
RUN dotnet build "ChatApp.LoadTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatApp.LoadTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatApp.LoadTest.dll"]