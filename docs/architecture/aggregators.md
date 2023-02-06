# Aggregators

Aggregators live in between the consumer and data tiers, containing the business
logic for the SDK. They have knowledge of the different types of data providers
and how to aggregate the resulting data.

## Aggregators' Responsibility

Aggregators are responsible for taking in consumer requests, calling the
specified data source, and performing any necessary logic before returning the
result to the consumer. Each Aggregator is responsible for handling requests
specific to a functionality. For eg: 'EmissionsAggregator' handles requests to
get actual carbon emissions data from the underlying datasource, whereas
`ForecastAggregator` is responsible for handling requests to get forecasted
carbon intensity data from the underlying data source.

### Consumer <-> Aggregator Contract

Each aggregator can support a wide variety of consumer requests whose arguments
may be required to access data from one or more data sources. The input to the
aggregator must be generic enough to handle those cases, but specific enough to
allow enforcement of required fields and validations (i.e., a list field cannot
be empty, a time field cannot be in the past etc.). The `CarbonAwareParameters`
class handles these concerns for both `EmissionsAggregator` and
`ForecastAggregator`. Each public method in the aggregator receives an instance
of this "Parameters" class. Future aggregators will create their own
"Parameters" class to manage their argument needs.

## Carbon Aware Parameters

The `CarbonAwareParameters` class allows the user to pass in a unique parameter
instance to the public methods in the Aggregators with the specific parameters
needed by that call. The list of allowed parameters is defined in the class and
includes

- SingleLocation
- MultipleLocations
- Start
- End
- RequestedAt
- Duration

### Parameter Display Names

The display name of each parameter can be overriden using the public setter. By
default, each parameter display name is set to the variable name (ex:
`Start = "start"`). The parameter display names are used when creating the
validation error messages. Overriding them is useful in situations where the
variables the user is using for input don't exactly match the default display
name of the parameter (e.g. the user variable in the controller is
`periodStartTime` instead of `startTime`). That way, when the error is thrown to
the user, the parameter names will match the users' expectation

To do the override, define a class that inherits from
CarbonAwareParametersBaseDTO and uses the [FromQuery(Name =
"myAwesomeDisplayName")] or [JsonPropertyName("myAwesomeDisplayName")]
attribute. A second (less recommended) option is to pass the optional arg
Dictionary<string, string>? displayNameMap when you are directly creating the
object. With either option, the SDK will handle updating references internally.

### Required Properties

The first core check the parameters class does is validating that required
parameters are defined. By default, all parameters are considered optional.
Calling the `SetRequiredProperties(...)` function with the desired arguments
sets the required parameters for the instance.

```csharp
    /// <summary>
    /// Accepts any PropertyNames as arguments and sets the associated property 
    /// as required for validation.
    /// </summary>
    public void SetRequiredProperties(params PropertyName[] requiredProperties)
```

### Validations

The second core check the parameters class does is enforcing validations on the
parameters themselves. Some common examples include

- Relationship validations: _`start < end` must be true_
- Content validations: _`list.Any()` must be true for list fields_

Calling the `SetValidations(...)` function with the desired arguments sets the
validations for the instance.

```csharp
    /// <summary>
    /// Accepts any ValidationName as arguments and sets the associated 
    /// validation to check.
    /// </summary>
    public void SetValidations(params ValidationName[] validationNames)
```

### Validate

Calling the `Validate(...)` function will validate (1) required parameters and
(2) specified validations. Currently, the only validation we check is whether
`start` is before `end`.

If no errors are thrown, the function simply returns. If any validation errors
are found, they are packaged into a single `ArgumentException` error with each
being part of the `data` dictionary.

```csharp
    /// <summary>
    /// Validates the properties and relationships between properties. 
    /// Any validation errors found are packaged into an
    /// ArgumentException and thrown. If there are no errors, simply 
    /// returns void.
    /// </summary>
    public void Validate()
```

### Getters With Default Fallbacks

Certain parameters have special getters that allow you to define a fallback
default value if the parameter is null. This can be useful in cases where a
parameter is optional, so you want to get it if it was defined by the user, or
otherwise fallback to a specific default. These include `Start`, `End`,
`Requested`,and `Duration`

```csharp
   DateTimeOffset StartOrDefault(DateTimeOffset defaultStart)
   DateTimeOffset EndOrDefault(DateTimeOffset defaultEnd)
   DateTimeOffset RequestedOrDefault(DateTimeOffset defaultRequested)
   TimeSpan DurationOrDefault
```
