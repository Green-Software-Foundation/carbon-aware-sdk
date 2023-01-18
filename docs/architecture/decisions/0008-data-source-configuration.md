# 8. Data Source Configuration

## Status

Accepted

## Date

2022-11-1

## Context

The current CarbonAware configuration is not intuitive for a user because it hides the relationships between entities, and so requires deep reading of the documentation to properly configure. This challenge is amplified for use-cases where different interfaces can be configured with different data sources. EG: JSON data source for emissions, but WattTime data source for forecast data.

## Decision

A top-level 'DataSources' section will specify all of the data source configuration needs for the consumer.

Within that, specific data source configurations will be defined in the 'Configurations' section. Each item containing the parameters required for configuring the data source in its entirety EG: client config, file paths, additional parameters, etc.

Each data source interface can then be configured by referencing these 'Configurations' by their key.

The resulting 'DataSources' config schema being:

```json
{
  "DataSources": {
    <DataSourceInterface Key1>: <'Configurations' Key String>,
    <DataSourceInterface Key2>: <'Configurations' Key String>,
    //...
    "Configurations": {
      <ConfigKey1>: {
        "Type": <string enum of data source implementation>,
        <Additional config key for this type>: <value>, 
        <Additional config key for this type>: <value>,
        //... 
      },
      <ConfigKey2>: {
        //... 
      },
      //...
    }
  }
}
```

## Consequences

The configuration scheme is flexible and extensible to support any new interfaces and data sources.
It reflects the relationship between data sources and all of their relevant configuration values.

### Implementation

Here is an example of the proposed configuration schema change with multiple potential data source interfaces:

```json
{
 "DataSources": {
    "CarbonIntensityDataSource": "WattTime",
    "EnergyDataSource": "ElectricityMaps",
    "EmbodiedCarbonDataSource": "ElectricityMaps",
    "Configurations": {
      "WattTime": {
        "Type": "CarbonAware.DataSources.WattTime",
        "ClientConfiguration": {
          "Username": "username",
          "Password": "password",
          "BaseURL": "https://api2.watttime.org/v2/"
        }
      },
      "ElectricityMaps": {
        "Type": "CarbonAware.DataSources.ElectricityMaps",
        "ClientConfiguration": {
          "API_Key": "abcd",
          "BaseURL": "https://api.electricitymap.org/v3/"
        },
        "disableEstimations": "true",
        "emissionsFactorType": "lifecycle"
      }
    }
  }
```

During initialization of the interface, the config will read to get the data source associated with it. For example, when a `CarbonIntensityDataSource` is initialized, it will get the corresponding value from the config, which is 'WattTime' in the above example. It then looks up the value of the 'WattTime' key in the 'Configurations' section. The object retrieved from the config will be then used to load and configure the `WattTimeDataSource`.

This "by-reference" configuration enables operators to use the same configuration for multiple data source interfaces without requiring duplication, as shown in the above example with the hypothetical `EnergyDataSource` and `EmbodiedCarbonDataSource` both using the same `ElectricityMaps` data source.

## Green Impact

Neutral
