#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LinkerConsoleApp/LinkerConsoleApp.csproj", "LinkerConsoleApp/"]
RUN dotnet restore "LinkerConsoleApp/LinkerConsoleApp.csproj"
COPY . .
WORKDIR "/src/LinkerConsoleApp"
RUN dotnet build "LinkerConsoleApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinkerConsoleApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LinkerConsoleApp.dll"]