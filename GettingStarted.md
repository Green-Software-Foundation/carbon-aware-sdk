# Getting Started

This SDK has several entry points:

- You can run the application using the [CLI](./src/CarbonAware.CLI).

- You can build a container containing the [WebAPI](./src/CarbonAware.WebApi) and connect via REST requests.

- (Future) You can install the Nuget package and make requests directly. ([tracked here](https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues/40))

Each of these has configuration requirements which are detailed below. You can also visit the [quickstart.md](docs/quickstart.md) guide for a step-by-step process for running the CLI locally, deploying the Web API locally, polling the API via HTTP requests or generating and using client libraries (Python example).

## Pre-requisites

Make sure you have installed the following pre-requisites:

- dotnet core SDK [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download)
- WattTime account - See [instruction on WattTime](https://www.watttime.org/api-documentation/#register-new-user) for details (or use our python samples as described [here](samples/watttime-registration/readme.md)).

## Data Sources

We intend to support multiple data sources for carbon data.  At this time, only a JSON file and [WattTime](https://www.watttime.org/) are supported.  To use WattTime data, you'll need to acquire a license from them and set the appropriate configuration information.

## Configuration

This project uses standard [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) mechanisms.

The WebAPI project uses standard configuration sources provided by [ASPNetCore](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/).  Please review this link to understand how configuration is loaded and the priority of that configuration.

Please note that configuration is hierarchical.  The last configuration source loaded that contains a configuration value will be the value that's used.  This means that if the same configuration value is found in both `appsettings.json` and as an environment variable, the value from the environment variable will be the value that's applied.

### Configuration options

See [configuration.md](/docs/configuration.md) for details about how to configure specific components of the application.

#### Environment variables
When adding values via environment variables, we recommend that you use the double underscore form, rather than the colon form.  Colons won't work in non-windows environment.  For example:

```bash
  CarbonAwareVars__EmissionsDataSource="WattTime"
```

Note that double underscores are used to represent dotted notation or child elements that you see in the JSON below.  For example, to set proxy information using environment variables, you'd do this:

```bash
  DataSources__Configurations__WattTime__UseProxy
```

#### Local project settings

For local-only settings you can use environment variables, [the Secret Manager tool](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#secret-manager), or an untracked Development appsettings file to override the default project settings.

To use the settings file, rename a copy of the template called `appsettings.Development.json.template` to `appsettings.Development.json` and remove the first line of (invalid) comments. Then update any settings according to your preferences.

> Wherever possible, the projects leverage the [default .NET configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#default-application-configuration-sources) expectations.  Thus, they can be configured using any file matching the format: `appsettings.<ENV>.json`. Where `<ENV>` is the value of the `ASPNETCORE_ENVIRONMENT` environment variable. By convention projects tend to use the provided HostEnvironment constants `Development`, `Staging`, and `Production`.

## Publish WebAPI with container

You can publish Web API for Carbon Aware SDK with container. This instruction shows how to build / run container image with [Podman](https://podman.io/).

### Build container image

Following commands build the container which named to `carbon-aware-sdk-webapi` from sources.

```bash
$ cd src
$ podman build -t carbon-aware-sdk-webapi -f CarbonAware.WebApi/src/Dockerfile .
```

### Run Web API container

Carbon Aware SDK Web API publishes the service on Port 80, so you need to map it to local port. Following commands maps it to Port 8080.

You also need to configure the SDK with environment variables. They are minimum set when you use WattTime as a data source.

```bash
$ podman run -it --rm -p 8080:80 \
    -e DataSources__EmissionsDataSource="WattTime" \
    -e DataSources__Configurations__WattTime__Username="wattTimeUsername" \
    -e DataSources__Configurations__WattTime__Password="wattTimePassword" \
  carbon-aware-sdk-webapi
```

When you success to run the container, you can access it via HTTP client.

```bash
$ curl -s http://localhost:8080/emissions/forecasts/current?location=westus2 | jq
[
  {
    "generatedAt": "2022-08-10T14:10:00+00:00",
    "optimalDataPoint": {
      "location": "GCPD",
      "timestamp": "2022-08-10T20:40:00+00:00",
      "duration": 5,
      "value": 440.4361702590741
    },
            :
```
