# Data Sources

Data sources allow developers easily integrate different data providers into the carbon aware SDK ([WattTime](https://www.wattime.org), [ElectricityMap](https://static.electricitymap.org), etc) to be made available to all higher level user-interfaces (WebAPI, CLI, etc), while avoiding the details of how to interact with any specific provider.

## Creating a New Data Source

To create a new data source, create a new dotnet project with the following steps:

```sh
cd src
dotnet new classlib --name CarbonAware.DataSources.MyNewDataSource -o CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src
dotnet sln add CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src/CarbonAware.DataSources.MyNewDataSource.csproj
dotnet add CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src/CarbonAware.DataSources.MyNewDataSource.csproj reference CarbonAware/src/CarbonAware.csproj
cd CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src
dotnet add package Microsoft.Extensions.DependencyInjection
```

Add unit tests:

```sh
cd src
dotnet new nunit --name CarbonAware.DataSources.MyNewDataSource.Tests -o CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/test
dotnet sln CarbonAwareSDK.sln add CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/test/CarbonAware.DataSources.MyNewDataSource.Tests.csproj
cd CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource
dotnet add tests/CarbonAware.DataSources.MyNewDataSource.Tests.csproj reference src/CarbonAware.DataSources.MyNewDataSource.csproj
```

Create a Service Collection Extension in the `Configuration` directory:

```sh
cd src/CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src
mkdir Configuration
touch Configuration\ServiceCollectionExtensions.cs
```

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.MyNewDataSource.Configuration;

public static class ServiceCollectionExtensions 
{
    public static void AddMyNewDataSource(this IServiceCollection services)
    {
        // ... register your data source with the IServiceCollection instance
    }
}
```
