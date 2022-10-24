# GSF Carbon Intensity Library

This document helps to have the concept on how GSF Carbon Intensity SDK library was conceived. 

## Namespace

Given the fact this is going to be a library exposing functionality to consumers, it is [recommended](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces) to use the following namespace naming schema: `<Company>.(<Product>|<Technology>)[.<Feature>][.<Subnamespace>]`. For GSF CarbonAware SDK this the following schema:

- **Company**: ***GSF***
- **Product**: ***CarbonIntensity***
- **Feature**: ***Models***, ***Handlers***, ...

An example of a namespace would be: `namespace GSF.CarbonIntensity.Models` and a class (record, interface, ...) that belongs to that namespace would be:

```c#
namespace GSF.CarbonIntensity.Models;

public record EmissionsData
{
    ....
}
```

The following namespaces are considered:

| namespace   |
| ----------- |
| GSF.CarbonIntensity.Exceptions |
| GSF.CarbonIntensity.Configuration |
| GSF.CarbonIntensity.Handlers |
| GSF.CarbonIntensity.Models |
| GSF.CarbonIntensity.Parameters |


## Features

### Models

There are two main classes that represents the data fetched from the data sources (i.e `Static Json`, [WattTime](https://www.watttime.org) and [ElectricityMap](https://www.electricitymaps.com)):

- `EmissionsData`
- `EmissionsForecast`

We propose to keep `EmissionsData` and to simplify `EmissionsForecast`, removing extra metadata which we believe library users will not need (as they will have that data from the call to the library already).
```c#
namespace GSF.CarbonIntensity.Models;
public record EmissionsData
{
    string Location 
    DateTimeOffset Time
    double Rating
    TimeSpan Duration
}
```
```c#
namespace GSF.CarbonIntensity.Models;
public record EmissionsForecast
{
    DateTimeOffset RequestedAt
    DateTimeOffset GeneratedAt
    IEnumerable<EmissionsData> EmissionsData
    IEnumerable<EmissionsData> OptimalDataPoints
}
```


The user can expect to either have a primitive type (such as an int) or one of these specific models as a return type of the  **Handlers**.

### Handlers

There will be two handlers for each of the data types returned:
- EmissionsHandler
- ForecastHandler
Each would be responsible of interacting on its own domain. For instance, EmissionsHandler can have a method GetAverageCarbonIntensity() to pull EmissionsData data from a configured data source and calculate the average carbon intensity. ForecastHandler can have a method GetCurrent(), that will return a EmissionsForecast instance.
(**Note**: The current core implementation is using async/await paradigm, which would be the default for GSF SDK library too).

### Parameters

Both handlers require that exact fields be passed in as input. Within the docs of each library function, we will specifically call out which fields the function expects to be defined versus which are optional. Internally, we will handle creating the CarbonAwareParameters object and validating the fields through that.

### Error Handling

`CarbonIntensityException` class will be used to report errors to the consumer. It would follow the `Exception` class approach, where messages and details will be provided as part of error reporting.

### Dependency Injection

In order to get access to the handlers, using a common practice with C# is through `Microsoft.Extensions.DependencyInjection` extensions. This way the whole life cycle of the handler instance is managed by the container’s framework, and it would help to isolate the concrete implementation from the user facing interface. For instance, a consumer would be able to call extensions as:
```c#
// Using DI Services (Emissions) to register GSF SDK library
services.AddEmissionsServices(configuration);
```
```c#
// An application Consumer construct should inject a GSF handler like the following example
class ConsumerClass(IEmissionsHandler handler, ILogger<ConsumerClass> logger)
{
    ....
    this._handler = handler;
    this._logger = logger;
    ....
}
```

And the usage of a method for IEmissionsHandler

```c#
async Task<double> GetRating()
{
    ...
    return await this._handler.GetAverageCarbonIntensity(…);
}
```
Another functionality of the application could just do Forecast data. So, it would be a matter of following the same pattern:

```c#
// Using DI Services (Forecast) to register GSF SDK library
services.AddForecastServices(configuration);
```

```c#
class ForecastApp(IForecastHandler handler)
{
    ...
    this._handler = handler;
}
```
And the usage of a method for IForecastHandler:

```c#
async Task<EmissionsData> GetOptimal(…)
{
    ...
    return await this._handler.GetCurrentAsync()...).OptimalDataPoints.First();
}
```

This way it would fit within the same stack as the rest of the SDK is implemented. Also, it would be easier to integrate later when the current consumers (CLI/WebApi) should be moved to use the library.


## References

https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
