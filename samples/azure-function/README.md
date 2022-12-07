# Use Carbon Aware SDK with an Azure Function

## Overview
The two samples included showcase the Azure Functions tooling for the Carbon Aware SDK [C# Class Library](../../docs/architecture/c%23-client-library.md). The emissions function app implements GetAverageCarbonIntensity and the forecast function app implements GetCurrentForecast. GetAverageCarbonIntensity uses the EmissionsHandler to return the Carbon Intensity rate of a location for a specific timespan. GetCurrentForecast uses the ForecastHandler to yield the optimal time of a specified location and duration. The functions can run locally for debugging or be deployed to Azure.

## Azure Function Dependency Injection
The Carbon Aware SDK is included in the function .csproj file by [creating and adding the SDK as a package](../../docs/packaging.md#included-scripts).  The [Startup.cs](Startup.cs) file uses dependency injection to access the handlers in the library. The following code initializes the C# Library:

_For the emissions function app:_

```C#
 public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services
            .AddEmissionsServices(configuration)
        }
```

_For the forecast function app:_
```C#
 public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services
            .AddForecastServices(configuration);
        }
```


## Run Function Locally
Both azure function apps can be run locally without needing an azure subscription. The process for running both is the same, as they use the same configuration, just call different paths within the SDK.

### Prerequisites
[.NET Core SDK](https://dotnet.microsoft.com/download)

[Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Start Function
To run and debug locally, update the [appsettings.json](appsettings.json) file to include the desired [configuration](../../docs/configuration.md).

In the app folder (`samples/azure-function/emissions-azure-function` or `samples/azure-function/forecast-azure-function`), run the command:  ```func start```

After the function has compiled and is running, the URLs to the functions will be presented.  

_Example call for Emissions azure function_

The following example will retrieve the Average Carbon Intensity.  For this example, query parameters were used, but the values could also be sent in the body of the request.

```
curl --location --request GET 'http://localhost:7071/api/GetAverageCarbonIntensity?startDate=2022-03-01T15:30:00Z&endDate=2022-03-01T18:30:00Z&location=eastus'
```

_Example call for Forecast azure function_

The following example will call the Current Forecast route.  If an error is returned, update the start and end dates.  The request can use either the request body or query parameters.

```
curl --location --request GET 'http://localhost:7071/api/GetCurrentForecast' \
--header 'Content-Type: application/json' \
--data-raw '{
    "startDate": "2022-11-02T15:30:00Z",
    "endDate": "2022-11-02T18:30:00Z",
    "location" : "eastus",
    "duration": 15
}'
```

## Deploy to Azure
If you have an azure subscription, you can also deploy these functions to Azure.

### Prerequisites

You must have the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) or [Azure PowerShell](https://learn.microsoft.com/en-us/powershell/azure/install-az-ps) installed locally to be able to publish to Azure.

[Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Create Function App
Log in to Azure:  ```az login```

Once the correct subscription is selected run the following script to create a new function app:

```
# Function app and storage account names must be unique.

# Variable block
let "randomIdentifier=$RANDOM*$RANDOM"
location="eastus"
resourceGroup="carbon-aware-$randomIdentifier"
storage="casaccount$randomIdentifier"
functionApp="carbon-aware-functionapp-$randomIdentifier"
skuStorage="Standard_LRS"
functionsVersion="3"

# Create a resource group
echo "Creating $resourceGroup in "$location"..."
az group create --name $resourceGroup --location "$location"

# Create an Azure storage account in the resource group.
echo "Creating $storage"
az storage account create --name $storage --location "$location" --resource-group $resourceGroup --sku $skuStorage

# Create a serverless function app in the resource group.
echo "Creating $functionApp"
az functionapp create --name $functionApp --storage-account $storage --consumption-plan-location "$location" --resource-group $resourceGroup --functions-version $functionsVersion
```

### Publish Functions 
Update the [appsettings.json](appsettings.json) file to include the desired [configuration](../../docs/configuration.md).

To publish the function code to a function app in Azure, use the publish command in the samples/azure-function folder:

```
func azure functionapp publish $functionApp
```

## References

[Azure Functions developer guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference?tabs=blob)

[Use dependency injection in .NET Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection)

[Run functions locally](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#start)

[Create a function app for serverless code execution](https://learn.microsoft.com/en-us/azure/azure-functions/scripts/functions-cli-create-serverless?source=recommendations)