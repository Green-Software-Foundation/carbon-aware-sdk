# Overview

There are several ways to consume CarbonAware data for your use case. Each
approach surfaces the same data for the same call (e.g. the CLI should not give
you different data than the WebAPI for the same query). We provide a number of
different endpoints to provide the most flexibility to integrate to your
environment:

- You can run the application using the [CLI](./src/CarbonAware.CLI) and refer
  to more documentation [here](./carbon-aware-cli.md).

- You can build a container containing the [WebAPI](./src/CarbonAware.WebApi)
  and connect via REST requests and refer to more documentation
  [here](./carbon-aware-webapi.md).

- You can reference the [Carbon Aware C# Library](./src/GSF.CarbonAware) in your
  projects and make use of its functionalities and features.

- (Future) You can install the Nuget package and make requests directly.
  ([tracked here](https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues/40))

Each of these has configuration requirements which are detailed below. You can
also visit the [quickstart.md](docs/quickstart.md) guide for a step-by-step
process for running the CLI locally, deploying the Web API locally or in the
cloud, polling the API via HTTP requests or generating and using client
libraries (Python example).

For more detailed architecture and design decisions around the Carbon Aware SDK,
refer to the [Architecture directory](./architecture/).

## Carbon Aware Library

The Carbon Aware SDK provides a C# Client Library with handlers that replicates
the Web API, CLI and SDK functionality. See:

- [carbon-aware-library.md](./carbon-aware-library.md) for more information
  about library features.
- [packaging.md](./packaging.md) for details on how to package and consume the
  library.
- [gsf-carbon-aware-library-package.md](./gsf-carbon-aware-library-package.md)
  for instructions on integrating the library in other projects with dependency
  injection.

## Pre-requisites

Make sure you have installed the following pre-requisites to setup your local
environment:

- dotnet core SDK
  [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download)
- Access to one (or all) of the supported external data APIs
  - WattTime account - See
    [instruction on WattTime](https://www.watttime.org/api-documentation/#register-new-user)
    for details (or use our python samples as described
    [here](samples/watttime-registration/readme.md)).
  - ElectricityMaps account - See
    [instruction on ElectricityMaps](https://api-portal.electricitymaps.com/home)
    for details (or setup a
    [free trial](https://api-portal.electricitymaps.com)). Note that the free
    trial has some
    [restrictions](./docs/selecting-a-data-source.md#restrictions-electricitymaps-free-trial-user)
  - ElectricityMapsFree account - See
    [instruction on ElectricityMapsFree](https://www.co2signal.com/#Subscriber-Email)
    for details.

Alternatively, you can also set up your environment using VSCode Remote
Containers (Dev Container):

- Docker
- VSCode (it is recommended to work in a Dev Container)
- [Remote Containers extension for VSCode](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

## Data Sources

We support multiple data sources for carbon data. At this time, a JSON file,
[WattTime](https://www.watttime.org/),
[ElectricityMaps](https://www.electricitymaps.com/), and
[ElectricityMapsFree](https://www.co2signal.com/) are supported. To use WattTime
data or Electricity Maps data, you'll need to acquire a license from them and
set the appropriate configuration information.

You can also visit the
[selecting-a-date-source.md](docs/../selecting-a-data-source.md) guide for more
information on data sources options, and
[data-sources.md](./architecture/data-sources.md) for detailed architecture
decisions around integrating different data providers into the carbon aware SDK.

## Configuration

This project uses the dotnet standard
[Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration)
mechanism, which allows the user to configure their environment variables in a
unified view while making use of different configuration sources. Review the
link to understand more about the `IConfiguration` type.

The WebAPI project uses standard configuration sources provided by
[ASPNetCore](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/).
Please review this link to understand how configuration is loaded and the
priority of that configuration.

Please note that configuration is hierarchical. The last configuration source
loaded that contains a configuration value will be the value that's used. This
means that if the same configuration value is found in both `appsettings.json`
and as an environment variable, the value from the environment variable will be
the value that's applied.

### Configuration options

See [configuration.md](/docs/configuration.md) for details about how to
configure specific components of the application.

#### Environment variables

When adding values via environment variables, we recommend that you use the
double underscore form, rather than the colon form. Colons won't work in
non-windows environment. For example:

```bash
  DataSources__EmissionsDataSource="WattTime"
```

Note that double underscores are used to represent dotted notation or child
elements that you see in the JSON below. For example, to set proxy information
using environment variables, you'd do this:

```bash
  DataSources__Configurations__WattTime__UseProxy
```

#### Local project settings

For local-only settings you can use environment variables,
[the Secret Manager tool](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#secret-manager)
, or an untracked Development appsettings file to override the default project
settings.

To use the settings file, rename a copy of the template called
`appsettings.Development.json.template` to `appsettings.Development.json` and
remove the first line of (invalid) comments. Then update any settings according
to your preferences.

> Wherever possible, the projects leverage the
> [default .NET configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#default-application-configuration-sources)
> expectations. Thus, they can be configured using any file matching the format:
> `appsettings.<ENV>.json`. Where `<ENV>` is the value of the
> `ASPNETCORE_ENVIRONMENT` environment variable. By convention projects tend to
> use the provided HostEnvironment constants `Development`, `Staging`, and
> `Production`.

## Publish WebAPI with container

You can publish Web API for Carbon Aware SDK with container. These instructions
show how to build / run container image with [Podman](https://podman.io/).

### Build container image

Following commands build the container which named to `carbon-aware-sdk-webapi`
from sources.

```bash
$cd src
$podman build -t carbon-aware-sdk-webapi -f CarbonAware.WebApi/src/Dockerfile .
```

### Run Web API container

Carbon Aware SDK Web API publishes the service on Port 80, so you need to map it
to local port. Following commands maps it to Port 8080.

You also need to configure the SDK with environment variables. They are minimum
set when you use WattTime or ElectricityMaps or ElectricityMapsFree as a data
source.

```bash
$ podman run -it --rm -p 8080:80 \
    -e DataSources__ForecastDataSource="WattTime" \
    -e DataSources__Configurations__WattTime__Type="WattTime" \
    -e DataSources__Configurations__WattTime__Username="wattTimeUsername" \
    -e DataSources__Configurations__WattTime__Password="wattTimePassword" \
  carbon-aware-sdk-webapi
```

or

```bash
$ podman run -it --rm -p 8080:80 \
    -e DataSources__ForecastDataSource="ElectricityMaps" \
    -e DataSources__Configurations__ElectricityMaps__Type="ElectricityMaps" \
    -e DataSources__Configurations__ElectricityMaps__APITokenHeader="auth-token" \
    -e DataSources__Configurations__ElectricityMaps__APIToken="electricityMapsToken" \
  carbon-aware-sdk-webapi
```

or

```bash
$ podman run -it --rm -p 8080:80 \
    -e DataSources__EmissionsDataSource="ElectricityMapsFree" \
    -e DataSources__Configurations__ElectricityMapsFree__Type="ElectricityMapsFree" \
    -e DataSources__Configurations__ElectricityMapsFree__token="<YOUR_CO2SIGNAL_TOKEN>" \
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

For more information on containerization, refer to the markdown in
[containerization.md](./containerization.md).

### Deploy Web API on Kubernetes with Helm

You can deploy Web API as a Kubernetes application via Helm. GSF provides a chart as an OCI container, so you have to use Helm v3.8.0 or later.

Following command creates `carbon-aware-sdk` namespace and deploys Web API into it with specified `values.yaml`.

```bash
$ helm install casdk -n carbon-aware-sdk --create-namespace oci://ghcr.io/green-software-foundation/charts/carbon-aware-sdk --values values.yaml
```

`values.yaml` should contain `appsettings.json` which would be used in Web API at least. It should include data source definitions and their credentials. It would be stored as `Secret` resource.

```yaml
appsettings: |-
  {
    "DataSources": {
      "EmissionsDataSource": "WattTime",
      "ForecastDataSource": "WattTime",
      "Configurations": {
        "WattTime": {
          "Type": "WattTime",
          "Username": "username",
          "Password": "password",
          "BaseURL": "https://api2.watttime.org/v2/"
        }
      }
    }
  }
```

Also you can include following configuration into `values.yaml`.

```yaml
# Number of replicas
replicaCount: 1

image:
  repository: ghcr.io/green-software-foundation/carbon-aware-sdk
  pullPolicy: IfNotPresent
  # You can set specified tag (equivalent with the SDK version in here)
  tag: latest

# Set the value if you want to override the name.
nameOverride: ""
fullnameOverride: ""

podAnnotations: {}

service:
  type: ClusterIP
  port: 80

ingress:
  enabled: false
  className: ""
  annotations: {}
  hosts:
    - host: carbon-aware-sdk.local
      paths:
        - path: /
          pathType: ImplementationSpecific
  tls: []
  #  - secretName: carbon-aware-sdk-tls
  #    hosts:
  #      - carbon-aware-sdk.local

resources: {}
  # limits:
  #   cpu: 100m
  #   memory: 128Mi
  # requests:
  #   cpu: 100m
  #   memory: 128Mi

autoscaling:
  enabled: false
  minReplicas: 1
  maxReplicas: 100
  targetCPUUtilizationPercentage: 80
  # targetMemoryUtilizationPercentage: 80

nodeSelector: {}

tolerations: []

affinity: {}

# appsettings.json
appsettings: |-
  {
    "DataSources": {
      "EmissionsDataSource": "ElectricityMaps",
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
          }
        },
        "ElectricityMaps": {
          "Type": "ElectricityMaps",
          "APITokenHeader": "auth-token",
          "APIToken": "myAwesomeToken",
          "BaseURL": "https://api.electricitymap.org/v3/"
        }
      }
    }
  }
```
