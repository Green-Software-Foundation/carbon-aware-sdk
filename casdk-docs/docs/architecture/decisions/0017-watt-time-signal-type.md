# 0017. Signal Type in WattTime v3 Data Source

## Status

Proposed

## Context

WattTime v3 API has been supported since [Carbon Aware SDK v1.5.0](https://carbon-aware-sdk.greensoftware.foundation/blog/release-v1.5). As we mentioned in [ADR-0015](https://carbon-aware-sdk.greensoftware.foundation/docs/architecture/decisions/watt-time-v3), `signal_type` has been added in each endpoints which the SDK will access since v3 API. We should be able to set following parameters to it, but it can't in Carbo Aware SDK.

https://watttime.org/data-science/data-signals/

| Signal Type | Description |
|---|---|
| `co2_moer` |  Marginal Operating Emissions Rate of carbon dioxide. |
| `co2_aoer` |  Average Operating Emissions Rate of carbon dioxide. |

According to [Green Software Practitioners](https://learn.greensoftware.foundation/carbon-awareness#marginal-carbon-intensity), "marginal" means the carbon intensity of the power plant that would have to be employed to meet any new demand. On the other hand, "average" means the average of all of power plants. It should be chosen by Carbon Aware SDK user because which value is needed depends on the user.

`co2_moer` is hard-coded until Carbon Aware SDK v1.7.0 (at least).

## Decision

The proposal is for adding a new parameter for Signal Type in WattTime Data Source.

## Update Changes

We will introduce new parameter for data source configuration of WattTime as following.

### appsettings.json

```json
"WattTime": {
  "Type": "WattTime",
  "Username": "username",
  "Password": "password",
  "BaseURL": "https://api.watttime.org/v3/",
  "SignalType": "co2_aoer"
}
```

### environment variable

```bash
DataSources__Configurations__WattTime__SignalType=co2_aoer
```

## Green Impact

Neutral
