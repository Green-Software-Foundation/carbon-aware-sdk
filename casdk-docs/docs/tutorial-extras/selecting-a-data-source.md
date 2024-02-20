---
sidebar_position: 1
---

# Selecting a Data Source

The Carbon Aware SDK includes access to various data sources of carbon aware
data, including WattTime, ElectricityMaps, ElectricityMapsFree, and a custom
JSON source. These matrices are an attempt to track what features of the Carbon
Aware SDK are enabled for which data sources.

## Contents

- [Type of Data Sources and Configuration](#type-of-data-sources-and-configuration)
- [Data Source Methods Available](#data-source-methods-available)
- [Location Coverage](#location-coverage)
- [Restriction: ElectricityMaps Free Trial User](#restrictions-electricitymaps-free-trial-user)

## Type of Data Sources and Configuration

In the CarbonAware SDK configuration, you can set what data source to use as the
`EmissionsDataSource` and the `ForecastDataSource`. There are also certain
configuration fields that must be set in order to access the raw data.

| Type                          | WattTime  | ElectricityMaps                                                                    | ElectricityMapsFree | JSON     |
|-------------------------------|-----------|------------------------------------------------------------------------------------|---------------------|----------|
| Is Emissions DataSource       | &#9989;   | &#9989;                                                                            | &#9989;             | &#9989;  |
| Is Forecast DataSource        | &#9989;   | &#9989;                                                                            | &#10060;            | &#10060; |
| Makes HTTP(s) call            | &#9989;   | &#9989;                                                                            | &#9989;             | &#10060; |
| Can Use Custom Data           | &#10060;  | &#10060;                                                                           | &#10060;            | &#9989;  |
| Supports Trial + Full Account | &#9989;   | &#9989; (\*[see restriction below](#restrictions-electricitymaps-free-trial-user)) | N/A                 | N/A      |

## Data Source Methods Available

Not all data sources support all the routes provided in the interfaces
(`IEmissionsDataSource`/`IForecastDataSource`).

| Methods                 | WattTime | ElectricityMaps | ElectricityMapsFree |   JSON   | CLI Usage                                                              | Web Api Usage                                                                                                                                                                                           | SDK Usage                                                                                                     |
| ----------------------- | :------: | :-------------: | :-----------------: | :------: | :--------------------------------------------------------------------: | :-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :-----------------------------------------------------------------------------------------------------------: |
| GetCarbonIntensityAsync | &#9989;  |     &#9989;     |        &#9989;      | &#9989;  | `emissions`                                                            | `emissions/bylocation` or `emissions/bylocations` or `emissions/bylocations/best` or `emissions/average`&#8209;`carbon`&#8209;`intensity` or `emissions/average`&#8209;`carbon`&#8209;`intensity/batch` | `GetEmissionsDataAsync(...)` or `GetBestEmissionsDataAsync(...)` or `GetAverageCarbonIntensityDataAsync(...)` |
| GetCurrentForecastAsync | &#9989;  |     &#9989;     |        &#10060;     | &#10060; | `emissions`&#8209;`forecasts`                                          | `forecasts/current`                                                                                                                                                                                     | `GetCurrentForecastAsync(...)`                                                                                |
| GetForecastByDateAsync  | &#9989;  |     &#10060;    |        &#10060;     | &#10060; | `emissions`&#8209;`forecasts`&#32;&#8209;&#8209;`requested`&#8209;`at` | `forecasts/batch` with `requestedAt` field                                                                                                                                                              | `GetForecastByDateAsync(...)`                                                                                 |

## Location Coverage

Different data sources provide both different features (as outlined above) but
also coverage of different geographic areas. It's important to note that each
data source may have different region names, which are handled through the
location config.

- For `WattTime`, see their
  [interactive coverage map](https://www.watttime.org/explorer) to find the
  relevant zone.
- For `ElectricityMaps`, see their
  [live map app](https://app.electricitymaps.com/map)
  to find the relevant zone and see current data coming in.
- For `ElectricityMapsFree`, see the Electricity Maps
  [zone list](https://api.electricitymap.org/v3/zones) to find the relevant
  zones.

## Restrictions: free trial of ElectricityMaps

ElectricityMaps allows new users to create a free trial for 1 month access to
the API. Free trial users have restricted access to the API and a slightly
different configuration for the SDK (see
[configuration.md](../tutorial-extras/configuration.md#electricitymaps-configuration). You can
request a free trial on the
[ElectricityMaps API Portal](https://api-portal.electricitymaps.com/).

### Restricted Zone Access

Free trial users only have access ~100 zones in the ElectricityMaps API.
ElectricityMaps maintains a
[frequently updated list](https://docs.google.com/document/d/e/2PACX-1vTdYp8E5E3fNogL54ICf_UxfA_rZ_RPO4WKWI4ZANPSX25jCbvHtAxc-VrJt9HymeRHFcSGWXjhVHS0/pub)
of available free trial zones that include the key, name, and country of each
zone. If you need access to other zones not included on the list, you will need
a full access product key.

### Restricted Endpoint Access

Free trial users only have access to seven endpoints in the ElectricityMaps API.
Of those seven, only two are currently supported as part of Carbon Aware SDK:

1. `GET /carbon-intensity/forecast`
2. `GET /carbon-intensity/history`

> Note: The Carbon Aware SDK is not restricting implementations to only support
> free trial users of ElectricityMaps. There may be implementations in the
> future that use endpoints that a free trial user may not be able to access and
> therefore cannot use that functionality of the SDK.

### Restricted Call Access

Free trial users are capped at 1,000 calls for the month of the free trial. Any
calls beyond the 1,000th call will be rejected.
