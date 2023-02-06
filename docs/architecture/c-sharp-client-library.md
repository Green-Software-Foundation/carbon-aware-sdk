# C\# Client Library

This document outlines the designs behind the GSF Carbon Aware C# Client
Library.

## Namespace

Given the fact this is going to be a library exposing functionality to
consumers, will use the
[standard](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces)
namespace naming schema:
`<Company>.(<Product>|<Technology>)[.<Feature>][.<Subnamespace>]`. For GSF
CarbonAware SDK this the following schema:

- **Company**: **_GSF_**
- **Product**: **_CarbonAware_**
- **Feature**: **_Models_**, **_Handlers_**, ...

An example of a namespace would be: `namespace GSF.CarbonAware.Models` and a
class (record, interface, ...) that belongs to that namespace would be:

```c#
namespace GSF.CarbonAware.Models;

public record EmissionsData
{
    ....
}
```

The following namespaces are included:

| namespace                     |
| ----------------------------- |
| GSF.CarbonAware.Exceptions    |
| GSF.CarbonAware.Configuration |
| GSF.CarbonAware.Handlers      |
| GSF.CarbonAware.Models        |
| GSF.CarbonAware.Parameters    |

## Features

### Models

There are two main classes that represents the data fetched from the data
sources (i.e `Static Json`, [WattTime](https://www.watttime.org) and
[ElectricityMap](https://www.electricitymaps.com)):

- `EmissionsData`
- `EmissionsForecast`

We will define records that are owned by the library for each of these data
types.

```c#
namespace GSF.CarbonAware.Models;
public record EmissionsData
{
    string Location
    DateTimeOffset Time
    double Rating
    TimeSpan Duration
}
```

```c#
namespace GSF.CarbonAware.Models;
public record EmissionsForecast
{
    DateTimeOffset RequestedAt
    DateTimeOffset GeneratedAt
    IEnumerable<EmissionsData> EmissionsDataPoints
    IEnumerable<EmissionsData> OptimalDataPoints
}
```

The user can expect to either have a primitive type (such as an int) or one of
these specific models as a return type of the **Handlers**.

### Handlers

There will be two handlers for each of the data types returned:

- `EmissionsHandler`
- `ForecastHandler`

Each is responsible for interacting on its own domain. For instance,
EmissionsHandler can have a method `GetAverageCarbonIntensityAsync()` to pull
EmissionsData data from a configured data source and calculate the average
carbon intensity. ForecastHandler can have a method `GetCurrentAsync()`, that
will return a EmissionsForecast instance. (**Note**: The current core
implementation is using async/await paradigm, which would be the default for
library too).

### Parameters

Both handlers require that exact fields be passed in as input. Within the docs
of each library function, we will specifically call out which fields the
function expects to be defined versus which are optional. Internally, we will
handle creating the CarbonAwareParameters object and validating the fields
through that.

### Error Handling

The `CarbonAwareException` class will be used to report errors to the consumer.
It will follow the `Exception` class approach, where messages and details will
be provided as part of error reporting.

## References

<https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/>
