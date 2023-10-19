
# 0016. Throws exception when future datetime is passed to current/historical APIs

## Status

Proposed

## Context

Current/historical APIs (e.g. /bylocation WebAPI endpoint) shouldn't accept future datetime because they are not for forecast data. So it should return an error when future datetime is passed.

Currently (v1.1 at least) current/historical APIs return empty list even if future datetime is passed. So we cannot aware it is invalid arguments when we call these APIs with future datetime.

## Decision

In C# code, throw [ArgumentOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception) when future datetime is passed to current/historical APIs.

In WebAPI, returns HTTP 400 (Bad request) to client when catches `ArgumentOutOfRangeException`.

### Affected WebAPI endpoints

* /bylocations/best
* /bylocations
* /bylocation
* /average-carbon-intensity

### Affected C# methods

* Implementations of `IEmissionsDataSource.GetCarbonIntensityAsync()`

## Green Impact  

Neutral
