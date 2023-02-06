# Java Client Example

This folder contains an example for WebAPI client in Java. Client library would
be generated dynamically via
[openapi-generator-maven-plugin](https://github.com/OpenAPITools/openapi-generator/tree/master/modules/openapi-generator-maven-plugin),
and call WebAPI endpoints without HTTP code.

Javadoc is [here](apidocs).

openapi-generator-maven-plugin generates Maven/Gradle project when it kicks,
however this example uses generated codes directly. So you don't need to
run/modify project files in it.

## Requirements

- OpenAPI spec file
  - Both online and offline file are available.
  - See [WebAPI document](../../docs/carbon-aware-webapi.md#autogenerate-webapi)
    for details.
- WebAPI instance
  - See [Getting Started](../../GettingStarted.md#publish-webapi-with-container)
    if you'd like to start it on container.
- Java 8 or later
- Maven

## Client code

[WebApiClient.java](src/main/java/foundation/greensoftware/carbonawaresdk/samples/java/WebApiClient.java)
is an example program to call WebAPI endpoint. It calls all of endpoints, and
shows the result.

Following methods are available:

- `processBestEmissionsDataByLocations`
  - Call /emissions/bylocations/best
  - Shows result of the best emission data for west/central/east US yesterday.
- `processEmissionsDataByLocations`
  - Call /emissions/bylocations
  - Shows top of result of emission data for west/central/east US yesterday.
- `processEmissionsDataByLocation`
  - Call /emissions/bylocation
  - Shows top of result of emission data for westus yesterday.
- `processCurrentForecastData`
  - Call /emissions/forecasts/current
  - Shows top of forecast data for westus.
- `processForecastBatchData`
  - Call /emissions/forecasts/batch
  - Shows forecast data for westus.
- `processAverageData`
  - Call /emissions/average-carbon-intensity .
  - Shows average data for westus yesterday.
- `processAverageBatchData`
  - Call /emissions/average-carbon-intensity/batch
  - Shows average data for westus yesterday.

[OffsetDateTime](https://docs.oracle.com/javase/8/docs/api/java/time/OffsetDateTime.html)
is used for parameters in each APIs. However the error occurs if nano sec is set
to it in case of WattTime. So it is highly recommended that clears nanosec field
like `withNano(0)`.

## How it works

### 1. Set up for [POM](pom.xml)

You need to change following properties:

- `openapi.spec`
  - OpenAPI spec file
- `webapi.endpoint`
  - WebAPI base URL

### 2. Build

```sh
$ mvn clean package
```

### 3. Run

```sh
$ mvn exec:java
```

### Running in container

This example also can run in container. You can use
[Maven official image](https://hub.docker.com/_/maven).

If you want to run both WebAPI and build process in container, you need to join
2 containers to same network.

Following instructions are for Podman.

#### 1. Create pod

This pod publishes port 80 in the pod to 8080 on the host, then you can access
WebAPI in the pod. The pod is named to `carbon-aware-sdk`.

```sh
podman pod create -p 8080:80 --name carbon-aware-sdk
```

#### 2. Start WebAPI container

Start WebAPI container in `carbon-aware-sdk` pod. It is specified at `--pod`
option.

See [Getting Started](../../GettingStarted.md) to build container image.

```sh
$ podman run -it --rm --pod carbon-aware-sdk \
    -e CarbonAwareVars__CarbonIntensityDataSource="WattTime" \
    -e WattTimeClient__Username="wattTimeUsername" \
    -e WattTimeClient__Password="wattTimePassword" \
  carbon-aware-sdk-webapi
```

#### 3. Run Maven in the container

Run `mvn` command in Maven container in `catbon-aware-sdk` pod. You need to
mount Carbon Aware SDK source directory to the container. It mounts to `/src` in
the container in following case.

In following command, you can rebuild java-client, and can run the artifact. You
can get artifacts from `samples/java-client/target` on the container host of
course.

```sh
$ podman run -it --rm --pod carbon-aware-sdk \
    -v `pwd`/carbon-aware-sdk:/src:Z  \
  docker.io/maven:3.8-eclipse-temurin-8 \
    mvn -f /src/samples/java-client/pom.xml clean package exec:java
```

Maven will download many dependencies in each `mvn` call. You can avoid it when
you mount `.m2` like `-v $HOME/.m2:/root/.m2` because it shares Maven cache
between the host and the container.
