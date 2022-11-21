# Packaging the Carbon Aware SDK

With the addition of the C# Client Library as a way to consume the Carbon Aware SDK, we have also added [bash scripts](../scripts/package/) to package the library, and have included a sample [Console App](../samples/lib-integration/) showing how the package can be consumed.

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
9. "CarbonAware.Tools.WattTimeClient"

These 8 projects enable users of the library to consume the current endpoints exposed by the library. The package that needs to be added to a new C# project is `GSF.CarbonAware`.

## Included Scripts

There are 2 scripts included to help the packaging process

1. `create_package.sh <dotnet_solution> <package_destination>`
2. `add_package.sh <dotnet_project> <package_destination>`

The [`create_package`](../scripts/package/create_packages.sh) script is called with 2 parameters: the CarbonAwareSDK dotnet solution file (`.sln`) path, and the output directory destination for the package. The [`add_package`](../scripts/package/add_package.sh) script is also called with 2 parameters: the target project file (`.csproj`) path, and the package destination path.

To see a working example of both scripts being invoked, you can look at the github action detailed in [build-packages.yaml](../.github/workflows/build-packages.yaml).

### Running the packaging scripts

The packaging scripts can be run inside a VS Code dev container defined in this project. When running in the dev container you will need:

* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* [VSCode](https://code.visualstudio.com/)
* [Remote Containers extension for VSCode](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

Alternatively you can run in your local environment using the [.NET Core 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

## SDK Configuration

The configuration needed to connect to WattTime or Json data sources can be managed using environment variables or appsettings.

### ElectricityMaps

Below are the environment variables (i.e. bash shell) needed to set up the **WattTime** data source.

* `export DataSources__ForecastDataSource=ElectricityMaps`
* `export DataSources__Configurations__ElectricityMaps__Type=ElectricityMaps`
* `export DataSources__Configurations__ElectricityMaps__APITokenHeader=[ElectricityMaps APITokenHeader]`
* `export DataSources__Configurations__ElectricityMaps__APIToken=[ElectricityMaps APIToken]`

### WattTime

Below are the environment variables (i.e. bash shell) needed to set up the **WattTime** data source.

* `export DataSources__EmissionsDataSource=WattTime`
* `export DataSources__ForecastDataSource=WattTime`
* `export DataSources__Configurations__WattTime__Type=WattTime`
* `export DataSources__Configurations__WattTime__Username=[WattTime Username]`
* `export DataSources__Configurations__WattTime__Password=[WattTime Password]`
* `export DataSources__Configurations__WattTime__BaseURL="https://api2.watttime.org/v2/"`

### Json

Below is the environment variable (i.e. bash shell) needed to set up the **Json** data source.

* `export DataSources__EmissionsDataSource=Json`
* `export DataSources__Configurations__Json__Type=Json`
* `export DataSources__Configurations__Json__DataFileLocation="test-data-azure-emissions.json"`

## Console App Sample

There is a sample console app in the [lib integration folder](../samples/lib-integration/ConsoleApp/). The app shows how to use dependency injection to pull in the packages and interact with the SDK.

In order to run the sample console app, you will need to

* Run the [script commands](#included-scripts) to create the packages and add them into the app
* Create the [environment variables](#sdk-configuration) to connect to the WattTime or Json data sources
