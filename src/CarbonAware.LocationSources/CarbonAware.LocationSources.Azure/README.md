# AzureLocationSource
This project contains a library to convert a Azure region to its corresponding geoposition coordinates. 

## Configuration
In order for this to work, a static json file with all the Azure regions and corresponding latitude/longitude coordinates is required.  

Make sure you have Azure CLI installed to execute the commands below. Instructions on downloading the Azure CLI can be found here - https://docs.microsoft.com/en-us/cli/azure/

1. Login to your Azure subscription: There are several ways to sign in to your account. Follow any of the methods described here - https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli?view=azure-cli-latest

2. Get list of Azure regions metadata: Use the following command to get a list of available Azure regions for the subscription provided: 

```bash 
az account list-locations --query "[].{RegionName:name,Latitude:metadata.latitude,Longitude:metadata.longitude }"
```

The Azure CLI uses the --query parameter to execute a JMESPath query on the results of commands. JMESPath is a query language for JSON, giving you the ability to select and modify data from CLI output. The above command will only select the specified elements from the metadata -regionName, latitude and longitude in this case.

3. Copy the results and save it in a file named 'azure-regions.json'


## Usage

In `myProject.csproj`:
```xml
<ItemGroup>
    <ProjectReference Include="..\CarbonAware.LocationSources.Azure\src\CarbonAware.LocationSources.Azure.csproj" />
</ItemGroup>
```

Once the LocationSource is configured through dependency injection, you can instantiate using

```csharp
services.TryAddSingleton<ILocationSource, AzureLocationSource>();
```
