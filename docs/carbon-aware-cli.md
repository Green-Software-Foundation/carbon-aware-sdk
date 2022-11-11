# Carbon Aware CLI

The CLI is best for use with systems you can not change the code in but can invoke command line.  For example - build pipelines.

The CLI exposes the primary `getEmissionsByLocationsAndTime` SDK methods via command line and outputs the results as json to stdout.  

> You can use the CLI via a docker image.

- [Carbon Aware CLI Reference](#carbon-aware-cli-reference)
  - [Build and Install](#build-and-install)
  - [Using the CLI](#using-the-cli)
    - [emissions](#emissions)
      - [Description](#description)
      - [Usage](#usage)
      - [Options](#options)
      - [Examples](#examples)
        - [Single Location Emissions](#single-location-emissions)
        - [Multiple Location Emissions](#multiple-location-emissions)
        - [Emissions with Start and End Times](#emissions-with-start-and-end-times)

## Build and Install

Build the CLI using the `dotnet publish` command:

```bash
dotnet publish ./src/CarbonAware.CLI/src/CarbonAware.CLI.csproj -c Release -o <your desired installation path>
```

> By default this will build for your host operating system.  To build for a platform other than your host platform you can specify the target runtime like this, using any valid [Runtime ID](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog#using-rids) (EG `win-x64`, `linux-x64`, `osx-x64`):
>
> ```bash
> dotnet publish .\src\CarbonAware.CLI\src\CarbonAware.CLI.csproj -c Release -r <RuntimeID> --self-contained -o <your desired installation path>
> ```

## Using the CLI

To use the CLI for the first time, navigate to your installation directory and run the binary with the `-h` flag to see the help menu.

On Windows: `.\caw.exe -h`
On MacOS/Linux: `.\caw -h`

### emissions

#### Description

Retrieve emissions data from specified locations and time periods.

#### Usage

`caw emissions [options]`

#### Options

```text
  -l, --location <location> (REQUIRED)  A named location
  -s, --start-time <startTime>          Start time of emissions data
  -e, --end-time <endTime>              End time of emissions data
  -b, --best                            Filter results down to the best (typically lowest) data point.
  -a, --average                         Outputs the weighted average of all data points within the start and end time boundaries.
  -?, -h, --help                        Show help and usage information
```

#### Examples

##### Single Location Emissions

command: `.\caw.exe emissions -l eastus`

output:

```json
[{"Location":"eastus","Time":"2022-08-30T12:45:11+00:00","Rating":65,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-08-30T20:45:11+00:00","Rating":65,"Duration":"08:00:00"},
// ...
{"Location":"eastus","Time":"2022-09-06T04:45:11+00:00","Rating":73,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-09-06T12:45:11+00:00","Rating":84,"Duration":"08:00:00"}]
```

##### Multiple Location Emissions

command: `.\caw emissions -l eastus -l westus`

output:

```json
[{"Location":"eastus","Time":"2022-08-30T12:45:11+00:00","Rating":65,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-08-30T20:45:11+00:00","Rating":65,"Duration":"08:00:00"},
// ...
{"Location":"westus","Time":"2022-09-06T04:45:11+00:00","Rating":73,"Duration":"08:00:00"},
{"Location":"westus","Time":"2022-09-06T12:45:11+00:00","Rating":84,"Duration":"08:00:00"}]
```

##### Emissions with Start and End Times

command: `.\caw emissions -l eastus --start-time 2022-07-01T00:00:00Z --end-time 2022-07-31T23:59:59Z --best`

output:

```json
[{"Location":"eastus","Time":"2022-07-01T04:45:11+00:00","Rating":65,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-07-01T12:45:11+00:00","Rating":65,"Duration":"08:00:00"},
// ...
{"Location":"eastus","Time":"2022-07-31T12:45:11+00:00","Rating":73,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-07-31T20:45:11+00:00","Rating":84,"Duration":"08:00:00"}]
```


##### Best Emissions

command: `.\caw emissions -l eastus -l westus --start-time 2022-07-01T00:00:00Z --end-time 2022-07-31T23:59:59Z --best`

output:

```json
[{"Location":"eastus","Time":"2022-07-08T04:45:11+00:00","Rating":48,"Duration":"08:00:00"}]
```

##### Average Emissions

command: `.\caw emissions -l eastus -l westus --start-time 2022-07-09T00:00:00Z --end-time 2022-07-09T12:00:00Z --average`

output:

```json
[{"Location":"eastus","Time":"2022-07-09T00:00:00+00:00","Rating":79.357,"Duration":"12:00:00"},
{"Location":"westus","Time":"2022-07-09T00:00:00+00:00","Rating":86.91243,"Duration":"12:00:00"}]
```

### Command `emissions-forecasts`

#### Description

Forecasted emissions

#### Usage

`caw emissions-forecasts [options]`

#### Options

```text
  -l, --location <location> (REQUIRED)  A list of locations
  --data-start-at <startTime>           Filter out forecasted data points before start at time.
  --data-end-at <endTime>               Filter out forecasted data points after end at time.
  -w, --window-size <INT>                   The estimated duration (in minutes) of the workload being forecasted. Defaults to the duration of a single forecast data point
  --requested-at                        Datetime of a previously generated forecast.  Returns the most current forecast if not provided.
  -?, -h, --help                        Show help and usage information
```

#### Examples

##### Single Location Current Forecast

command: `.\caw emissions-forecasts -l northeurope`

output:

```json
[{
  "requestedAt": "2022-07-19T13:37:49+00:00",
  "generatedAt": "2022-07-19T13:35:00+00:00",
  "location": "northeurope",
  "dataStartAt": "2022-07-19T14:00:00Z",
  "dataEndAt": "2022-07-20T04:38:00Z",
  "windowSize": 5,
  "optimalDataPoint": {
    "location": "IE",
    "timestamp": "2022-07-19T18:45:00+00:00",
    "duration": 5,
    "value": 448.4451043375
  },
  "forecastData": [
    {
      "location": "IE",
      "timestamp": "2022-07-19T14:00:00+00:00",
      "duration": 5,
      "value": 532.02293146
    },
    ...
    {
      "location": "IE",
      "timestamp": "2022-07-20T04:30:00+00:00",
      "duration": 5,
      "value": 535.7318741001667
    }
  ]
}]
```

##### Multiple Location Current Forecasts

command: `.\caw emissions-forecasts -l eastus -l westus`

output:

```json
[
  {
    "requestedAt": "2022-06-01T12:01:00+00:00"
    "generatedAt": "2022-06-01T12:00:00+00:00",
    "optimalDataPoint": {
      "location": "PJM_ROANOKE",
      "timestamp": "2022-06-01T16:45:00+00:00",
      "duration": 5,
      "value": 448.4451043375
    },
    "forecastData": [ ... ] // all relevant forecast data points
    "location": "eastus",
    "dataStartAt": "2022-06-01T14:05:00+00:00",
    "dataEndAt": "2022-06-02T14:00:00+00:00",
    "windowSize": 5,
  },
  {
    "requestedAt": "2022-06-01T12:01:00+00:00"
    "generatedAt": "2022-06-01T12:00:00+00:00",
    "optimalDataPoint": {
      "location": "CAISO_NORTH",
      "timestamp": "2022-06-13T09:25:00+00:00",
      "duration": 5,
      "value": 328.178478
    },
    "forecastData": [ ... ] // all relevant forecast data points
    "location": "westus",
    "dataStartAt": "2022-06-01T14:05:00+00:00",
    "dataEndAt": "2022-06-02T14:00:00+00:00",
    "windowSize": 5,
  }
]
```

##### Filtered Data and Window Size Forecast

> Note: For current forecasts, since the data filters must fall within the forecasted data points, it is advisable to create them dynamically.
> `TIME_TWO_HOURS_FROM_NOW=$(date --date='2 hours' --utc --iso-8601='seconds')`
> `TIME_NINETEEN_HOURS_FROM_NOW=$(date --date='19 hours' --utc --iso-8601='seconds')`
command: `.\caw emissions-forecasts -l northeurope -l westus --data-start-at TIME_TWO_HOURS_FROM_NOW --data-end-at TIME_NINETEEN_HOURS_FROM_NOW -w 10`

output:

```json
[{
  "requestedAt": "2022-07-19T13:37:49+00:00",
  "generatedAt": "2022-07-19T13:35:00+00:00",
  "location": "northeurope",
  "dataStartAt": "2022-07-19T15:37:49+00:00",
  "dataEndAt": "2022-07-20T08:37:49+00:00",
  "windowSize": 10,
  "optimalDataPoint": {
    "location": "IE",
    "timestamp": "2022-07-19T18:45:00+00:00",
    "duration": 10,
    "value": 448.4451043375
  },
  "forecastData": [
    {
      "location": "IE",
      "timestamp": "2022-07-19T15:40:00+00:00",
      "duration": 10,
      "value": 532.02293146
    },
    ...
    {
      "location": "IE",
      "timestamp": "2022-07-20T08:30:00+00:00",
      "duration": 10,
      "value": 535.7318741001667
    }
  ]
},
{
  "requestedAt": "2022-07-19T13:37:49+00:00",
  "generatedAt": "2022-07-19T13:35:00+00:00",
  "location": "westus",
  "dataStartAt": "2022-07-19T15:37:49+00:00",
  "dataEndAt": "2022-07-20T08:37:49+00:00",
  "windowSize": 10,
  "optimalDataPoint": {
    "location": "CAISO_NORTH",
    "timestamp": "2022-07-19T18:45:00+00:00",
    "duration": 10,
    "value": 502.02293146
  },
  "forecastData": [
    {
      "location": "CAISO_NORTH",
      "timestamp": "2022-07-19T15:40:00+00:00",
      "duration": 10,
      "value": 612.9132146
    },
    ...
    {
      "location": "CAISO_NORTH",
      "timestamp": "2022-07-20T08:30:00+00:00",
      "duration": 10,
      "value": 523.172030157
    }
  ]
}]
```

##### Historical Forecast

command: `.\caw emissions-forecasts -l northeurope -l westus --requested-at 2022-06-15T18:31:00Z`

output:

```json
[{
  "requestedAt": "2022-06-15T18:31:00+00:00",
  "generatedAt": "2022-06-15T18:30:00+00:00",
  "location": "northeurope",
  "dataStartAt": "2022-06-15T18:35:00+00:00",
  "dataEndAt": "2022-06-16T18:30:00+00:00",
  "windowSize": 5,
  "optimalDataPoint": {
    "location": "IE",
    "timestamp": "2022-06-15T23:40:00+00:00",
    "duration": 5,
    "value": 448.4451043375
  },
  "forecastData": [
    {
      "location": "IE",
      "timestamp": "2022-06-15T18:35:00+00:00",
      "duration": 5,
      "value": 532.02293146
    },
    ...
    {
      "location": "IE",
      "timestamp": "2022-06-16T18:25:00+00:00",
      "duration": 5,
      "value": 535.7318741001667
    }
  ]
},
{
  "requestedAt": "2022-06-15T18:31:00+00:00",
  "generatedAt": "2022-06-15T18:30:00+00:00",
  "location": "westus",
  "dataStartAt": "2022-06-15T18:35:00+00:00",
  "dataEndAt": "2022-06-16T18:30:00+00:00",
  "windowSize": 5,
  "optimalDataPoint": {
    "location": "CAISO_NORTH",
    "timestamp": "2022-06-15T23:40:00+00:00",
    "duration": 5,
    "value": 423.4451043375
  },
  "forecastData": [
    {
      "location": "CAISO_NORTH",
      "timestamp": "2022-06-15T18:35:00+00:00",
      "duration": 5,
      "value": 482.02293146
    },
    ...
    {
      "location": "CAISO_NORTH",
      "timestamp": "2022-06-16T18:25:00+00:00",
      "duration": 5,
      "value": 576.7318741008
    }
  ]
}]
```
