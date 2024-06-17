
# 0015. WattTime v3 Changes

## Status

Proposed

## Context
As part of the update to Watt Time v3 we are proposing the changes to the underlying API calls.  This needs to be tracked so we understand the impacts, and if multiple options are available, which option was selected and why.

This wll impact the `CarbonAware.DataSources.WattTime` project primarily.

## Decision

The proposal is for the outlined WattTime API mapping and changes.

## WattTime v2, v3 Mapping

The following document and guidelines was used to understand the impact to the Carbon Aware SDK for the WattTime v3 updates.  https://docs.watttime.org/#tag/Transitioning-from-APIv2-to-APIv3 

### Base URL
The base URL will need to change.
> TODO: Add where this is configured 

|Base URL (v2) | Based URL (v3) | 
|---|---|
| /v2 | /v3 |


### Paths
The paths will also need to change. 

The following is configured at  `CarbonAware.DataSources.WattTime/src/Constants/Paths.cs`

| API Endpoint | Description | Path (v2) | Path (v3) | Notes |
|--------------|-------------|-----------|-----------|---|
| Data         | Get data    | /data     | /historical          |  _Request_ <li> `starttime` is now `start` and mandatory <li> `endtime` is now `end` and mandatory <li> `ba` is now `region` <li> `signal_type` added <br> _Response_ <li> `signal_type` added
| Forecast     | Get forecast| /forecast | /forecast   | **TODO: CHECK IMPACT** <br> No longer be used for historical data <br> _Request_ <li> `ba` is now `region` <li> `extended_forecast` removed <li> `horizon_hours` added  <li> `signal_type` added <li> Historical forecasts are now at `/forecast/historical` <br> _Response_ <li> `signal_type` added 
| Historical   | Get historical forecast data | /historical (?) | /forecast/historical (?)           | **We need to validate why historical was being used for the API, and what historical used to be, and whether this should be the new /forecast/historical or not.**
| Balancing Authority From Location | Get balancing authority from location | /ba-from-loc | /region-from-loc          | Check if the CA SDK uses BA at all <br><br> _Request_ <li> `name` is now `region_full_name` <li> `abbrev` is now `region` <li> `signal_type` added <br> _Response_ <li> `id` removed <li> `signal_type` added  | 
| Login        | User login  | https://api2.watttime.org/v2/login    | https://api.watttime.org/login | Path has changed from being version specific to being no longer related to the API version.  <br><br> NOTE: Updated in wattTime client to now have 2 HTTP clients to decouple versions from the login. 

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

## Green Impact  

Neutral

