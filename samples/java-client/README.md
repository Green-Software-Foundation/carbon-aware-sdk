# Java Client Example

This folder contains an example for WebAPI client in Java. Client library would be generated dynamically via [openapi-generator-maven-plugin](https://github.com/OpenAPITools/openapi-generator/tree/master/modules/openapi-generator-maven-plugin), and call WebAPI endpoints without HTTP code.

Javadoc is [here](apidocs).

openapi-generator-maven-plugin generates Maven/Gradle project when it kicks, however this example uses generated codes directly. So you don't need to run/modify project files in it.

## Requirements

* OpenAPI spec file
    * Both online and offline file are available.
    * See [WebAPI document](../../docs/carbon-aware-webapi.md#autogenerate-webapi) for details.
* WebAPI instance
    * See [Getting Started](../../GettingStarted.md#publish-webapi-with-container) if you'd like to start it on container.
* Java 8 or later
* Maven

## Client code

[WebApiClient.java](src/main/java/foundation/greensoftware/carbonawaresdk/samples/java/WebApiClient.java) is an example program to call WebAPI endpoint. It calls all of endpoints, and shows the result.

Following methods are available:

* `processBestEmissionsDataByLocations`
    * Call /emissions/bylocations/best
    * Shows result of the best emission data for west/central/east US yesterday.
* `processEmissionsDataByLocations`
    * Call /emissions/bylocations
    * Shows top of result of emission data for west/central/east US yesterday.
* `processEmissionsDataByLocation`
    * Call /emissions/bylocation
    * Shows top of result of emission data for westus yesterday.
* `processCurrentForecastData`
    * Call /emissions/forecasts/current
    * Shows top of forecast data for westus.
* `processForecastBatchData`
    * Call /emissions/forecasts/batch
    * Shows forecast data for westus.
* `processAverageData`
    * Call /emissions/average-carbon-intensity .
    * Shows average data for westus yesterday.
* `processAverageBatchData`
    * Call /emissions/average-carbon-intensity/batch
    * Shows average data for westus yesterday.

[OffsetDateTime](https://docs.oracle.com/javase/8/docs/api/java/time/OffsetDateTime.html) is used for parameters in each APIs. However the error occurs if nano sec is set to it in case of WattTime. So it is highly recommended that clears nanosec field like `withNano(0)`.

## How it works

### 1. Set up for [POM](pom.xml)

You need to change following properties:

* `openapi.spec`
    * OpenAPI spec file
* `webapi.endpoint`
    * WebAPI base URL

### 2. Build

```sh
$ mvn clean package
```

### 3. Run

```sh
$ mvn exec:java
```
