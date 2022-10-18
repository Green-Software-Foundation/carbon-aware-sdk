# GSF Carbon Intensity Library

This document helps to have the concept on how GSF Carbon Intensity SDK library was conceived. 

## Namespace

Given the fact this is going to be a library exposing functionality to consumers, it is [recommended](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces) to use the following namespace naming schema: `<Company>.(<Product>|<Technology>)[.<Feature>][.<Subnamespace>]`. For GSF CarbonAware SDK this the following schema:

- **Company**: ***GSF***
- **Product**: ***CarbonIntensity***
- **Feature**: ***Model***, ***Handlers***, ...

An example of a namespace would be: `namespace GSF.CarbonIntensity.Model` and a class (record, interface, ...) that belongs to that namespace would be:

```c#
namespace GSF.CarbonIntensity.Model;

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
| GSF.CarbonIntensity.Model |
| GSF.CarbonIntensity.Parameters |


## Features

### Model

There are two main classes that represents the data fetched from the data sources (i.e `Static Json`, [WattTime](https://www.watttime.org) and [ElectricityMap](https://www.electricitymaps.com)):

- `EmissionsData`
- `EmissionsForecast`

We propose to keep `EmissionsData` and to simplify `EmissionsForecast`, removing extra metadata which we believe library users will not need (as they will have that data from the call to the library already). In place of `EmissionsForecast`, we have created the `ForecastData` record.

The user can expect to either have a primitive type (such as an int) or one of these specific models as a return type of the  **Handlers**.

### Handlers

There will be two handlers for each of the data types returned:

- `EmissionsHandler`
- `ForecastHandler`

Each would be responsible of interacting on its own domain. For instance `EmissionsHandler` can have a method `GetAverageCarbonIntensity` to pull `EmissionsData` data from a configured data source and calculate the average carbon intensity.
(Note: The current core implementation is using `async/await` paradigm, which would be the default for GSF SDK library too).

### Parameters

Both handlers require that a `CarbonAwareParameters` object be passed in as input. We will be providing users with a `CarbonAwareParametersBuilder` class that will make it easy to create, add fields, and then build it into the required parameters object. Within the docs of each library function, we will specifically call out which fields the function expects to be defined in the parameters object (which is validated down the line by the Aggregator).

### Error Handling

`CarbonIntensityException` class will be used to report errors to the consumer. It would follow the `Exception` class approach, where messages and details will be provided as part of error reporting.

### Dependency Injection

Using C# practices on how to register services, the library would be available through `Microsoft.Extensions.DependencyInjection` extension. For instance a consumer would be able to call:

```c#
// Using DI Services to register GSF SDK library
services.AddCarbonIntensityServices(configuration);
```
```c#
// An application Consumer construct should inject a GSF handler like the following example
public class ConsumerApp(IEmissionsHandler handler, ILogger<ConsumerApp> logger)
{
    ....
    this._handler = handler;
    this._logger = logger;
    ....
}

public Task<double> GetRating()
{
    ....
    return await this._handler.GetEmissionsDataAsync(...).Rating;
}
```

## References

https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
