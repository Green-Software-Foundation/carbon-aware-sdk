# Carbon Aware REST API


- [Carbon Aware REST API](#carbon-aware-rest-api)
  - [Endpoints](#endpoints)
    - [POST /sci-scores](#post-sci-scores)
    - [POST /sci-scores/marginal-carbon-intensity](#post-sci-scoresmarginal-carbon-intensity)
  
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