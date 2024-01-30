# 0012. Treat Electricity Maps and Electricity Maps Free as different, unrelated data sources

## Status
Approved

## Context
Electricity Maps offers two different services:
- the paid one, which has already been added to the dev branch of the Carbon Aware SDK,
- and the free one, which they also call "CO2 Signal" ([https://www.co2signal.com/](https://www.co2signal.com/)), which the Carbon Aware SDK already supports in a [branch](https://github.com/Green-Software-Foundation/carbon-aware-sdk/tree/feat/electricity-map), though it is based on an older, now outdated version of the SDK.

These two services, despite being provided by the same company, use different APIs. The free API isn't just a subset of the paid one: **the endpoints are different, the tokens are different, and the responses are different**. Here's an example of two equivalent calls to these services, getting the latest value for the Carbon Intensity in France:

- ElectricityMaps free (CO2 Signal):
  - Documentation: [https://docs.co2signal.com/](https://docs.co2signal.com/)
  - Request:

      `curl -s 'https://api.co2signal.com/v1/latest?countryCode=FR' -H 'auth-token: myapitoken'`

  - Response:
```json
        {
          "_disclaimer": "This data is the exclusive property of Electricity Maps and/or related parties. If you're in doubt about your rights to use this data, please contact api@co2signal.com",
          "status": "ok",
          "countryCode": "FR",
          "data": {
            "datetime": "2023-01-23T17:00:00.000Z",
            "carbonIntensity": 103,
            "fossilFuelPercentage": 13.639999999999999
          },
          "units": {
            "carbonIntensity": "gCO2eq/kWh"
          }
        }
```

- ElectricityMaps paid:
  - Documentation: [https://static.electricitymaps.com/api/docs/index.html](https://static.electricitymaps.com/api/docs/index.html)
  - Request:

      `curl -s 'https://api.electricitymap.org/v3/carbon-intensity/latest?zone=FR' -H 'auth-token: myapitoken'`

  - Response:
	```json
        {
          "zone": "FR",
          "carbonIntensity": 103,
          "datetime": "2023-01-23T17:00:00.000Z",
          "updatedAt": "2023-01-23T16:53:20.794Z",
          "emissionFactorType": "lifecycle",
          "isEstimated": true,
          "estimationMethod": "TIME_SLICER_AVERAGE"
        }
  ```

The goal is to support both services, to maximize the usage of the Carbon Aware SDK. The question is how to handle these differences.
Treating them as the same data source would require to add some complexity, to distinguish whether an account is free or paid. This distinction would have to be either in the form of an extra parameter, or it would require the Carbon Aware SDK to test every time (at least once per session) which service is meant, using a fallback logic: try the paid service first, and if you get an error try the free one. This adds complexity, requires managing the error code, and in any case forces to make more calls, which has a negative impact on the emissions.

## Decision
Since the endpoints, the tokens and the output format are all different, it is easier to treat them as different data sources, unrelated to each other, called "Electricty Maps" and "Electricty Maps Free". This approach requires the user to explicitly indicate what service they want to use, but then every other problem is solved automatically.


## Consequences
The new data source will have to be added following the [instructions](https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/Changelog/docs/architecture/data-sources.md#user-content-creating-a-new-data-source).

## Green Impact
Neutral. This is an implementation detail, under the hood. It has no impact for the users of the Carbon Aware SDK.
