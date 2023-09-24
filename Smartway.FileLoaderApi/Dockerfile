#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Smartway.FileLoaderApi/Smartway.FileLoaderApi.csproj", "Smartway.FileLoaderApi/"]
RUN dotnet restore "Smartway.FileLoaderApi/Smartway.FileLoaderApi.csproj"
COPY . .
WORKDIR "/src/Smartway.FileLoaderApi"
RUN dotnet build "Smartway.FileLoaderApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Smartway.FileLoaderApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Smartway.FileLoaderApi.dll"]