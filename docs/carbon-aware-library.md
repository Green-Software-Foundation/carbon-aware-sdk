# Carbon Aware Library

The Carbon Aware SDK provides a C\# Client Library to get the marginal carbon
intensity for a given location and time period. The values reported in the Green
Software Foundation's specification for marginal carbon intensity (Grams per
Kilowatt Hour).

**_Recommended_** - This user interface is best for when you need a consumable
version of the SDK as this library can be packaged into a nuget and consumed
locally.

The library replicates the Web Api, CLI and SDK functionality, leveraging the
same configuration

## Table of Contents

- [Carbon Aware Library](#carbon-aware-library)
  - [Table of Contents](#table-of-contents)
  - [EmissionsHandler Functions](#emissionshandler-functions)
    - [GetEmissionsDataAsync](#getemissionsdataasync)
    - [GetBestEmissionsDataAsync](#getbestemissionsdataasync)
    - [GetAverageCarbonIntensityAsync](#getaveragecarbonintensityasync)
  - [ForecastHandler Functions](#forecasthandler-functions)
    - [GetCurrentForecastAsync](#getcurrentforecastasync)
    - [GetForecastByDateAsync](#getforecastbydateasync)
  - [Data Sources](#data-sources)
    - [WattTime](#watttime)
      - [Locations](#locations)
      - [Exception Handling](#exception-handling)
    - [ElectricityMaps](#electricitymaps)
      - [Locations](#locations)
      - [Exception Handling](#exception-handling)

## EmissionsHandler Functions

The `EmissionsHandler` is responsible for all the functions that query the SDK
for `EmissionsData`. This includes both getting the data directly, or getting
the average carbon intensity from the data. There are currently 4 functions
managed by this handler:

1. [GetEmissionsDataAsync](#getemissionsdataasync)
2. [GetBestEmissionsDataAsync](#getbestemissionsdataasync)
3. [GetAverageCarbonIntensityAsync](#getaveragecarbonintensityasync)

### GetEmissionsDataAsync

This function calculates the observed emission data by location for a specified
time period. The location is a required parameter and is the name of the data
region for the configured Cloud provider. If time period is not provided, it
retrieves all the data until the current time.

`GetEmissionsDataAsync` has two signatures: one which takes a single location
and one which takes an array of locations. Both signatures return the same
response: an array of `EmissionsData` objects that contains the location, time
and the rating in g/kWh.

#### _Signature 1: Single Location + Start + End_

Parameters:

1. `location`: The string name of the data region for the configured Cloud
   provider.
2. `startTime`: [Optional] The time at which the workload and corresponding
   carbon usage begins.
3. `endTime`: [Optional] The time at which the workload and corresponding carbon
   usage ends.

Request:

```csharp
var data =  await this._emissionsHandler.GetEmissionsDataAsync(
  "eastus",
  DateTimeOffset(2022,1,2),
  DateTimeOffset(2022,5,17)
);
```

Response:

```csharp
[
  EmissionsData()
    {
      Location:"eastus"
      Time: DateTimeOffset("2022-05-17T20:45:11.5092741+00:00"),
      Rating: 70,
      Duration: 60,
    },
  ...
]
```

#### _Signature 2: Array of Locations + Start + End_

Parameters:

1. `locations`: The string array of names of the data regions for the configured
   Cloud provider.
2. `startTime`: [Optional] The time at which the workload and corresponding
   carbon usage begins.
3. `endTime`: [Optional] The time at which the workload and corresponding carbon
   usage ends.

Request:

```csharp
var data =  await this._emissionsHandler.GetEmissionsDataAsync(
  string[]{"eastus", "westus"},
  DateTimeOffset(2022,1,2),
  DateTimeOffset(2022,5,17)
);
```

Response:

```csharp
[
  EmissionsData()
    {
      Location:"eastus"
      Time: DateTimeOffset("2022-05-17T20:45:11.5092741+00:00"),
      Rating: 70,
      Duration: 60,
    },
  EmissionsData()
    {
      Location:"west"
      Time: DateTimeOffset("2022-05-17T20:45:11.5092741+00:00"),
      Rating: 52,
      Duration: 60,
    },
  ...
]
```

### GetBestEmissionsDataAsync

This function calculates the best observed emission data by an array of
locations for a specified time period.

Location is a required parameter and is an array of the names of the data region
for the configured Cloud provider. If time period is not provided, it retrieves
all the data until the current time.

`GetBestEmissionsDataAsync` has two signatures: one which takes a single
location and one which takes an array of locations. Both signatures return the
same response: an array of `EmissionsData` objects that contains the location,
time and the rating in g/kWh.

#### _Signature 1: Location + Start + End_

Parameters:

1. `location`: The string name of the data regions for the configured Cloud
   provider.
2. `startTime`: [Optional] The time at which the workload and corresponding
   carbon usage begins.
3. `endTime`: [Optional] The time at which the workload and corresponding carbon
   usage ends.

```csharp
var data =  await this._emissionsHandler.GetBestEmissionsDataAsync(
  "eastus",
  DateTimeOffset(2022,1,2),
  DateTimeOffset(2022,5,17)
);
```

The response is an array of `EmissionsData` objects that contains the location,
time and the rating in g/kWh.

```csharp
[
  EmissionsData()
    {
      Location:"eastus"
      Time: DateTimeOffset("2022-05-17T20:45:11.5092741+00:00"),
      Rating: 70,
      Duration: 60,
    },
  ...
]
```

#### _Signature 2: Multiple Locations + Start + End_

Parameters:

1. `locations`: The string array of names of the data regions for the configured
   Cloud provider.
2. `startTime`: [Optional] The time at which the workload and corresponding
   carbon usage begins.
3. `endTime`: [Optional] The time at which the workload and corresponding carbon
   usage ends.

```csharp
var data =  await this._emissionsHandler.GetBestEmissionsDataAsync(
  string[]{"eastus", "westus"},
  DateTimeOffset(2022,1,2),
  DateTimeOffset(2022,5,17)
);
```

The response is an array of `EmissionsData` objects that contains the location,
time and the rating in g/kWh.

```csharp
[
  EmissionsData()
    {
      Location:"westus"
      Time: DateTimeOffset("2022-05-17T20:45:11.5092741+00:00"),
      Rating: 70,
      Duration: 60,
    },
  ...
]
```

### GetAverageCarbonIntensityAsync

This function retrieves the measured carbon intensity data for a given location
between the time boundaries and calculates the average carbon intensity during
that period. Location is a required parameter and is the name of the data region
for the configured Cloud provider. This function is useful for reporting the
measured carbon intensity for a specific time period in a specific location.

Parameters:

1. `location`: The string name of the data region for the configured Cloud
   provider.
2. `start`: The time at which the workflow we are measuring carbon intensity for
   started
3. `end`: The time at which the workflow we are measuring carbon intensity for
   ended

Request:

```csharp
var data =  await this._emissionsHandler.GetAverageCarbonIntensityAsync(
  "eastus",
  DateTimeOffset(2022,7,19,14,0,0,Timespan.Zero),
  DateTimeOffset(2022,7,19,18,0,0,Timespan.Zero)
);
```

The response is a single double value representing the calculated average
marginal carbon intensity g/kWh.

```csharp
345.434
```

## ForecastHandler Functions

The `ForecastHandler` is responsible for all the functions that query the SDK
for `EmissionsForecast`. There are currently 2 functions managed by this
handler:

1. [GetCurrentForecastAsync](#getcurrentforecastasync)
2. [GetForecastByDateAsync](#getforecastbydateasync)

### GetCurrentForecastAsync

This function fetches only the most recently generated forecast for all provided
locations. It uses the "dataStartAt" and "dataEndAt" parameters to scope the
forecasted data points (if available for those times). If no start or end time
boundaries are provided, the entire forecast dataset is used. The scoped data
points are used to calculate average marginal carbon intensities of the
specified "windowSize" and the optimal marginal carbon intensity window is
identified.

The forecast data represents what the data source predicts future marginal
carbon intensity values to be, not actual measured emissions data (as future
values cannot be known).

This endpoint is useful for determining if there is a more carbon-optimal time
to use electricity predicted in the future.

Parameters:

1. `location`: This is a required parameter and is an array of the names of the
   data region for the configured Cloud provider.
2. `dataStartAt`: Start time boundary of the current forecast data points.
   Ignores current forecast data points before this time. Must be within the
   forecast data point timestamps. Defaults to the earliest time in the forecast
   data.
3. `dataEndAt`: End time boundary of the current forecast data points. Ignores
   current forecast data points after this time. Must be within the forecast
   data point timestamps. Defaults to the latest time in the forecast data. If
   neither `dataStartAt` nor `dataEndAt` are provided, all forecasted data
   points are used in calculating the optimal marginal carbon intensity window.
4. `windowSize`: The estimated duration (in minutes) of the workload. Defaults
   to the duration of a single forecast data point.

```csharp
var data = await this._forecastHandler.GetCurrentForecastAsync(
  "northeurope",
  DateTimeOffset(2022,7,19,14,0,0,TimeSpan.Zero),
  DateTimeOffset(2022,7,20,4,38,0,TimeSpan.Zero),
  10
);
```

The response is an array of `EmissionsForecast` objects (one per requested
location) with their optimal marginal carbon intensity windows.

```csharp
[
  EmissionsForecast()
  {
    RequestedAt: DateTimeOffset("2022-07-19T13:37:49+00:00"),
    GeneratedAt: DateTimeOffset("2022-07-19T13:35:00+00:00"),
    OptimalDataPoints: [
      EmissionsData()
      {
        Location: "IE",
        Time: DateTimeOffset("2022-07-19T18:45:00+00:00"),
        Duration: 10,
        Rating: 448.4451043375
      }
    ],
    EmissionsDataPoints: [
      EmissionsData()
      {
        Location: "IE",
        Time: DateTimeOffset("2022-07-19T14:00:00+00:00"),
        Duration: 10,
        Rating: 532.02293146
      },
      ...
      EmissionsData()
      {
        Location: "IE",
        Time: DateTimeOffset("2022-07-20T04:25:00+00:00"),
        Duration: 10,
        Rating: 535.7318741001667
      }
    ]
  }
]
```

### GetForecastByDateAsync

This function takes a requests for historical forecast data, fetches it, and
calculates the optimal marginal carbon intensity window. This endpoint is useful
for back-testing what one might have done in the past, if they had access to the
current forecast at the time.

Parameters:

1. `location`: This is a required parameter and is the name of the data region
   for the configured Cloud provider.
2. `dataStartAt`: Start time boundary of the forecast data points. Ignores
   forecast data points before this time. Must be within the forecast data point
   timestamps. Defaults to the earliest time in the forecast data.
3. `dataEndAt`: End time boundary of the forecast data points. Ignores forecast
   data points after this time. Must be within the forecast data point
   timestamps. Defaults to the latest time in the forecast data.
4. `requestedAt`: This is a required parameter and is the historical time used
   to fetch the most recent forecast as of that time.
5. `windowSize`: The estimated duration (in minutes) of the workload. Defaults
   to the duration of a single forecast data point

If neither `dataStartAt` nor `dataEndAt` are provided, all forecasted data
points are used in calculating the optimal marginal carbon intensity window.

```csharp
var data = await this._forecastHandler.GetForecastByDateAsync(
  "northeurope",
  DateTimeOffset(2022,7,19,14,0,0,TimeSpan.Zero),
  DateTimeOffset(2022,7,20,4,38,0,TimeSpan.Zero),
  DateTimeOffset(2022,7,19,13,30,0,TimeSpan.Zero),
  10
);
```

The response is an `EmissionsForecast` object with the optimal marginal carbon
intensity window.

```csharp
EmissionsForecast()
{
  RequestedAt: DateTimeOffset("2022-07-19T13:30:00+00:00"),
  GeneratedAt: DateTimeOffset("2022-07-19T13:35:00+00:00"),
  OptimalDataPoints: [
    EmissionsData()
    {
      Location: "IE",
      Time: DateTimeOffset("2022-07-19T18:45:00+00:00"),
      Duration: 10,
      Rating: 448.4451043375
    }
  ],
  EmissionsDataPoints: [
    EmissionsData()
    {
      Location: "IE",
      Time: DateTimeOffset("2022-07-19T14:00:00+00:00"),
      Duration: 10,
      Rating: 532.02293146
    },
    ...
    EmissionsData()
    {
      Location: "IE",
      Time: DateTimeOffset("2022-07-20T04:25:00+00:00"),
      Duration: 10,
      Rating: 535.7318741001667
    }
  ]
}
```

## Data Sources

### WattTime

#### Locations

Each WattTime emissions data point is associated with a particular named
balancing authority. For transparency, this value is also used in
`EmissionsData` response objects. It is not overwritten to match the named
datacenter provided by any request.

> "A balancing authority ensures, in real time, that power system demand and
> supply are finely balanced. This balance is needed to maintain the safe and
> reliable operation of the power system. If demand and supply fall out of
> balance, local or even wide-area blackouts can result."
>
> See [this post](https://www.eia.gov/todayinenergy/detail.php?id=27152) on
> balancing authories from the EIA to learn more.

#### Exception Handling

If WattTime responds with a 4XX or 5XX status code the WattTime Data Source will
forward the response code and message back to the caller. Refer to the
[current WattTime documentation](https://www.watttime.org/api-documentation/)
for the most up-to-date information about possible error codes.

#### Example Emissions Call Using WattTime

The swimlanes diagram below follows an example request for emissions data using
WattTime as the data source provider. In this diagram, the _Client_ is a user of
the SDK and the _WattTimeService_ is the [WattTime](https://www.wattime.org)
API.

![webapi to watttime flow diagram](./images/webapi-swimlanes.png)

### ElectricityMaps

#### Locations

Each ElectricityMaps emissions data point is associated with a particular named
zone name. While the ElectricityMaps endpoint supports calling with lat/long
geoposition as well, the result will always be a corresponding zone name. They
provide a
[route on their API](https://static.electricitymaps.com/api/docs/index.html#zones)
which can be queried to list all the zone names you have access to given your
token

#### Exception Handling

If ElectricityMaps responds with a 4XX or 5XX status code the ElectricityMaps
Data Source will forward the response code and message back to the caller. Refer
to the
[ElectricityMapsHttpClientException](../src/CarbonAware.DataSources/CarbonAware.DataSources.ElectricityMaps/src/Client/ElectricityMapsClientHttpException.cs)
class for documentation on expected error codes.
