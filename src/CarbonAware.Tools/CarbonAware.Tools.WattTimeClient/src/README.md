# WattTime API Client
This project contains a client library for the [WattTime API](https://www.watttime.org/api-documentation/).  The client contains some business logic except around managing authentication.  It is up to the caller to validate any inputs and outputs as they see fit. 

## Configuration
To use the client library in another project you need to add the following to the project:

In `myProject.csproj`:
```xml
<ItemGroup>
    <ProjectReference Include="..\CarbonAware.Tools.WattTimeClient\CarbonAware.Tools.WattTimeClient.csproj" />
</ItemGroup>
```

Make your WattTime username and password accessible via [configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0) to the application.

EG

```bash
set WattTimeClient__Username=<your username>
set WattTimeClient__Password=<your password>
```

Add and configure the client using dependency injection:

```csharp
using CarbonAware.Tools.WattTimeClient;

services.ConfigureWattTimeClient(configuration);
```

## Usage

Once you have configured the client through dependency injection you can access it in your application by passing the IWattTimeClient interface.

```csharp
public MyController(IWattTimeClient client)
{
    _client = client;
}
```

### Determining Balancing Authority

All WattTime requests require the balancing authority to be specified.  You can do this in the client by using either a `BalancingAuthority` object or passing the balancing authority's abbreviated name as a `string`.

```csharp
    BalancingAuthority ba = await _client.GetBalancingAuthorityAsync("53.349804", "-6.260310");

    string ba = await _client.GetBalancingAuthorityAbbreviationAsync("53.349804", "-6.260310");
```

### Getting Emissions Data

```csharp
List<GridEmissionDataPoint> emissions = await _client.GetDataAsync(ba, "2022-01-01T00:00:00", "2022-01-01T23:59:59");
```

### Getting a 24 hour forecast

```csharp
Forecast forecast = await _client.GetCurrentForecastAsync(ba);
```

### Getting historical emissions data >30 days old

```csharp
Stream historicalZipFileStream = await _client.GetHistoricalDataAsync(ba);
```

### Exception Handling

If WattTime responds with a 4XX or 5XX status code the client will wrap the response and raise a `WattTimeClientException` to be handled by the caller.  Refer to the [current WattTime documentation](https://www.watttime.org/api-documentation/) for the most up-to-date information about possible error codes.

## Running tests

One can run the tests using the `dotnet test` command.

```bash
cd src\CarbonAware.Tools.WattTimeClient.Tests
dotnet test
```