# For OpenAPI document
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS openapi-env
WORKDIR /app
ENV DOTNET_ROLL_FORWARD LatestMajor
COPY . ./
RUN dotnet build CarbonAware.WebApi/src/CarbonAware.WebApi.csproj -o build
WORKDIR /app/CarbonAware.WebApi/src
RUN dotnet tool restore && \
    dotnet tool run swagger tofile --output /app/build/swagger.yaml --yaml /app/build/CarbonAware.WebApi.dll v1


# Builder
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG TARGETARCH
WORKDIR /app
ENV DOTNET_ROLL_FORWARD LatestMajor
# Copy everything from source
COPY . ./
# Use implicit restore to build and publish
RUN dotnet publish CarbonAware.WebApi/src/CarbonAware.WebApi.csproj -a $TARGETARCH -o publish


# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Install curl for health check
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl
# Copy artifacts
WORKDIR /app
COPY --from=build-env /app/publish .
RUN mkdir -p /app/wwwroot/api/v1
COPY --from=openapi-env /app/build/swagger.yaml /app/wwwroot/api/v1/
ENTRYPOINT ["dotnet", "CarbonAware.WebApi.dll"]
