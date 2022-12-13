# Data Source Matrices

The Carbon Aware SDK includes access to various data sources of carbon aware data, including WattTime, ElectricityMaps, and a custom JSON source. These matrices are an attempt to track what features of the Carbon Aware SDK are enabled for which data sources.

## Contents

- [Type of Data Sources and Configuration](#type-of-data-sources-and-configuration)
- [Data Source Routes Available](#data-source-routes-available)
- [Location Source Availability](#location-source-availability)

## Type of Data Sources and Configuration

In the CarbonAware SDK configuration, you can set what data source to use as the `EmissionsDataSource` and the `ForecastDataSource`. There are also certain configuration fields that must be set in order to access the raw data.
| Type      | WattTime  | ElectricityMaps | JSON |
|--------------------------|-----------|-----------------|------|
| Is Emissions DataSource   | Yes      | Yes             | Yes     |
| Is Forecast DataSource   | Yes  | Yes            | No     |
| Makes HTTP(s) call   | Yes  | Yes            | No    |
| Can Use Custom Data   | No  | No            | Yes    |
| Needs Authentication Config   | Yes - username/password | Yes - token/header            | No    |
| BaseUrl Override Config   | Only for testing  | Switch between trial + full version, testing            | No   |
| Supports Trial + Full Account   | Yes  | Yes (*different URL and token header required)            | N/A    |

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

## Location Source Availability

*I wasn't sure exactly what to fill here but wanted something along the lines of:

- WattTime is better to capture data for XX locations
- Electricity Maps is better to capture data for YY locations
- Electricity Maps can take a zone name or lat/long
- WattTime takes a region name (azure region?) or lat long
