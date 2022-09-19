# Carbon Aware CLI Reference

The following is the documentation for the Carbon Aware CLI

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

```text
[{"Location":"eastus","Time":"2022-08-30T12:45:11+00:00","Rating":65,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-08-30T20:45:11+00:00","Rating":65,"Duration":"08:00:00"},
// ...
{"Location":"eastus","Time":"2022-09-06T04:45:11+00:00","Rating":73,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-09-06T12:45:11+00:00","Rating":84,"Duration":"08:00:00"}]
```

##### Multiple Location Emissions

command: `.\caw emissions -l eastus -l westus`

output:

```text
[{"Location":"eastus","Time":"2022-08-30T12:45:11+00:00","Rating":65,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-08-30T20:45:11+00:00","Rating":65,"Duration":"08:00:00"},
// ...
{"Location":"westus","Time":"2022-09-06T04:45:11+00:00","Rating":73,"Duration":"08:00:00"},
{"Location":"westus","Time":"2022-09-06T12:45:11+00:00","Rating":84,"Duration":"08:00:00"}]
```

##### Emissions with Start and End Times

command: `.\caw emissions -l eastus --start-time 2022-07-01T00:00:00Z --end-time 2022-07-31T23:59:59Z --best`

output:

```text
[{"Location":"eastus","Time":"2022-07-01T04:45:11+00:00","Rating":65,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-07-01T12:45:11+00:00","Rating":65,"Duration":"08:00:00"},
// ...
{"Location":"eastus","Time":"2022-07-31T12:45:11+00:00","Rating":73,"Duration":"08:00:00"},
{"Location":"eastus","Time":"2022-07-31T20:45:11+00:00","Rating":84,"Duration":"08:00:00"}]
```


##### Best Emissions

command: `.\caw emissions -l eastus -l westus --start-time 2022-07-01T00:00:00Z --end-time 2022-07-31T23:59:59Z --best`

output:

```text
[{"Location":"eastus","Time":"2022-07-08T04:45:11+00:00","Rating":48,"Duration":"08:00:00"}]
```

##### Average Emissions

command: `.\caw emissions -l eastus -l westus --start-time 2022-07-09T00:00:00Z --end-time 2022-07-09T12:00:00Z --average`

output:

```text
[{"Location":"eastus","Time":"2022-07-09T00:00:00+00:00","Rating":79.357,"Duration":"12:00:00"},
{"Location":"westus","Time":"2022-07-09T00:00:00+00:00","Rating":86.91243,"Duration":"12:00:00"}]
```