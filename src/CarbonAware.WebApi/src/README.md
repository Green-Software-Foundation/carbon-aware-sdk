The Carbon Aware SDK provides an API to get the marginal carbon intensity for a given location and time period. The values reported in the Green Software Foundation's specification for marginal carbon intensity (Grams per Kilowatt Hour). In order to use the API, the environment needs to be prepared with a set of configuration parameters. Instructions on setting up the environment could be found here - https://github.com/microsoft/carbon-aware-sdk/blob/dev/GettingStarted.md

# Carbon Aware REST API

- [Carbon Aware REST API](#carbon-aware-rest-api)
  - [Endpoints](#endpoints)
    - [POST /sci-scores](#post-sci-scores)
    - [POST /sci-scores/marginal-carbon-intensity](#post-sci-scoresmarginal-carbon-intensity)
    - [GET emissions/bylocation](#get-emissionsbylocation)
    - [GET emissions/bylocations](#get-emissionsbylocations)
    - [GET emissions/bylocations/best](#get-emissionsbylocationsbest)
    - [GET forecasts/current](#get-forecastscurrent)
  - [Error Handling](#error-handling)
  - [Autogenerate WebAPI](#autogenerate-webapi)
  
## Endpoints

### POST /sci-scores

This endpoint calculates the SCI score using the [Green Software Foundation SCI specification formula](https://github.com/Green-Software-Foundation/software_carbon_intensity/blob/main/Software_Carbon_Intensity/Software_Carbon_Intensity_Specification.md#methodology-summary).

> ((E \* I) + M)/R

- (E) Energy
- (I) Marginal Carbon Intensity
- (M) Embodied Emissions
- (R) Functional Unit

The payload object must include `location` and `timeInterval`.

If location is of type `CloudProvider`, the object should include the `providerName` and `regionName` attributes.
If location if of type `Geoposition` then the object should include `latitude` and `longitude` attributes.

EG
```
{
    "location": {
        "locationType": "CloudProvider",
        "providerName": "Azure",
        "regionName": "uswest"
    },
    "timeInterval": "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z"
}
```

The response object MUST include the SCI score and the component variables. EG

```
{
  "sciScore": 80.0,
  "energyValue": 1.0,
  "marginalCarbonIntensityValue": 750.0,
  "embodiedEmissionsValue": 50.0,
  "functionalUnitValue": 10
}
```

### POST /sci-scores/marginal-carbon-intensity

This endpoint calculates just the Average Marginal Carbon Intensity which is the `I` portion of the Green Software Foundation specification.  This is useful if you only need to report this value, or if another service is responsible for the other parts of your SCI score. 

The payload object must include location and timeInterval, it is the same as the payload for the `/sci-scores` endpoint and follows the same `locationType` requirements as above.

EG
```
{
    "location": {
        "locationType": "Geoposition",
        "latitude": -37.814,
        "longitude": 144.96332
    },
    "timeInterval": "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z"
}
```

The response object is the same SciScore object as above, but only `marginalCarbonIntensityValue` is populated, the other attributes are set to `null`.

EG
```
{
  "sciScore": null,
  "energyValue": null,
  "marginalCarbonIntensityValue": 750.0,
  "embodiedEmissionsValue": null,
  "functionalUnitValue": null
}
```

### GET emissions/bylocation

This endpoint calculates the observed emission data by location for a specified time period.

Location is a required parameter and is name of the data region for the configured Cloud provider.
If time period is not provided, it retrieves available data until the current time.

EG
```
https://<server_name>/emissions/bylocation?location=useast&time=2022-01-01&toTime=2022-05-17
```

The response is an array of EmissionsData objects that contains the location, time and the rating in g/kWh
EG
```
[
 {
  "location":"eastus",
  "time":"2022-05-17T20:45:11.5092741+00:00",
  "rating":70
 }
]
```

### GET emissions/bylocations

This endpoint calculates the observed emission data by an array of locations for a specified time period

Location is a required parameter and is an array of the names of the data region for the configured Cloud provider.
If time period is not provided, it retrieves all the data until the current time.

EG
```
https://<server_name>/emissions/bylocations?locations=eastus&locations=westus&time=2022-01-01&toTime=2022-05-17
```

The response is an array of EmissionsData objects that contains the location, time and the rating in g/kWh.
EG
```
[
  {
    "location":"eastus"
    "time":"2022-05-17T20:45:11.5092741+00:00",
    "rating":70
  }
]
```

### GET emissions/bylocations/best

This endpoint calculates the best observed emission data by an array of locations for a specified time period

Location is a required parameter and is an array of the names of the data region for the configured Cloud provider.
If time period is not provided, it retrieves all the data until the current time.

EG
```
https://<server_name>/emissions/bylocations/best?locations=eastus&locations=westus&time=2022-01-01&toTime=2022-05-17
```

The response is an array of EmissionsData objects that contains the location, time and the rating in g/kWh.
EG
```
[
  {
    "location":"eastus"
    "time":"2022-05-17T20:45:11.5092741+00:00",
    "rating":70
  }
]
```


### GET forecasts/current

This endpoint fetches the most recent forecast for all provided locations and calculates the optimal marginal carbon intensity windows (per the specified windowSize) for each, within the start and end time boundaries. 
If no start or end time boundaries are provided, all forecasted data points are used. 

The forecast data represents what the data source predicts future marginal carbon intesity values to be, not actual measured emissions data (as future values cannot be known).
This endpoint is useful for determining if there is a more carbon-optimal time to use electicity predicted in the future.

Parameters:
1. location: This is a required parameter and is an array of the names of the data region for the configured Cloud provider.
2. startTime: Start time boundary of forecasted data points. Ignores current forecast data points before this time. It defaults to the earliest time in the forecast data.
3. endTime: End time boundary of forecasted data points. Ignores current forecast data points after this time. Defaults to the latest time in the forecast data.
If time period is not provided, it retrieves all the data until the current time.
4. windowSize: The estimated duration (in minutes) of the workload. Defaults to the duration of a single forecast data point.

EG
```
https://<server_name>/forecasts/current?location=northeurope&startTime=2022-07-19 14:00&endTime=2022-07-20&windowSize=10
```
The response is an array of forecasts (one per requested location) with their optimal marginal carbon intensity windows.
EG
```
[
  {
    "generatedAt": "2022-07-19T13:10:00+00:00",
    "optimalDataPoint": {
      "location": "IE",
      "timestamp": "2022-07-19T13:55:00+00:00",
      "duration": 10,
      "value": 411.27979312816666
    },
    "forecastData": [
      {
        "location": "IE",
        "timestamp": "2022-07-19T13:55:00+00:00",
        "duration": 10,
        "value": 411.27979312816666
      },
      {
        "location": "IE",
        "timestamp": "2022-07-19T14:00:00+00:00",
        "duration": 10,
        "value": 425.2569247386666
      },
      ..
    ]
  }
]
```
  

## Error Handling

The WebAPI leveraged the [.Net controller filter pipeline](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-6.0) to ensure that all requests respond with a consistent JSON schema.

![.Net controller filter pipeline image](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters/_static/filter-pipeline-2.png?view=aspnetcore-6.0)

Controllers are responsible for managing the "Success" responses.  If an error occurs in the WebAPI code and an unhandled exception is thrown, the [custom Exception Filter](./Filters/HttpResponseExceptionFilter.cs) will manage converting that exception into the appropriate JSON response.  NOTE: The Exception Filter is only used for unhandled exceptions.  If the exception is caught and handled by the WebAPI code, the controller will continue to manage the response.

The .Net framework will automatically respond to validation errors with a [ValidationProblemDetails](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.validationproblemdetails?view=aspnetcore-6.0) object.  Using the Exception Filter class enables the WebAPI to consistently respond with the `ValidationProblemDetails` error schema in all error cases and take advantage of error handling automatically provided by the framework.

![WebAPI Error Handling Flow Chart](docs/images/web-api-error-handling-flow.png)

## Autogenerate WebAPI

Using the following steps, it is possible to get the CarbonAware WebApi OpenAPI specification

1. Make sure the current directory is `<path to project root>/src/`

    ```sh
    dotnet restore
    cd CarbonAware.WebApi/src
    dotnet tool restore
    dotnet build --configuration Release --no-restore
    dotnet tool run swagger tofile --output ./api/v1/swagger.yaml --yaml bin/Release/net6.0/CarbonAware.WebApi.dll v1
    ```

1. The `CarbonAware.WebApi/src/api/v1/swagger.yaml` file contains the supported OpenApi specification.
1. Use for instance [swagger editor](https://editor.swagger.io) to see and try the endpoint routes.
