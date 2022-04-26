# Carbon Aware REST API

## Endpoints

### POST /sci-scores

This endpoint uses a custom plugin which calculates the SCI score using the Green Software Foundation SCI specification formula.

> ((E \* I) + M)/R

- (E) Energy
- (I) Marginal Carbon Intensity
- (M) Embodied Emissions
- (R) Functional Unit

The payload object must include location and timeInterval. If location type is CloudProvider, location should include provider and region attributes, if location type is geoposition then the location should include latitude and longitude.
```
{
    "locationType": "CloudProvider",
    "location": {
        "provider": "Azure",
        "region": "uswest"
        "latitude": null,
        "longitude": null
    }
}
```

The request object is defined by the calculator, but the response object MUST include the SCI score and the component variables. EG

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

The payload object must include location and timeInterval, it is the same as the payload for the sci-score endpoint. If location type is CloudProvider, location should include provider and region attributes, if location type is geoposition then the location should include latitude and longitude.
```
{
    "locationType": "CloudProvider",
    "location": {
        "provider": "Azure",
        "region": "uswest"
        "latitude": null,
        "longitude": null
    }
}
```


This endpoint calculates just the Average Marginal Carbon Intensity which is the I portion of the Green Software Foundation specification

The response object must include the the value of the marginal carbon intensity.
EG

```
{
    "marginalCarbonIntensityValue": 750.0
}
```