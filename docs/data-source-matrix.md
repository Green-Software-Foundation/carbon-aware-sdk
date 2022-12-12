# Data Source Matrix
The Carbon Aware SDK includes access to various data sources of carbon aware data, including WattTime, ElectricityMaps, and a custom JSON source. This matrix is an attempt to track what features of the Carbon Aware SDK are enabled for which data sources.

## Type of DataSource
In the CarbonAware SDK configuration, you can set what data source to use as the `EmissionsDataSource` and the `ForecastDataSource`.
| Type      | WattTime  | ElectricityMaps | JSON |
|--------------|:-----------:|:-----------------:|:------:|
| Emissions DataSource | Yes      | Yes             | Yes     ||
| Forecast DataSource | Yes  | Yes            | No     |

## Data Source Routes Available
Not all data sources support all the routes provided in the interfaces (`IEmissionsDataSource`/`IForecastDataSource`). The list below maps the interface route to the relevant consumer call, while the table lists only the interface route.

- GetCarbonIntensityAsync
    - CLI: `emissions`
    - API: `emissions/bylocation` / `emissions/bylocations` / `emissions/bylocations/best` / `emissions/average-carbon-intensity` / `average-carbon-intensity/batch`
    - Library: `GetEmissionsDataAsync(...)` / `GetBestEmissionsDataAsync(...)` / `GetAverageCarbonIntensityDataAsync(...)`
- GetCurrentForecastAsync
    - CLI: `emissions-forecasts`
    - API: `forecasts/current`
    - Library: `GetCurrentForecastAsync(...)`
- GetForecastByDateAsync
    - CLI: `emissions-forecasts --requested-at`
    - API: `forecasts/batch` with `requestedAt` field
    - Library: `GetForecastByDateAsync(...)`

| Route      | WattTime  | ElectricityMaps | JSON |
|--------------|:-----------:|:-----------------:|:------:|
| GetCarbonIntensityAsync | Yes  | Yes            | Yes    |
| GetCurrentForecastAsync | Yes  | Yes            | No    |
| GetForecastByDateAsync | Yes  | No            | No     |

## Data Source Configuration
| Configuration Type      | WattTime  | ElectricityMaps | JSON |
|--------------|:-----------:|:-----------------:|:------:|
| Authentication Required | Yes - username/password | Yes - token/header            | N/A    |
| BaseUrl Override | Only for testing  | Switch between trial + full version, testing            | N/A    |
| Support trial + full account | Yes  | Yes* (*different URL and token header required)            | N/A    |


## Miscellaneous
| Note     | WattTime  | ElectricityMaps | JSON |
|--------------|:-----------:|:-----------------:|:------:|
| Makes HTTP(s) call | Yes  | Yes            | No    |
| Can use custom data | No  | No            | Yes    |