The Carbon Aware SDK provides an API to get the marginal carbon intensity for a given location and time period. The values reported in the Green Software Foundation's specification for marginal carbon intensity (Grams per Kilowatt Hour). In order to use the API, the environment needs to be prepared with a set of configuration parameters. Instructions on setting up the environment could be found here - https://github.com/microsoft/carbon-aware-sdk/blob/dev/GettingStarted.md

# Carbon Aware REST API


- [Carbon Aware REST API](#carbon-aware-rest-api)
  - [Endpoints](#endpoints)
    - [GET emissions/bylocation](#get-emissionsbylocation)
    - [GET emissions/bylocations](#get-emissionsbylocations)
    - [GET emissions/bylocations/best](#get-emissionsbylocationsbest)
  - [Error Handling](#error-handling)
  
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


## Error Handling

The WebAPI leveraged the [.Net controller filter pipeline](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-6.0) to ensure that all requests respond with a consistent JSON schema.

![.Net controller filter pipeline image](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters/_static/filter-pipeline-2.png?view=aspnetcore-6.0)

Controllers are responsible for managing the "Success" responses.  If an error occurs in the WebAPI code and an unhandled exception is thrown, the [custom Exception Filter](./Filters/HttpResponseExceptionFilter.cs) will manage converting that exception into the appropriate JSON response.  NOTE: The Exception Filter is only used for unhandled exceptions.  If the exception is caught and handled by the WebAPI code, the controller will continue to manage the response.

The .Net framework will automatically respond to validation errors with a [ValidationProblemDetails](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.validationproblemdetails?view=aspnetcore-6.0) object.  Using the Exception Filter class enables the WebAPI to consistently respond with the `ValidationProblemDetails` error schema in all error cases and take advantage of error handling automatically provided by the framework.

![WebAPI Error Handling Flow Chart](docs/images/web-api-error-handling-flow.png)
