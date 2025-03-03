# Configuration

- [Configuration](#configuration)
  - [Logging](#logging)
  - [DataSources](#datasources)
    - [WattTime Configuration](#watttime-configuration)
      - [username](#username)
      - [password](#password)
      - [baseUrl](#baseurl)
      - [Proxy](#proxy)
      - [WattTime Caching BalancingAuthority](#watttime-caching-balancingauthority)
      - [SignalType](#signaltype)
    - [Json Configuration](#json-configuration)
    - [ElectricityMaps Configuration](#electricitymaps-configuration)
      - [API Token Header](#api-token-header)
      - [API Token](#api-token)
      - [BaseUrl](#baseurl)
      - [Emission Factor Type](#emission-factor-type)
      - [Disable Estimations](#disable-estimations)
    - [ElectricityMapsFree Configuration](#electricitymapsfree-configuration)
      - [API Token](#api-token)
      - [BaseUrl](#baseurl)
  - [Cache](#cache)
  - [CarbonAwareVars](#carbonawarevars)
    - [Tracing and Monitoring Configuration](#tracing-and-monitoring-configuration)
    - [Verbosity](#verbosity)
    - [Prometheus exporter](#prometheus-exporter-for-emissions-data)
    - [Web API Prefix](#web-api-prefix)
  - [LocationDataSourcesConfiguration](#locationdatasourcesconfiguration)
- [Sample Configurations](#sample-configurations)
  - [Configuration for Emissions data Using WattTime](#configuration-for-emissions-data-using-watttime)
  - [Configuration for Forecast data Using ElectricityMaps](#configuration-for-forecast-data-using-electricitymaps)
  - [Configuration for Emissions data using ElectricityMaps and Forecast data using WattTime](#configuration-for-emissions-data-using-electricitymaps-and-forecast-data-using-watttime)
  - [Configuration for Emissions data using ElectricityMapsFree and Forecast data using WattTime](#configuration-for-emissions-data-using-electricitymapsfree-and-forecast-data-using-watttime)
  - [Configuration For Emissions data Using JSON](#configuration-for-emissions-data-using-json)
  - [Configuration Using WattTime and Defined Location Source Files](#configuration-using-watttime-and-defined-location-source-files)

## Logging

The default LogLevel settings for the application are found in the corresponding
`appsettings.json`, which may contain the following section -- see here for
additional details on
[Logging in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
and on
[Logging Providers in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging-providers)

```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
```

To permanently change the LogLevel, just update the `appsettings.json` for the
app. To override a LogLevel at runtime, an environment variable can set the
LogLevel value. For example to set the Logging:LogLevel:Default LogLevel to
Debug: `export Logging__LogLevel__Default="Debug"`

Example using the CLI:

```sh
cd src/CarbonAware.CLI
export Logging__LogLevel__Default="Debug"
dotnet run -l westus
```

Example using the WebApp:

```sh
cd src/CarbonAware.WebApi
export Logging__LogLevel__Default="Debug"
dotnet run
```

Or, to change the LogLevel for just one run of the app:

```sh
cd src/CarbonAware.WebApi
Logging__LogLevel__Default="Debug" dotnet run
```

## DataSources

The SDK supports multiple data sources for getting carbon data. At this time,
only a JSON file, [WattTime](https://www.watttime.org/) and
[ElectricityMaps](https://www.electricitymaps.com/) are supported.

Each data source interface is configured with a specific data source
implementation.

If set to `WattTime` or `ElectricityMaps`, the configuration specific to that
data provider must also be supplied.

`JSON` will result in the data being loaded from the file specified in the
`DataFileLocation` property

```json
{
  "DataSources": {
    "EmissionsDataSource": "Json",
    "ForecastDataSource": "WattTime",
    "Configurations": {
      "WattTime": {
        "Type": "WattTime",
        "Username": "username",
        "Password": "password",
        "BaseURL": "https://api2.watttime.org/v2/",
        "Proxy": {
          "useProxy": true,
          "url": "http://10.10.10.1",
          "username": "proxyUsername",
          "password": "proxyPassword"
        },
        "SignalType": "co2_aoer"
      },
      "ElectricityMaps": {
        "Type": "ElectricityMaps",
        "APITokenHeader": "auth-token",
        "APIToken": "myAwesomeToken",
        "BaseURL": "https://api.electricitymap.org/v3/"
      },
      "Json": {
        "Type": "Json",
        "DataFileLocation": "test-data-azure-emissions.json"
      }
    }
  }
}
```

### WattTime Configuration

If using the WattTime data source, WattTime configuration is required.

```json
{
  "username": "",
  "password": "",
  "baseUrl": "https://api2.watttime.org/v2/"
}
```

> **Sign up for a test account:** To create an account, follow these steps
> [from the WattTime documentation](https://www.watttime.org/api-documentation/#best-practices-for-api-usage)

#### username

The username you receive from WattTime. This value is required when using a
WattTime data source.

#### password

The WattTime password for the username supplied. This value is required when
using a WattTime data source.

#### baseUrl (WT)

The url to use when connecting to WattTime. Defaults to
[https://api2.watttime.org/v2/](https://api2.watttime.org/v2/).

In normal use, you shouldn't need to set this value, but this value can be used
to enable integration testing scenarios or if the WattTime url should change in
the future.

#### Proxy

This value is used to set proxy information in situations where internet egress
requires a proxy. For proxy values to be used `useProxy` must be set to `true`.
Other values should be set as needed for your environment.

```bash
  DataSources__Configurations__WattTime__UseProxy
```

#### WattTime Caching BalancingAuthority

To improve performance communicating with the WattTime API service, the client
caches the data mapping location coordinates to balancing authorities. By
default, this data is stored in an in-memory cache for `86400` seconds, but
expiration can be configured using the setting `BalancingAuthorityCacheTTL` (Set
to "0" to disable the caching feature). The regional boundaries of a balancing
authority tend to be stable, but as they can change, the
[WattTime documentation](https://www.watttime.org/api-documentation/#determine-grid-region)
recommends not caching for longer than 1 month.

```bash
DataSources__Configurations__WattTime__BalancingAuthorityCacheTTL="90"
```

#### SignalType

WattTime supports 2 signal type. They can be set as a parameter.

* `co2_moer`: Marginal operating emissions rate
* `co2_aoer`: Average operating emissions rate

If values other than these are set, an error occurs.
See [WattTime documentation](https://watttime.org/data-science/data-signals/) for details.

```bash
DataSources__Configurations__WattTime__SignalType=co2_aoer
```

### Json Configuration

By setting
`DataSources__Configurations__Json__DataFileLocation=mycustomfile.json` property
when Data source is set to `Json`, the user can specify a file that can contains
custom `EmissionsData` sets. The file should be located under the
`<user's repo>/src/data/data-sources/` directory that is part of the repository.
At build time, all the JSON files under `<user's repo>/src/data/data-sources/`
are copied over the destination directory
`<user's repo>/src/CarbonAware.WebApi/src/bin/[Debug|Publish]/net8.0/data-sources/json`
that is part of the `CarbonAware.WebApi` assembly. Also the file can be placed
where the assembly `CarbonAware.WebApi.dll` is located under `data-sources/json`
directory. For instance, if the application is installed under `/app`, copy the
file to `/app/data-sources/json`.

Emission data in JSON is cached by default, it means original data would be handled
even if JSON is updated.
You need to set `DataSources__Configurations__JSON__CacheJsonData=false`
if you want to handle updated data.

```sh
cp <mydir>/mycustomfile.json /app/data-sources/json
export DataSources__Configurations=Json
export DataSources__Configurations__JSON__Type=JSON
export DataSources__Configurations__Json__DataFileLocation=mycustomfile.json
dotnet /app/CarbonAware.WebApi.dll
```

As soon a first request is performed, a log entry shows:

```text
info: CarbonAware.DataSources.Json.JsonDataSource[0]
    Reading Json data from /app/data-sources/json/mycustomfile.json
```

### ElectricityMaps Configuration

If using the ElectricityMaps data source, ElectricityMaps configuration is
required.

**With an account token:**

> **NOTE** The ElectricityMaps API does not currently support access to
> historical forecasts. This means that functionality such as the CLI
> `emissions-forecasts` `--requested-at` flag and the API `/forecasts/batch` >
> `requestedAt` input will respond with a `NotImplemented` error.
>
> Depending on the goal, the historical measured `emissions` commands may be a
> reasonable workaround. This would treat the measured emissions as a "perfect
> historical forecast" effectively. Otherwise, use a data source that has
> support for historical forecasts, such as [WattTime](#watttime-configuration).

```json
{
  "APITokenHeader": "auth-token",
  "APIToken": "<api-token>",
  "baseUrl": "https://api.electricitymap.org/v3/"
}
```

**With a free trial token:**

```json
{
  "APITokenHeader": "X-BLOBR-KEY",
  "APIToken": "<api-token>",
  "baseUrl": "https://api-access.electricitymaps.com/<url-token>"
}
```

> **Sign up for a free trial:** Select the free trial product from
> [the ElectricityMaps catalog](https://api-portal.electricitymaps.com/). Note
> that there are some
> [restrictions](./selecting-a-data-source.md#restrictions-electricitymaps-free-trial-user)
> on the free trial product.

#### API Token Header

The API Token Header for ElectricityMaps. If you have a paid account, the header
is "auth-token". If you're using the free trial, the header is "X-BLOBR-KEY"

#### API Token

The ElectricityMaps token you receive with your account or free trial.

#### BaseUrl

The url to use when connecting to ElectricityMaps. Defaults to
"https://api.electricitymap.org/v3/" but can be overridden in the config if
needed (such as for free-trial users or enable integration testing scenarios).

#### Emission Factor Type

String value for the optional `emissionFactorType` parameter to be sent on every
ElectricityMaps API request that accepts this parameter.

See the
[ElectricityMaps API Documentation](https://static.electricitymaps.com/api/docs/index.html#emission-factors)
for more details and valid values.

#### Disable Estimations

Boolean value for the optional `disableEstimations` parameter to be sent on
every ElectricityMaps API request that accepts this parameter.

See the
[ElectricityMaps API Documentation](https://static.electricitymaps.com/api/docs/index.html#estimations)
for more details.

### ElectricityMapsFree Configuration

If using the ElectricityMapsFree data source, ElectricityMapsFree configuration
is required.

**With an account token:**

> **NOTE** The ElectricityMapsFree API does not currently support access to
> historical forecasts. This means that functionality such as the CLI
> `emissions-forecasts` > `--requested-at` flag and the API `/forecasts/batch` >
> `requestedAt` input will respond with a `NotImplemented` error.
>
> The ElectricityMapsFree API also does not currently support access to
> historical emissions data. It only supports getting the single latest
> emissions data point for the given location.
>
> If either of these restrictions are an issue, a data source that has support
> for historical forecasts, such as [WattTime](#watttime-configuration) or
> historical emissions, such as
> [ElectricityMaps](#electricitymaps-configuration) may be preferable.

```json
{
  "token": "<api-token>",
  "baseUrl": "https://api.co2signal.com/v1/"
}
```

#### API Token

The ElectricityMapsFree token you receive with your account.

#### BaseUrl

The url to use when connecting to ElectricityMapsFree. Defaults to
"https://api.co2signal.com/v1/" but can be overridden in the config if needed
(such as to enable integration testing scenarios).

## Cache

Frequent access to data sources could cause problems such as performance trouble
or exceed rate limit. To avoid them, you can configure data cache like this:

```json
{
  "EmissionsDataCache": {
    "Enabled": true,
    "ExpirationMin": 30
  }
}
```

The behavior of current cache implementation:
* Only emissions data are cached
  * Forecast data are not stored
* The result of the latest query to data sources is cached
* Use cache rather than data sources if even one datum in cache match with the query
  * Even though more data in data sources would be matched, they are not retrieved
* Cached data are stored in memory
  * They are cleard when the process of the SDK is down

## CarbonAwareVars

This section contains the global settings for the SDK. The configuration looks
like this:

```json
{
  "carbonAwareVars": {
    "TelemetryProvider": "ApplicationInsights",
    "VerboseApi": "true",
    "webApiRoutePrefix": ""
  }
}
```

### Tracing and Monitoring Configuration

The application uses Opentelemetry to create traces locally in the console. In
order to configure it to send the traces to a tracing app like Zipkin, Jaeger
etc, please refer to the Open Telemetry
[documentation](https://opentelemetry.io/).

#### Configuring with Application Insights

Application monitoring and tracing can be configured using the
`TelemetryProvider` variable in the application configuration.

```bash
CarbonAwareVars__TelemetryProvider="ApplicationInsights"
```

This application is integrated with Application Insights for monitoring
purposes. The telemetry collected in the app is pushed to AppInsights and can be
tracked for logs, exceptions, traces and more. To connect to your Application
Insights instance, configure the `ApplicationInsights_Connection_String`
variable.

```bash
ApplicationInsights_Connection_String="AppInsightsConnectionString"
```

You can alternatively configure using Instrumentation Key by setting the
`AppInsights_InstrumentationKey` variable. However, Microsoft is ending
technical support for instrumentation key�based configuration of the Application
Insights feature soon. ConnectionString-based configuration should be used over
InstrumentationKey. For more details, please refer to
[the documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string?tabs=net).

```bash
AppInsights_InstrumentationKey="AppInsightsInstrumentationKey"
```

#### Configuring telemetry log

WebAPI configures [console exporter of OpenTelemetry](https://opentelemetry.io/docs/languages/net/exporters/#console) by default.
You can configure whether the exporter is enabled with `EnableTelemetryLogging`.
Set `false` if you want to reduce the log.

```bash
CarbonAwareVars__EnableTelemetryLogging=false
```

### Prometheus exporter for emissions data

> DISCLAIMER:  The `/metrics` Prometheus exporter is currently unsupported, and is used for internal GSF needs, and may change in the future.  It will retrieve _all_ emissions data and create heavy load on your data API's.  It is turned off by default.

In the WebApi project, this application can exporse latest carbon emissions data as a prometheus exporter.

```bash
CarbonAwareVars__EnableCarbonExporter="true"
```
The scraping endpoint is `<ROOT_PATH>/metrics` like this:

```bash
http://localhost/metrics
```

By default, the exposed data are latest ones within last 24 hours. If you would like to change the period 
in some reasones, you can configure the value like this:

```json
{
  "CarbonExporter": {
    "PeriodInHours": 48
  }
}
```

### Verbosity

You can configure the verbosity of the application error messages by setting the
'VerboseApi' environment variable. Typically, you would set this value to 'true'
in the development or staging regions. When set to 'true', a detailed stack
trace would be presented for any errors in the request.

```bash
CarbonAwareVars__VerboseApi="true"
```

### Web API Prefix

Used to add a prefix to all routes in the WebApi project. Must start with a `/`.
Invalid paths will cause an exception to be thrown at startup.

By default, all controllers are off of the root path. For example:

```bash
http://localhost/emissions
```

If `webApiRoutePrefix` is set, it will allow calls to controllers using the
prefix, which can be helpful for cross cluster calls, or when proxies strip out
information from headers. For example, if this value is set to:

```bash
CarbonAwareVars__webApiRoutePrefix="/mydepartment/myapp"
```

```bash
/mydepartment/myapp
```

Then calls can be made that look like this:

```bash
http://localhost/mydepartment/myapp/emissions
```

Note that the controllers still respond off of the root path.

## LocationDataSourcesConfiguration

By setting `LocationDataSourcesConfiguration` property with one or more location
data sources, it is possible to load different `Location` data sets in order to
have more than one location. For instance by setting two location regions, the
property would be set as follow using
[environment](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#naming-of-environment-variables)
variables:

```sh
"LocationDataSourcesConfiguration__LocationSourceFiles__0__DataFileLocation": "azure-regions.json",
"LocationDataSourcesConfiguration__LocationSourceFiles__0__Prefix": "az",
"LocationDataSourcesConfiguration__LocationSourceFiles__0__Delimiter": "-",
"LocationDataSourcesConfiguration__LocationSourceFiles__1__DataFileLocation": "custom-regions.json",
"LocationDataSourcesConfiguration__LocationSourceFiles__1__Prefix": "custom",
"LocationDataSourcesConfiguration__LocationSourceFiles__1__Delimiter": "_",
```

This way when the application starts, it open the files specified by
`DataFileLocation` property that should located under `location-sources/json`
directory. The format of these files is the same as the `Location` Model class.
In order to differentiate between regions, a `Prefix` and `Delimiter` properties
are used to allow the user to select the region when a request is performed. By
settings the properties, the region should be made of
**region**=`Prefix`+`Delimiter`+`RegionName`, so when the query is performed, it
would be found. The following example shows how to perform an http request:

```sh
PREFIX=az
DELIMITER='-'
REGION=${PREFIX}${DELIMITER}eastus
curl "http://${IP_HOST}:${PORT}/emissions/bylocations/best?location=${REGION}&time=2022-05-25&toTime=2022-05-26&durationMinutes=0"
```

At build time, all the JSON files under
`<user's repo>/src/data/location-sources` are copied over the destination
directory
`<user's repo>/src/CarbonAware.WebApi/src/bin/[Debug|Publish]/net8.0/location-sources/json`
that is part of the `CarbonAware.WebApi` assembly. Also the file can be placed
where the assembly `CarbonAware.WebApi.dll` is located under
`location-sources/json` directory. For instance, if the application is installed
under `/app`, copy the file to `/app/location-sources/json`.

**Note**: Under `<user's repo>/src/data/location-sources` there is a template
file `custom-azure-zones.json.template` that can be used for locations that
don't have latitude and logitude, and the underline datasource requires a zone
name. This is the case for data source `ElectricityMaps` where the routes can be
accessible using lat/lon, but some regions are zone name based.

One can also specify these values in `appsettings.json` like this:

```json
{
  "LocationDataSourcesConfiguration": {
    "LocationSourceFiles": [
      {
        "DataFileLocation": "azure-regions.json",
        "Prefix": "az",
        "Delimiter": "-"
      },
      {
        "DataFileLocation": "custom-regions.json",
        "Prefix": "custom",
        "Delimiter": "_"
      }
    ]
  }
}
```

## Sample Configurations

### Configuration for Emissions data Using WattTime

```bash
DataSources__EmissionsDataSource="WattTime"
CarbonAwareVars__WebApiRoutePrefix="/microsoft/cse/fsi"
DataSources__Configurations__WattTime__Proxy__UseProxy=true
DataSources__Configurations__WattTime__Proxy__Url="http://10.10.10.1"
DataSources__Configurations__WattTime__Proxy__Username="proxyUsername"
DataSources__Configurations__WattTime__Password="proxyPassword"
DataSources__Configurations__WattTime__Username="wattTimeUsername"
DataSources__Configurations__WattTime__Password="wattTimePassword"
```

## Configuration for Forecast data Using ElectricityMaps

```json
{
  "DataSources": {
    "ForecastDataSource": "ElectricityMaps",
    "Configurations": {
      "ElectricityMaps": {
        "Type": "ElectricityMaps",
        "APITokenHeader": "auth-token",
        "APIToken": "token",
        "BaseURL": "https://api.electricitymap.org/v3/"
      }
    }
  }
}
```

## Configuration for Emissions data using ElectricityMaps and Forecast data using WattTime

```json
  "DataSources": {
    "EmissionsDataSource": "ElectricityMaps",
    "ForecastDataSource": "WattTime",
    "Configurations": {
      "WattTime": {
        "Type": "WattTime",
        "Username": "username",
        "Password": "password",
        "BaseURL": "https://api2.watttime.org/v2/",
      },
      "ElectricityMaps": {
        "Type": "ElectricityMaps",
        "APITokenHeader": "auth-token",
        "APIToken": "token",
        "BaseURL": "https://api.electricitymap.org/v3/"
      }
    }
  }
```

## Configuration for Emissions data using ElectricityMapsFree and Forecast data using WattTime

```json
  "DataSources": {
    "EmissionsDataSource": "ElectricityMapsFree",
    "ForecastDataSource": "WattTime",
    "Configurations": {
      "WattTime": {
        "Type": "WattTime",
        "Username": "username",
        "Password": "password",
        "BaseURL": "https://api2.watttime.org/v2/",
      },
      "ElectricityMapsFree": {
        "Type": "ElectricityMapsFree",
        "token": "token",
        "BaseURL": "https://api.co2signal.com/v1/"
      }
    }
  }
```

### Configuration For Emissions data Using JSON

```json
{
  "DataSources": {
    "EmissionsDataSource": "Json",
    "Configurations": {
      "Json": {
        "Type": "Json",
        "DataFileLocation": "test-data.json"
      }
    }
  }
}
```

### Configuration Using WattTime and Defined Location Source Files

```json
{
    "DataSources": {
        "EmissionsDataSource": "WattTime",
        "Configurations": {
          "WattTime": {
            "Type": "WattTime",
            "Username": "user",
            "Password": "password"
        }
    },
    "locationDataSourcesConfiguration": {
        "locationSourceFiles": [
            {
                "prefix": "az",
                "delimiter": "-",
                "dataFileLocation": "azure-regions.json"
            },
            {
                "prefix": "custom",
                "delimiter": "_",
                "dataFileLocation": "custom-regions.json"
            }
        ]
    }
}
```
