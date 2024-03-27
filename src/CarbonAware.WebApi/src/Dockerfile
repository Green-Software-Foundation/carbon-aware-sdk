FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

ENV DOTNET_ROLL_FORWARD LatestMajor
# Copy everything from source
COPY . ./
# Use implicit restore to build and publish
RUN dotnet publish CarbonAware.WebApi/src/CarbonAware.WebApi.csproj -c Release -o publish
# Generate OpenAPI spec
WORKDIR /app/CarbonAware.WebApi/src
RUN dotnet tool restore && \
    mkdir -p /app/publish/wwwroot/api/v1 && \
    dotnet tool run swagger tofile --output /app/publish/wwwroot/api/v1/swagger.yaml --yaml /app/publish/CarbonAware.WebApi.dll v1

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Install curl for health check
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl
# Copy artifacts from build-env
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "CarbonAware.WebApi.dll"]
