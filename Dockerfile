# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY ./src/dotnet/ ./
COPY ./entrypoint.sh ./


#WORKDIR /src/dotnet

RUN dotnet publish  ./CarbonAware.CLI/CarbonAware.CLI.csproj -c Release -o out --no-self-contained
RUN cp ./CarbonAware.CLI/carbon-aware.json out
RUN cp -r  ./data/data-files/ out

RUN cp ./entrypoint.sh out


# Label the container
#LABEL maintainer="Green-Software-Foundation"
LABEL repository="https://github.com/Green-Software-Foundation/carbon-aware-sdk"
LABEL homepage="https://github.com/Green-Software-Foundation/carbon-aware-sdk"

# Label as GitHub action
LABEL com.github.actions.name="CarbonAware"
LABEL com.github.actions.description="A Github Action to enable the creation of carbon aware applications, applications that do more when the electricity is clean and do less when the electricity is dirty"
LABEL com.github.actions.icon="sliders"
LABEL com.github.actions.color="purple"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/runtime:6.0
COPY --from=build-env /out .
RUN apt-get update && apt-get install jq -y

RUN chmod +x entrypoint.sh
#ENTRYPOINT ["/CarbonAwareCLI"]
ENTRYPOINT ["/entrypoint.sh"]