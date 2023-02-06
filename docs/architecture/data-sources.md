# Data Sources

Data sources allow developers easily integrate different data providers into the
carbon aware SDK ([WattTime](https://www.wattime.org),
[ElectricityMaps](https://www.electricitymaps.com/), etc) to be made available
to all higher-level user-interfaces (WebAPI, CLI, etc), while avoiding the
details of how to interact with any specific provider.

## Data Sources' Responsibility

Data sources act as the data ingestion tier for the SDK, handling the retrieval
of data from a given data provider. They contain specific knowledge about the
data provider they access, such as flags used in requests, fields that come back
in responses, special use cases etc. They also handle any external calls that
must be made to access the data provider. While helper clients can be built to
handle these calls, only the data source should have access to, and knowledge
of, that client.

- For example, the WattTimeDataSource has a reference to a private
  WattTimeClient within it's implementation. The WattTimeClient handles the HTTP
  GET/POST calls to WattTime and the data source invokes the client once it has
  processed the request, and then processes the response before returning a
  final result.

### Aggregator <-> Data Source Contract

In order for the SDK to support different data sources, there is a defined
contract between the Aggregators and the Data tier. The Aggregator acts as the
"Business Logic" of the application so it needs a standard way of requesting
data from the data source and a standard response in return. This means that
each data source is responsible for:

- Pre-processing any arguments passed to it from the Aggregator to create the
  expected request for the data provider.
- Post-processing the data provider result to create the expected return type
  for the Aggregator

#### Post-Processing Caveat

Post-processing should only ensure the types are what is expected and to fix any
inconsistencies or issues that may be known to that specific data source. This
post-processing **should not** do any extra data operations beyond those
required to fulfill the Aggregator request ( i.e., averaging, min/max ops etc.).
In other words, the data source should only manipulate data for the aim of
returning _valid\*_ data in the boundaries requested by the Aggregator

\* What constitutes _valid_ data varies between data sources. It may be the case
that some data sources don't handle time boundaries well so extra processing may
be required to ensure the data returned is what the aggregator expects assuming
it was any data source and that those edge cases would be handled properly.

## Creating a New Data Source

Each new data source should be a new .NET project under the
`CarbonAware.DataSources` namespace and corresponding directory. This project
should have a reference to the `CarbonAware` project, and include the
`Microsoft.Extensions.DependencyInjection` package. It should also be added to
the solution. We have provided a command snippet below:

```bash
cd src
dotnet new classlib --name CarbonAware.DataSources.MyNewDataSource -o CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src
dotnet sln add CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src/CarbonAware.DataSources.MyNewDataSource.csproj
dotnet add CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src/CarbonAware.DataSources.MyNewDataSource.csproj reference CarbonAware/src/CarbonAware.csproj
cd CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src
dotnet add package Microsoft.Extensions.DependencyInjection
```

### Adding/Extending a Data Source Interface

Each new data source should extend from a generic data source interface. A data
source interface defines all the parameters and functions that any data source
that falls under it's purview must define/implement. By defining the interface,
it allows the SDK to switch between the set of data sources seamlessly because
they all share the same input functions and output types.

Currently there are 2 data source interfaces defined - `IEmissionsDataSource`
and `IForecastDataSource` - which provides functionality for retrieving actual
and forecasted carbon intensity data respectively. A new data source interface
should be defined only when there is a new general area of calculation that is
being introduced to the SDK.

```csharp
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
namespace CarbonAware.DataSources.MyNewDataSource;
public class MyNewDataSource: IEmissionsDataSource
{
    ...
}
```

### Add Dependency Injection Configuration

The SDK uses dependency injection to load registered data sources based on set
environment variables. For a data source to be registered, it need to have a
Service Collection Extension defined. To do so, add a `Configuration` directory
in your data source project and create a new ServiceCollectionExtensions file.
We have provided a command snippet below:

```sh
cd src/CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/src
mkdir Configuration
touch Configuration\ServiceCollectionExtensions.cs
```

Using the skeleton below, add the data source specific configuration and
implementation instances to the service collection.

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

### Register the New Data Source

Once the data source's ServiceCollectionExtensions is configured, it can be
registered as an available data source for the SDK by adding to the switch
statement found in the AddDataSourceService function of
[this file](../../src/CarbonAware.DataSources/CarbonAware.DataSources.Registration\Configuration\ServiceCollectionExtensions.cs).
Note you will need to add a new enum type to the `DataSourceType`
[enum file](../../src/CarbonAware.DataSources/CarbonAware.DataSources.Registration/Configuration/DataSourceType.cs)
to reference in the switch statement.

```csharp
    switch (dataSourceType)
    {
        ...
        case DataSourceType.MyNewDataSourceEnum:
        {
            services.AddMyNewDataSource();
            break;
        }
        ...
    }
```

### Adding Tests

Each new data source is expected to come with a robust unit test suite that
ensures that the main flows and edge cases are properly handled. This also
ensures that the SDK can switch seamlessly between data sources and the
experiences up the stack remains consistent and helpful to the user.

The unit tests should be added as a new project under the data source's test
directory:
`CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/test`. Be sure
to include a reference to the data source's project and add it to the solution.
We have provided a command snippet below:

```sh
cd src
dotnet new nunit --name CarbonAware.DataSources.MyNewDataSource.Tests -o CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/test
dotnet sln CarbonAwareSDK.sln add CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource/test/CarbonAware.DataSources.MyNewDataSource.Tests.csproj
cd CarbonAware.DataSources/CarbonAware.DataSources.MyNewDataSource
dotnet add test/CarbonAware.DataSources.MyNewDataSource.Tests.csproj reference src/CarbonAware.DataSources.MyNewDataSource.csproj
```

### Try it Out

You are now ready to try out your new data source! If you added a new
`IEmissionsDataSource`, you can configure it using the `EmissionsDataSource`
setting:

```bash
DataSources__EmissionsDataSource="MyNewDataSource"
DataSources__Configurations__MyNewDataSource__Proxy__UseProxy=true
```

Both the WebAPI and the CLI read the env variables in so once set, you can spin
up either and send requests to get data from the new data source.
