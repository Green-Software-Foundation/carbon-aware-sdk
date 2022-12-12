# Find the Dockerfile at this URL
# https://github.com/Azure/azure-functions-docker/blob/dev/host/4/bullseye/amd64/dotnet/dotnet-inproc/dotnet.Dockerfile

FROM mcr.microsoft.com/azure-functions/dotnet:4.0 AS base
WORKDIR /home/site/wwwroot

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY ["src/", "data/src/"]
COPY ["scripts/", "data/scripts/"]
COPY ["samples/", "data/samples/"]

WORKDIR /data
RUN scripts/package/create_packages.sh src/CarbonAwareSDK.sln /packages && \
    dotnet restore "samples/azure-function/function.csproj" && \
    scripts/package/add_packages.sh samples/azure-function/function.csproj /packages && \
    dotnet build "samples/azure-function/function.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "samples/azure-function/function.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
