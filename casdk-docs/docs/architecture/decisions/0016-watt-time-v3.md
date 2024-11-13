
# 0015. WattTime v3 Changes

## Status

Proposed

## Context
As part of the update to Watt Time v3 we are proposing the changes to the underlying API calls.  This needs to be tracked so we understand the impacts, and if multiple options are available, which option was selected and why.

This wll impact the `CarbonAware.DataSources.WattTime` project primarily - however all test will need changing where there are downstream tests that are impacted, and for example, if any initialization needs reconfiguring it will impact dependency injection and likely `Program.cs` in the WattTime API projects.

## Decision

The proposal is for the outlined WattTime API mapping and changes.

## WattTime v2, v3 Mapping

The following document and guidelines was used to understand the impact to the Carbon Aware SDK for the WattTime v3 updates.  https://docs.watttime.org/#tag/Transitioning-from-APIv2-to-APIv3 

### Base URL
The base URL will need to change.  This is configured in the `appsettings.config` and can be set from environment variables.

|Base URL (v2) | Based URL (v3) | 
|---|---|
| /v2 | /v3 |


### Paths
The paths will also need to change. 

The following is configured at  `CarbonAware.DataSources.WattTime/src/Constants/Paths.cs`

All response types for emission data include a response/data object, and a `meta` object which contains information such as `region`.  As such historical data, forecast data, and historical forecast data objects will change significantly.  These will be moved across to their own `...Response` record objects to abstract any future changes in the response type.  Previously these objects had a lot of overlap so where the same class and this will cause significant rework of the code and tests, and breaking these out now will abstract them. 

The `Login` is now at a different base URL, and to avoid any future issues a different HTTP client will be used for authentication, and the existing HTTP client will be used for API interaction.  These will still sit in the `WattTimeClient` and as such no impacts to the dependent classes/logic.

| API Endpoint | Description | Path (v2) | Path (v3) | Notes |
|--------------|-------------|-----------|-----------|---|
| Data         | Get data    | /data     | /historical          |  _Request_ <li> `starttime` is now `start` and mandatory </li><li> `endtime` is now `end` and mandatory </li><li> `ba` is now `region` </li><li> `signal_type` added <br /> _Response_ </li><li> `signal_type` added </li>|
| Forecast     | Get forecast| /forecast | /forecast   | <br /> No longer be used for historical data <br /> _Request_ <li> `ba` is now `region` </li><li> `extended_forecast` removed </li><li> `horizon_hours` added  </li><li> `signal_type` added </li><li> Historical forecasts are now at `/forecast/historical` <br /> _Response_ </li><li> `signal_type` added </li>|
| Historical   | Get historical forecast data | /historical (?) | /forecast/historical (?)           | This changed signficantly.  <br /> _Request_ <li> `ba` is now `region` </li><li> `starttime` is now `start` and mandatory </li><li> `endtime` is now `end` and mandatory </li><li> `signal_type` added <br /> _Response_ </li><li> `signal_type` added </li>|
| Balancing Authority From Location | Get balancing authority from location | /ba-from-loc | /region-from-loc          | Check if the CA SDK uses BA at all <br /><br /> _Request_ <li> `name` is now `region_full_name` </li><li> `abbrev` is now `region` </li><li> `signal_type` added <br /> _Response_ </li><li> `id` removed </li><li> `signal_type` added  </li> | 
| Login        | User login  | https://api2.watttime.org/v2/login    | https://api.watttime.org/login | Path has changed from being version specific to being no longer related to the API version.  <br /><br /> Updated in `WattTimeClient` to now have 2 HTTP clients to decouple versions from the login. |

### Query Strings

#### Signal Type
Everything call takes an optional `signal_type` parameter that defaults to `co2_moer`.  

The following comes from `CarbonAware.DataSources.WattTime/src/Constants/QueryStrings.cs` and the changes are consistent with the discussion above.

| Query String (v2)                   | Query String if Changed (v3)                 | Description                  |
|------------------------------------|----------------------------------|------------------------------|
| `ba`                                 | `region`                                 | Balancing Authority / Region |
| `starttime`                          | `start`                                | Start Time                       |
| `endtime`                            | `end`                                 | End Time                         |
| `latitude`                           | -                                 | Latitude                         |
| `longitude`                          | -                                 | Longitude                        |
| `username`                           | -                                 | Username                         |

## Update Changes
With some of the changes to the code, some of the configuration will also needs to change.

| Config (v2)                   | Config (v3)                 | Description                  |
|------------------------------------|----------------------------------|------------------------------|
| `BalancingAuthorityCacheTTL`                                 | `RegionCacheTTL`                                 | This is the cache for regions data in seconds, and has a default value of 1 day. |
| n/a                        | `AuthenticationBaseUrl`                                | **NEW** This is the base URL for the WattTime Authentication API and defaults to `https://api.watttime.org/` if not set. |

Example below if set (note they do not have to be set)
```json
"wattTime_no-proxy": {
  "Type": "WattTime",
  "Username": "the_username",
  "Password": "super_secret_secret",
  "BaseURL": "https://api.watttime.org/v3/",
  "AutenticationBaseURL": "https://api.watttime.org", // This is new but not mandatory
  "RegionCacheTTL": 86400, // This is new but not mandatory
  "Proxy": {
    "UseProxy": false
  }
```
## Green Impact  

Neutral

