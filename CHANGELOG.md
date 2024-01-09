# Changelog

All notable changes to the Carbon Aware SDK will be documented in this file.

## [1.2.0] - 2024-01

### Added 

- [#381 Add Helm chart and workflow](https://github.com/Green-Software-Foundation/carbon-aware-sdk/pull/381)
- New package release for Helm charts available at https://github.com/Green-Software-Foundation/carbon-aware-sdk/pkgs/container/charts%2Fcarbon-aware-sdk

### Fixed

- [#232 Generating SDK client does not work on linux](https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues/232)
- [#393 Fix: verify-azure-function-with-packages](https://github.com/Green-Software-Foundation/carbon-aware-sdk/pull/393)
- [#391 fixing 3 broken links in overview.md](https://github.com/Green-Software-Foundation/carbon-aware-sdk/pull/391)
- [#389 EMFree data source should regard specified time range](https://github.com/Green-Software-Foundation/carbon-aware-sdk/pull/389)

### Changed

- [#425 Updating CONTRIBUTING.md](https://github.com/Green-Software-Foundation/carbon-aware-sdk/pull/425)

#### API

- 

#### API Deployment

- 

#### SDK 

- 


#### Other

- Improved process leveraging the project boards at https://github.com/orgs/Green-Software-Foundation/projects/15/views/2


For more details, checkout [https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues/232](https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues?q=label%3Av1.2+is%3Aclosed+)

## [1.1.0] - 2023-07-18

### Added 

- Added Electricity Maps (paid api) support for forecasting and historical data.  
- Added ElectricityMaps (free api) support for historical data.  Note that this API does not support forecast capabilities. 

### Fixed

- Fixed generated test data that had time bomb bug that was in test data, which caused integration tests to fail.  This is now automatically generated each time.
- Fixed some bugs that related to underlying data source errors surfacing as HTTP 500 errors from the API.  These should now be more consistent.
- Fixed an issue where UTF-8 passwords were encoded as ASCII for WattTime API, causing integration failure. 
- Fixes some bugs in unit tests with uncaught scenarios, or faulty tests.

### Changed

- No previous API's were changed.  
- Configuration has changed.  Refer to upgrading from 1.0.0 to 1.1.0 below.
- Time is now always in UTC.  Previously the API may have returned local time depending on underlying API.

#### API

- `/locations` - Show the list of configured named locations that can be used in the API.
- `/api/v1/swagger.yaml` - Provides OpenAPI document now at public endpoint when deployed.

#### API Deployment

- Configuration has changed.  Refer to upgrading from 1.0.0 to 1.1.0 below.

#### SDK 

- SDK was abstracted to provide a library for DLL import usage, which now allows users to use the SDK in their projects directly without the need to deploy an API.  This is useful in scenarios where the API can not be centralised.  Note - we still highly recommend centralising for management of the API and audit capabilities with observability.
- Functionality for forecast and historical data have been seperated into seperate interfaces.  This impacts configuration, see upgrading from 1.0.0 to 1.1.0 for more information.
- Additional tests across the SDK have been added.
- Aggregation tier in the SDK was removed, this should not impact users of the SDK, but may impact maintainers who were actively contributing.


#### Other

- All contributors need to signoff commits for contribution using `git commit -s`.
- Added PR release workflow improvements for the project management of the CA SDK project team.
- Updated the project to prune stale PR's and issues to help with the management of the CA SDK project.


### Upgrading from 1.0.0 to 1.1.0 

- Configuration changes are required due to historical and forecast configuration now being decoupled.  Refer to - [Configuration](docs/configuration.md) for a guide. The following is provided as an example of the new data source configuration format.
```json
{
  "DataSources": {
    "EmissionsDataSource": "Json",
    "ForecastDataSource": "WattTime",
    "Configurations": {
      "WattTime": {
        "Type": "WattTime",
        "Username": "username",
        "Password": "password",
        "BaseURL": "https://api2.watttime.org/v2/",
        "Proxy": {
          "useProxy": true,
          "url": "http://10.10.10.1",
          "username": "proxyUsername",
          "password": "proxyPassword"
        }
      },
      "ElectricityMaps": {
        "Type": "ElectricityMaps",
        "APITokenHeader": "auth-token",
        "APIToken": "myAwesomeToken",
        "BaseURL": "https://api.electricitymap.org/v3/"
      },
      "Json": {
        "Type": "Json",
        "DataFileLocation": "test-data-azure-emissions.json"
      }
    }
  }
}
```

## [1.0.0] - 2022-10-01

### Added

- Added CLI for carbon data 
- Added WebApi for carbon data 
- Added support for WattTime in forecasts and historical data sets
- Added following API's 
  - `/emissions/bylocation` - Calculate the best emission data by location for a specified time period 
  - `/emissions/bylocations` - Calculate the observed emission data by list of locations for a specified time period
  - `/emissions/bylocations/best` - Calculate the best emission data by list of locations for a specified time period
  - `/emissions/average-carbon-intensity` - Retrieves the measured carbon intensity data between the time boundaries and calculates the average carbon intensity during that period
  - `/emissions/forecasts/current` - Retrieves the most recent forecasted data and calculates the optimal marginal carbon intensity window
  - `/emissions/forecasts/batch` - Given an array of historical forecasts, retrieves the data that contains forecasts metadata, the optimal forecast and a range of forecasts filtered by the attributes [start...end] if provided
  - `/emissions/average-carbon-intensity/batch` - Given an array of request objects, each with their own location and time boundaries, calculate the average carbon intensity for that location and time period and return an array of carbon intensity objects.


### Changed

- First major release to main, no previous version

### Security

- No known security vulnerabilities or concerns 
