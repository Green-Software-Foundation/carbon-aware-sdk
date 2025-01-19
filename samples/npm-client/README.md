# NPM Client Example

This folder contains an example for WebAPI client in NPM. Client library would be pulled from [GitHub Packages](https://github.com/orgs/Green-Software-Foundation/packages?repo_name=carbon-aware-sdk).

TypeDoc is [here](https://carbon-aware-sdk.greensoftware.foundation/client-apidocs/1.0.0/npm).

## Requirements

- WebAPI instance
  - See the [Overview](../../docs/overview.md#publish-webapi-with-container) if you'd like to start it on container.
- Node.js 18 or later

## Client code

[index.js](index.js) is an example program to call WebAPI endpoint. It calls all of endpoints, and shows the result.

Following methods are available:

- `processBestEmissionsDataByLocations`
  - Call /emissions/bylocations/best
  - Gather the best emission data for west/central/east US yesterday.
- `processEmissionsDataByLocations`
  - Call /emissions/bylocations
  - Gather emission data for west/central/east US yesterday.
- `processEmissionsDataByLocation`
  - Call /emissions/bylocation
  - Gather emission data for westus yesterday.
- `processCurrentForecastData`
  - Call /emissions/forecasts/current
  - Gather forecast data for westus.
- `processForecastBatchData`
  - Call /emissions/forecasts/batch
  - Gather forecast data for westus.
- `processAverageData`
  - Call /emissions/average-carbon-intensity .
  - Gather average data for westus yesterday.
- `processAverageBatchData`
  - Call /emissions/average-carbon-intensity/batch
  - Gather average data for westus yesterday.

## How it works

### 1. Set WebAPI endpoint

You need to set base URL of WebAPI endpoint in `index.ts`. `http://localhost:8080` is set by default.

```typescript
const conf = new Configuration({basePath: 'http://localhost:8080'});
```

### 2. Install required packages

You have to run `npm install` at first. CASDK client module would be downloaded from GitHub Packages, so you need to set GitHub Parsonal Access Token. You can set it to `GH_TOKEN` environment variable used in [.npmrc](.npmrc).

If you have not yet got GitHub PAT, see [this guide](https://docs.github.com/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens).


```sh
GH_TOKEN=<GitHub PAT> npm install
```

### 3. Run

```sh
npx ts-node index.ts
```
