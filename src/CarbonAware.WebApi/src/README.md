The Carbon Aware SDK provides an API to get the marginal carbon intensity for a given location and time period. The values reported in the Green Software Foundation's specification for marginal carbon intensity (Grams per Kilowatt Hour). In order to use the API, the environment needs to be prepared with a set of configuration parameters. Instructions on setting up the environment could be found here - [GettingStarted.md](../../../GettingStarted.md)

# Carbon Aware REST API

- [Carbon Aware REST API](#carbon-aware-rest-api)
  - [Endpoints](#endpoints)
    - [GET emissions/bylocation](#get-emissionsbylocation)
    - [GET emissions/bylocations](#get-emissionsbylocations)
    - [GET emissions/bylocations/best](#get-emissionsbylocationsbest)
    - [GET emissions/forecasts/current](#get-emissionsforecastscurrent)
    - [POST emissions/forecasts/batch](#post-emissionsforecastsbatch)
    - [GET emissions/average-carbon-intensity](#get-emissionsaverage-carbon-intensity)
    - [POST emissions/average-carbon-intensity/batch](#post-emissionsaverage-carbon-intensitybatch)
  - [Error Handling](#error-handling)
  - [Autogenerate WebAPI](#autogenerate-webapi)
  
## Endpoints

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

### GET emissions/forecasts/current

This endpoint fetches only the most recently generated forecast for all provided locations.  It uses the "dataStartAt" and 
"dataEndAt" parameters to scope the forecasted data points (if available for those times). If no start or end time 
boundaries are provided, the entire forecast dataset is used. The scoped data points are used to calculate average marginal 
carbon intensities of the specified "windowSize" and the optimal marginal carbon intensity window is identified.

The forecast data represents what the data source predicts future marginal carbon intesity values to be, 
not actual measured emissions data (as future values cannot be known).

This endpoint is useful for determining if there is a more carbon-optimal time to use electicity predicted in the future.

Parameters:
1. `location`: This is a required parameter and is an array of the names of the data region for the configured Cloud provider.
2. `dataStartAt`: Start time boundary of the current forecast data points. Ignores current forecast data points before this time. Must be within the forecast data point timestamps. Defaults to the earliest time in the forecast data.
3. `dataEndAt`: End time boundary of the current forecast data points. Ignores current forecast data points after this time. Must be within the forecast data point timestamps. Defaults to the latest time in the forecast data.
If neither `dataStartAt` nor `dataEndAt` are provided, all forecasted data points are used in calculating the optimal marginal carbon intensity window.
4. `windowSize`: The estimated duration (in minutes) of the workload. Defaults to the duration of a single forecast data point.

EG
```
https://<server_name>/emissions/forecasts/current?location=northeurope&dataStartAt=2022-07-19T14:00:00Z&dataEndAt=2022-07-20T04:38:00Z&windowSize=10
```
The response is an array of forecasts (one per requested location) with their optimal marginal carbon intensity windows.

EG
```
[
  {
    "requestedAt": "2022-07-19T13:37:49+00:00",
    "generatedAt": "2022-07-19T13:35:00+00:00",
    "location": "northeurope",
    "dataStartAt": "2022-07-19T14:00:00Z",
    "dataEndAt": "2022-07-20T04:38:00Z",
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
        "timestamp": "2022-07-19T14:00:00+00:00",
        "duration": 10,
        "value": 532.02293146
      },
      ...
      {
        "location": "IE",
        "timestamp": "2022-07-20T04:25:00+00:00",
        "duration": 10,
        "value": 535.7318741001667
      }
    ]
  }
]
```

### POST emissions/forecasts/batch
This endpoint takes a batch of requests for historical forecast data, fetches them, and calculates the optimal marginal carbon intensity windows for each using the same parameters available to the '/emissions/forecasts/current' endpoint.

This endpoint is useful for back-testing what one might have done in the past, if they had access to the current forecast at the time.

Parameters:
1. requestedForecasts: Array of requested forecasts. Each requested forecast contains
    * `requestedAt`: This is a required parameter and is the historical time used to fetch the most recent forecast as of that time.
    * `location`: This is a required parameter and is the name of the data region for the configured Cloud provider.
    * `dataStartAt`: Start time boundary of the forecast data points. Ignores forecast data points before this time. Must be within the forecast data point timestamps. Defaults to the earliest time in the forecast data.
    * `dataEndAt`: End time boundary of the forecast data points. Ignores forecast data points after this time. Must be within the forecast data point timestamps. Defaults to the latest time in the forecast data. 
    * `windowSize`: The estimated duration (in minutes) of the workload. Defaults to the duration of a single forecast data point

If neither `dataStartAt` nor `dataEndAt` are provided, all forecasted data points are used in calculating the optimal marginal carbon intensity window.

EG
```
[
  {
    "location": "eastus",
    "dataStartAt": "2022-06-01T14:00:00Z",
    "dataEndAt": "2022-06-01T18:00:00Z",
    "windowSize": 30,
    "requestedAt": "2022-06-01T12:01:00Z"
  },
  {
    "location": "westus",
    "dataStartAt": "2022-06-13T08:00:00Z",
    "dataEndAt": "2022-06-13T10:00:00Z",
    "windowSize": 30,
    "requestedAt": "2022-06-13T6:05:00Z"
  }
]

```
The response is an array of forecasts (one per requested location) with their optimal marginal carbon intensity windows.
EG
```
[
  {
    "generatedAt": "2022-06-01T12:00:00+00:00",
    "optimalDataPoint": {
      "location": "IE",
      "timestamp": "2022-06-01T16:45:00+00:00",
      "duration": 10,
      "value": 448.4451043375
    },
    "forecastData": [ ... ] // all relevant forecast data points
    "requestedAt": "2022-06-01T12:01:00
    "location": "eastus",
    "dataStartAt": "2022-06-01T14:00:00Z",
    "dataEndAt": "2022-06-01T18:00:00Z",
    "windowSize": 30,
  },
    {
    "generatedAt": "2022-06-13T06:05:00+00:00",
    "optimalDataPoint": {
      "location": "IE",
      "timestamp": "2022-06-13T09:25:00+00:00",
      "duration": 10,
      "value": 328.178478
    },
    "forecastData": [ ... ] // all relevant forecast data points
    "requestedAt": "2022-06-13T06:05:00
    "location": "westus",
    "dataStartAt": "2022-06-13T08:00:00Z",
    "dataEndAt": "2022-06-13T10:00:00Z",
    "windowSize": 30,
  }
]
```

### GET emissions/average-carbon-intensity

This endpoint retrieves the measured carbon intensity data between the time boundaries and calculates the average carbon intensity during that period. Location is a required parameter and is the name of the data region for the configured Cloud provider.

This endpoint is useful for reporting the measured carbon intensity for a specific time period in a specific location.

Parameters:
1. `location`: This is a required parameter and is the string name of the data region for the configured Cloud provider.
2. `startTime`: The time at which the workload and corresponding carbon usage begins.
3. `endTime`: The time at which the workload and corresponding carbon usage ends.

EG
```
https://<server_name>/emissions/average-carbon-intensity?location=eastus&startTime=2022-07-19T14:00:00Z&endTime=2022-07-19T18:00:00Z
```
The response is a single object that contains the information about the request and the average marginal carbon intensity

EG
```
{
  "location": "eastus",
  "startTime": "2022-07-19T14:00:00Z",
  "endTime": "2022-07-19T18:00:00Z",
  "carbonIntensity": 345.434
}
```

### POST emissions/average-carbon-intensity/batch
This endpoint takes an array of request objects, each with their own location and time boundaries, and calculates the average carbon intensity for that location and time period.

This endpoint only supports batching across a single location with different time boundaries. If multiple locations are provided, an error is returned. For each item in the request array, the application returns a corresponding object containing the location, time boundaries, and average marginal carbon intensity. 

Parameters:
1. requestedCarbonIntensities: Array of requested carbon intensities. Each requested carbon intensity contains
    * `location`: This is a required parameter and is the name of the data region for the configured Cloud provider.
    * `startTime`: The time at which the workflow we are requesting carbon intensity for started.
    * `endTime`: The time at which the workflow we are requesting carbon intensity for ended.

EG
```
[
  {
    "location": "eastus",
    "startTime": "2022-05-01T14:00:00",
    "endTime": "2022-05-01T18:00:00"
  },
  {
    "location": "eastus",
    "startTime": "2022-06-01T14:00:00",
    "endTime": "2022-06-01T18:00:00"
  },
  {
    "location": "eastus",
    "startTime": "2022-07-01T14:00:00",
    "endTime": "2022-07-01T18:00:00"
  }
]

```
The response is an array of CarbonIntensityDTO objects which each have a location, start time, end time, and the average marginal carbon intensity over that time period.

EG
```
[
  {
    "carbonIntensity": 32.935208333333335,
    "location": "eastus",
    "startTime": "2022-05-01T14:00:00-04:00",
    "endTime": "2022-05-01T18:00:00-04:00"
  },
  {
    "carbonIntensity": 89.18215277777779,
    "location": "eastus",
    "startTime": "2022-06-01T14:00:00-04:00",
    "endTime": "2022-06-01T18:00:00-04:00"
  },
  {
    "carbonIntensity": 10.416875,
    "location": "eastus",
    "startTime": "2022-07-01T14:00:00-04:00",
    "endTime": "2022-07-01T18:00:00-04:00"
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
