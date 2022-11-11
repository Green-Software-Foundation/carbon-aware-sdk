# 7. Data Source Interfaces

## Status

Accepted

## Date 

2022-11-1

## Context

Data sources can meet the needs of multiple use-cases, but all data sources should not be expected to meet all needs. Currently, the `JsonDataSource` throws a `NotImplementedException` for forecast-related methods required by the `ICarbonAwareDataSource` interface. As functionality expands in this way it becomes harder maintain data sources and it prevents programmatic understanding of which methods are actually available to the user.

For example, if someone wanted to expand functionality to include access to power generation sources (coal, hydro, solar, etc.), such a change breaks existing data sources and likely forces them into a similar workaround of raising exceptions to meet the new interface.  Users may similarly start to see a `power-consumption` command in the CLI that throws errors for the data source they are using.

## Decision

The project will strive toward the [interface segregation principle](https://en.wikipedia.org/wiki/Interface_segregation_principle) of [SOLID](https://en.wikipedia.org/wiki/SOLID) design by using separate interfaces for unique [roles](https://martinfowler.com/bliki/RoleInterface.html) that a data source may serve. This is often signalled by a different model/schema being returned by the methods.

A single data sources can implement multiple interfaces.

## Consequences

### Current Implementation

#### Interfaces

To align with this decision, the `ICarbonIntensityDataSource` interface would be broken into two interfaces:  

- `IEmissionsDataSource` – for data sources of measured emissions data  
- `IForecastDataSource` – for data sources of forecasted emissions data

#### Aggregators

There is also no need for these data sources to have a shared aggregator as none of the existing aggregator functions leverage both types of data together.  Thus, there should be two corresponding aggregators:

- `EmissionsAggregator`
- `ForecastAggregator`

Each aggregator should have its own parameters class, however the appropriate way to split up the existing `CarbonAwareParameters` class is less straight-forward due to the large amount of shared code. How to split up this class should be the subject of its own ADR, and it should continue to be shared by both aggregators until an agreed upon design has been reached.

#### Configuration

Each data source type should be independently configurable. So an example config of

```json
{
  "carbonAwareVars": {
    "carbonIntensityDataSource": "WattTime",
    // ...
  },
  // ...
}
```

becomes the following with no user-facing changes since WattTime implements both interfaces:

```json
{
  "carbonAwareVars": {
    "emissionsDataSource": "WattTime",
    "forecastDataSource": "WattTime",
    // ...
  },
  // ...
}
```

But it now becomes possible to configure different sources for each type.

```json
{
  "carbonAwareVars": {
    "emissionsDataSource": "JSON",
    "forecastDataSource": "WattTime",
    // ...
  },
  // ...
}
```

**Other Config Considerations**
*When no data source is specified...*
Use a default data source following the [null object pattern](https://en.wikipedia.org/wiki/Null_object_pattern) to provide empty, but strongly-typed responses.

*When the data source type is not implemented by the specified data source...*
Throw an exception, alerting the operator to improper configuration.

### Future Implications

*New Aggregators*
A hypothetical feature which returned the differences between forecasted emissions and measured emissions would implement a `ForecastEmissionsAggregator` to get the data from both sources and do the calculations.

*New Interfaces*
A hypothetical feature which exposed power generation data would create an `IPowerGenerationDataSource` which any data source with access to such data could implement.

It would be configured by operators the same way as existing data source types, by looking up the matching name of the interface (without the leading "I") in the `carbonAwareVars` section of the config.

```json
{
  "carbonAwareVars": {
    "emissionsDataSource": "JSON",
    "forecastDataSource": "WattTime",
    "powerGenerationDataSource": "MyNewDataSourceName"
    // ...
  },
  // ...
}
```

Features would be exposed to consumers via a `PowerGenerationAggregator` or some joint aggregator that merges data with another source, depending on the use-case.

## Green Impact

Neutral
