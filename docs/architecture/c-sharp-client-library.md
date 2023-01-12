# C\# Client Library

This document outlines the designs behind the GSF Carbon Aware C# Client Library. 

## Namespace

Given the fact this is going to be a library exposing functionality to consumers, will use the [standard](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces) namespace naming schema: `<Company>.(<Product>|<Technology>)[.<Feature>][.<Subnamespace>]`. For GSF CarbonAware SDK this the following schema:

- **Company**: ***GSF***
- **Product**: ***CarbonAware***
- **Feature**: ***Models***, ***Handlers***, ...

An example of a namespace would be: `namespace GSF.CarbonAware.Models` and a class (record, interface, ...) that belongs to that namespace would be:

```c#
namespace GSF.CarbonAware.Models;

public record EmissionsData
{
    ....
}
```

The following namespaces are included:

| namespace   |
| ----------- |
| GSF.CarbonAware.Exceptions |
| GSF.CarbonAware.Configuration |
| GSF.CarbonAware.Handlers |
| GSF.CarbonAware.Models |
| GSF.CarbonAware.Parameters |


## Features

### Models

There are two main classes that represents the data fetched from the data sources (i.e `Static Json`, [WattTime](https://www.watttime.org) and [ElectricityMap](https://www.electricitymaps.com)):

- `EmissionsData`
- `EmissionsForecast`

We will define records that are owned by the library for each of these data types.
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

The user can expect to either have a primitive type (such as an int) or one of these specific models as a return type of the  **Handlers**.

### Handlers

There will be two handlers for each of the data types returned:
- `EmissionsHandler`
- `ForecastHandler`

Each is responsible for interacting on its own domain. For instance, EmissionsHandler can have a method `GetAverageCarbonIntensityAsync()` to pull EmissionsData data from a configured data source and calculate the average carbon intensity. ForecastHandler can have a method `GetCurrentForecastAsync()`, that will return a EmissionsForecast instance.
(**Note**: The current core implementation is using async/await paradigm, which would be the default for library too).

### Parameters

Both handlers require that exact fields be passed in as input. Within the docs of each library function, we will specifically call out which fields the function expects to be defined versus which are optional. Internally, we will handle creating the CarbonAwareParameters object and validating the fields through that.

## Carbon Aware Parameters
The `CarbonAwareParameters` class allows the user to pass in a unique parameter instance to the public methods in the Handlers with the specific parameters needed by that call. 
The list of allowed parameters is defined in the class and includes
- SingleLocation
- MultipleLocations
- Start
- End
- RequestedAt
- Duration

### Parameter Display Names
The display name of each parameter can be overriden using the public setter. By default, each parameter display name is set to the variable name (ex: `Start = "start"`). The parameter display names are used when creating the validation error messages. Overriding them is useful in situations where the variables the user is using for input don't exactly match the default display name of the parameter (e.g. the user variable in the controller is `periodStartTime` instead of `startTime`). That way, when the error is thrown to the user, the parameter names will match the users' expectation

To do the override, define a class that inherits from CarbonAwareParametersBaseDTO and uses the [FromQuery(Name = "myAwesomeDisplayName")] or [JsonPropertyName("myAwesomeDisplayName")] attribute. A second (less recommended) option is to pass the optional arg Dictionary<string, string>? displayNameMap when you are directly creating the object.  With either option, the SDK will handle updating references internally.

### Required Properties
The first core check the parameters class does is validating that required parameters are defined. By default, all parameters are considered optional. Calling the `SetRequiredProperties(...)` function with the desired arguments sets the required parameters for the instance.
```csharp
    /// <summary>
    /// Accepts any PropertyNames as arguments and sets the associated property as required for validation.
    /// </summary>
    public void SetRequiredProperties(params PropertyName[] requiredProperties)
```

### Validations
The second core check the parameters class does is enforcing validations on the parameters themselves. Some common examples include
- Relationship validations: _`start < end` must be true_
- Content validations: _`list.Any()` must be true for list fields_

Calling the `SetValidations(...)` function with the desired arguments sets the validations for the instance.
```csharp
    /// <summary>
    /// Accepts any ValidationName as arguments and sets the associated validation to check.
    /// </summary>
    public void SetValidations(params ValidationName[] validationNames) 
```

### Validate
Calling the `Validate(...)` function will validate (1) required parameters and (2) specified validations. Currently, the only validation we check is whether `start` is before `end`.

If no errors are thrown, the function simply returns. If any validation errors are found, they are packaged into a single  `ArgumentException` error with each being part of the `data` dictionary.
```
    /// <summary>
    /// Validates the properties and relationships between properties. Any validation errors found are packaged into an
    /// ArgumentException and thrown. If there are no errors, simply returns void. 
    /// </summary>
    public void Validate()
 ```

 ### Getters With Default Fallbacks
 Certain parameters have special getters that allow you to define a fallback default value if the parameter is null. This can be useful in cases where a parameter is optional, so you want to get it if it was defined by the user, or otherwise fallback to a specific default. These include `Start`, `End`, `Requested`,and `Duration`
 ```
    DateTimeOffset StartOrDefault(DateTimeOffset defaultStart)
    DateTimeOffset EndOrDefault(DateTimeOffset defaultEnd)
    DateTimeOffset RequestedOrDefault(DateTimeOffset defaultRequested)
    TimeSpan DurationOrDefault

 ```


### Error Handling

The `CarbonAwareException` class will be used to report errors to the consumer. It will follow the `Exception` class approach, where messages and details will be provided as part of error reporting.

## References

https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
