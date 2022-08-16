FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything from source
COPY . ./
# Use implicit restore to build and publish
RUN dotnet publish CarbonAware.WebApi/src/CarbonAware.WebApi.csproj -c Release -o publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "CarbonAware.WebApi.dll"]
