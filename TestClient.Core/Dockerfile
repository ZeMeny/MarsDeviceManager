#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 12000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TestClient.Core/TestClient.Core.csproj", "TestClient.Core/"]
COPY ["MrsDeviceManager.Core/MrsDeviceManager.Core.csproj", "MrsDeviceManager.Core/"]
RUN dotnet restore "TestClient.Core/TestClient.Core.csproj"
COPY . .
WORKDIR "/src/TestClient.Core"
RUN dotnet build "TestClient.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestClient.Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestClient.Core.dll"]