# Configuration

## Logging

The default LogLevel settings for the application are found in the corresponding `appsettings.json`, which may contain the following section -- see here for additional details on [Logging in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) and on [Logging Providers in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging-providers)

```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
```

To permanently change the LogLevel, just update the `appsettings.json` for the app.
To override a LogLevel at runtime, an environment variable can set the LogLevel value. 
For example to set the Logging:LogLevel:Default LogLevel to Debug: `export Logging__LogLevel__Default="Debug"` 

Example using the CLI:

```sh
cd src/CarbonAware.CLI
export Logging__LogLevel__Default="Debug"
dotnet run -l westus
```

Example using the WebApp:

```sh
cd src/CarbonAware.WebApi
export Logging__LogLevel__Default="Debug"
dotnet run
```

Or, to change the LogLevel for just one run of the app:

```sh
cd src/CarbonAware.WebApi
Logging__LogLevel__Default="Debug" dotnet run
```

## CarbonAware.LocationSources

The `LocationSource` converts named locations to their corresponding geoposition coordinates based on JSON files containing those values.

### Azure locations

To generate a new version of the `src/data/location-sources/azure-regions.json` file, follow these steps:

1. [Install the Azure CLI](https://docs.microsoft.com/en-us/cli/azure/).
2. [Login to your Azure subscription](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli?view=azure-cli-latest).
3. Get a list of Azure regions metadata in the proper format:

    1. ```bash
       az account list-locations --query "[?metadata.latitude != null].{RegionName:name,Latitude:metadata.latitude,Longitude:metadata.longitude }" >> azure-regions.json
       ```

4. Copy the results and save it to `src/data/location-sources/`

## Static Data Files

Data files are stored in the `src/data/data-files` directory.

All files placed in that directory will be copied into the user-interface project (CLI, WebApi, etc) at build time.
