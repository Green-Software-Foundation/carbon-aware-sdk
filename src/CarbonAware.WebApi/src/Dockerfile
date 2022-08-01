#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CarbonAware.WebApi/src/CarbonAware.WebApi.csproj", "CarbonAware.WebApi/"]
COPY ["CarbonAware/src/CarbonAware.csproj", "CarbonAware/"]
COPY ["CarbonAware.Plugins.BasicJsonPlugin/src/CarbonAware.Plugins.BasicJsonPlugin.csproj", "CarbonAware.Plugins.BasicJsonPlugin/"]
RUN dotnet restore "CarbonAware.WebApi/CarbonAware.WebApi.csproj"
COPY . .
WORKDIR "/src/CarbonAware.WebApi/src"
RUN dotnet build "CarbonAware.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CarbonAware.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarbonAware.WebApi.dll"]