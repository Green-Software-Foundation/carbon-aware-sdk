# Packaging the Carbon Aware SDK

With the addition of the C# Client Library as a way to consume the Carbon Aware
SDK, we have also added [bash scripts](../scripts/package/) to package the
library, and have included a sample [Console App](../samples/lib-integration/)
showing how the package can be consumed.

- [Packaging the Carbon Aware SDK](#packaging-the-carbon-aware-sdk)
  - [Included Projects](#included-projects)
  - [Included Scripts](#included-scripts)
    - [Running the packaging scripts](#running-the-packaging-scripts)
  - [SDK Configuration](#sdk-configuration)
    - [ElectricityMaps](#electricitymaps)
    - [WattTime](#watttime)
    - [Json](#json)
  - [Use Package with Dependency Injection](#use-package-with-dependency-injection)
    - [Console App Sample](#console-app-sample)

## Included Projects

The current package include 8 projects from the SDK:

1. "GSF.CarbonAware"
2. "CarbonAware"
3. "CarbonAware.Aggregators"
4. "CarbonAware.DataSources.ElectricityMaps"
5. "CarbonAware.DataSources.Json"
6. "CarbonAware.DataSources.Registration"
7. "CarbonAware.DataSources.WattTime"
8. "CarbonAware.LocationSources.Azure"

These 8 projects enable users of the library to consume the current endpoints
exposed by the library. The package that needs to be added to a new C# project
is `GSF.CarbonAware`.

## Included Scripts

There are 2 scripts included to help the packaging process

1. `create_package.sh <dotnet_solution> <package_destination>`
2. `add_package.sh <dotnet_project> <package_destination>`

The [`create_package`](../scripts/package/create_packages.sh) script is called
with 2 parameters: the CarbonAwareSDK dotnet solution file (`.sln`) path, and
the output directory destination for the package. The
[`add_package`](../scripts/package/add_package.sh) script is also called with 2
parameters: the target project file (`.csproj`) path, and the package
destination path.

To see a working example of both scripts being invoked, you can look at the
github action detailed in
[build-packages.yaml](../.github/workflows/build-packages.yaml).

### Running the packaging scripts

The packaging scripts can be run inside a VS Code dev container defined in this
project. When running in the dev container you will need:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [VSCode](https://code.visualstudio.com/)
- [Remote Containers extension for VSCode](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

Alternatively you can run in your local environment using the
[.NET Core 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

## SDK Configuration

The configuration needed to connect to WattTime or Json data sources can be
managed using environment variables or appsettings. More information on data
source configuration can be found
[here](https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/docs/configuration.md#datasources)

### ElectricityMaps

Below are the environment variables (i.e. bash shell) needed to set up the
**WattTime** data source.

```bash
export DataSources__ForecastDataSource=ElectricityMaps
export DataSources__Configurations__ElectricityMaps__Type=ElectricityMaps
export DataSources__Configurations__ElectricityMaps__APITokenHeader=[ElectricityMaps APITokenHeader]
export DataSources__Configurations__ElectricityMaps__APIToken=[ElectricityMaps APIToken]`
```

### WattTime

Below are the environment variables (i.e. bash shell) needed to set up the
**WattTime** data source.

```bash
export DataSources__EmissionsDataSource=WattTime`
export DataSources__ForecastDataSource=WattTime`
export DataSources__Configurations__WattTime__Type=WattTime`
export DataSources__Configurations__WattTime__Username=[WattTime Username]`
export DataSources__Configurations__WattTime__Password=[WattTime Password]`
export DataSources__Configurations__WattTime__BaseURL="https://api2.watttime.org/v2/"`
```

### Json

Below is the environment variable (i.e. bash shell) needed to set up the
**Json** data source.

```bash
export DataSources__EmissionsDataSource=Json`
export DataSources__Configurations__Json__Type=Json`
export DataSources__Configurations__Json__DataFileLocation="test-data-azure-emissions.json"`
```

## Use Package with Dependency Injection

In order to get access to the
[handlers](./architecture/c%23-client-library.md#handlers) in the library, a
common practice with C# is through `Microsoft.Extensions.DependencyInjection`
extensions. This way the whole life cycle of the handler instance is managed by
the container’s framework, and it would help to isolate the concrete
implementation from the user facing interface. For instance, a consumer would be
able to call extensions as:

```c#
// Using DI Services (Emissions) to register GSF SDK library
services.AddEmissionsServices(configuration);
```

```c#
// An application Consumer construct should inject a GSF handler like the following example
class ConsumerClass(IEmissionsHandler handler, ILogger<ConsumerClass> logger)
{
    ....
    this._handler = handler;
    this._logger = logger;
    ....
}
```

And the usage of a method for IEmissionsHandler

```c#
async Task<double> GetRating()
{
    ...
    return await this._handler.GetAverageCarbonIntensity(…);
}
```

Another functionality of the application could just do Forecast data. So, it
would be a matter of following the same pattern:

```c#
// Using DI Services (Forecast) to register GSF SDK library
services.AddForecastServices(configuration);
```

```c#
class ForecastApp(IForecastHandler handler)
{
    ...
    this._handler = handler;
}
```

And the usage of a method for IForecastHandler:

```c#
async Task<EmissionsData> GetOptimal(…)
{
    ...
    return await this._handler.GetCurrentAsync()...).OptimalDataPoints.First();
}
```

This way it would fit within the same stack as the rest of the SDK is
implemented. Also, it would be easier to integrate later when the current
consumers (CLI/WebApi) should be moved to use the library.

## Console App Sample

There is a sample console app in the
[lib integration folder](../samples/lib-integration/ConsoleApp/) to demonstrate
package creation and interaction with Carbon Aware SDK.

### Run the Sample Console App

In order to build and run the app, all the dependent packages need to be created
first and then imported in the app. Follow the steps below to run the sample
console app -

- Run the [script commands](#included-scripts) to create the packages and add
  them into the app.
- Create the [environment variables](#sdk-configuration) to connect to the
  WattTime or Json data sources.
