# Selecting a Data Source

The Carbon Aware SDK includes access to various data sources of carbon aware
data, including WattTime, ElectricityMaps, and a custom JSON source. These
matrices are an attempt to track what features of the Carbon Aware SDK are
enabled for which data sources.

## Contents

- [Type of Data Sources and Configuration](#type-of-data-sources-and-configuration)
- [Data Source Methods Available](#data-source-methods-available)
- [Location Coverage](#location-coverage)

## Type of Data Sources and Configuration

In the CarbonAware SDK configuration, you can set what data source to use as the
`EmissionsDataSource` and the `ForecastDataSource`. There are also certain
configuration fields that must be set in order to access the raw data. | Type |
WattTime | ElectricityMaps | JSON |
|---|---|---|---| | Is Emissions
DataSource | &#9989; | &#9989; | &#9989; | | Is Forecast DataSource | &#9989; |
&#9989; | &#10060; | | Makes HTTP(s) call | &#9989; | &#9989; | &#10060; | | Can
Use Custom Data | &#10060; | &#10060; | &#9989; | | Supports Trial + Full
Account | &#9989; | &#9989;
(\*[different config required](./configuration.md#electricitymaps-configuration))
| N/A |

## Data Source Methods Available

Not all data sources support all the routes provided in the interfaces
(`IEmissionsDataSource`/`IForecastDataSource`).

| Methods | WattTime | ElectricityMaps | JSON | CLI Usage | Web Api Usage | SDK Usage |
| --- | :---: | :---: | :---: | :---: | :---: | :---: |
| GetCarbonIntensityAsync | &#9989; | &#9989; | &#9989; | `emissions` | `emissions/bylocation` or `emissions/bylocations` or `emissions/bylocations/best` or `emissions/average`&#8209;`carbon`&#8209;`intensity` or `emissions/average`&#8209;`carbon`&#8209;`intensity/batch` | `GetEmissionsDataAsync(...)` or `GetBestEmissionsDataAsync(...)` or `GetAverageCarbonIntensityDataAsync(...)` |
| GetCurrentForecastAsync | &#9989; | &#9989; | &#10060; | `emissions`&#8209;`forecasts` | `forecasts/current` | `GetCurrentForecastAsync(...)` |
| GetForecastByDateAsync | &#9989; | &#10060; | &#10060; | `emissions`&#8209;`forecasts`&#32;&#8209;&#8209;`requested`&#8209;`at` | `forecasts/batch` with `requestedAt` field | `GetForecastByDateAsync(...)` |

## Location Coverage

Different data sources provide both different features (as outlined above) but
also coverage of different geographic areas. It's important to note that each
data source may have different region names, which are handled through the
location config.

- For `WattTime`, see their
 [interactive coverage map](https://www.watttime.org/explorer) to find the
 relevant zone.
- For `ElectricityMaps`, see their
 [live map app](https://app.electricitymaps.com/map?utm_source=electricitymaps.com&utm_medium=website&utm_campaign=banner)
 to find the relevant zone and see current data coming in.
