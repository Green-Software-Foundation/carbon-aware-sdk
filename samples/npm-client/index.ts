import { CarbonAwareApi, CarbonIntensityBatchParametersDTO, Configuration, EmissionsForecastBatchParametersDTO } from '@Green-Software-Foundation/casdk-client';

const conf = new Configuration({basePath: 'http://localhost:8080'});
const caApi = new CarbonAwareApi(conf);


/**
 * Call /emissions/bylocations/best .
 * Returns the best emission data for west/central/east US yesterday as Promise.
 */
const processBestEmissionsDataByLocations = async() => {
  const time = new Date();
  time.setUTCDate(time.getUTCDate() - 1);
  time.setUTCHours(0, 0, 0, 0);
  const toTime = new Date(time.getTime());
  toTime.setUTCHours(23, 59, 59, 999);

  return caApi.getBestEmissionsDataForLocationsByTime(['westus', 'centralus', 'eastus'], time.toISOString(), toTime.toISOString())
              .then((response) => response.data);
};

/**
 * Call /emissions/bylocations .
 * Returns emission data for west/central/east US yesterday as Promise.
 */
const processEmissionsDataByLocations = async() => {
  const time = new Date();
  time.setUTCDate(time.getUTCDate() - 1);
  time.setUTCHours(0, 0, 0, 0);
  const toTime = new Date(time.getTime());
  toTime.setUTCHours(23, 59, 59, 999);

  return caApi.getEmissionsDataForLocationsByTime(['westus', 'centralus', 'eastus'], time.toISOString(), toTime.toISOString())
              .then((response) => response.data);
};

/**
 * Call /emissions/bylocation .
 * Returns emission data for westus yesterday as Promise.
 */
const processEmissionsDataByLocation = async() => {
  const time = new Date();
  time.setUTCDate(time.getUTCDate() - 1);
  time.setUTCHours(0, 0, 0, 0);
  const toTime = new Date(time.getTime());
  toTime.setUTCHours(23, 59, 59, 999);

  return caApi.getEmissionsDataForLocationByTime('westus', time.toISOString(), toTime.toISOString())
              .then((response) => response.data);
};

/**
 * Call /emissions/forecasts/current .
 * Returns forecast data for westus as Promise.
 */
const processCurrentForecastData = async() => {
  const startTime = new Date();
  startTime.setUTCHours(startTime.getUTCHours() + 1);
  startTime.setUTCSeconds(0);
  startTime.setUTCMilliseconds(0);
  const endTime = new Date(startTime.getTime());
  endTime.setUTCHours(endTime.getUTCHours() + 1);

  return caApi.getCurrentForecastData(['westus'], startTime.toISOString(), endTime.toISOString())
              .then((response) => response.data);
};

/**
 * Call /emissions/forecasts/batch .
 * Returns forecast data for westus as Promise.
 */
const processForecastBatchData = async() => {
  const requestTime = new Date();
  requestTime.setUTCMilliseconds(0);
  const startTime = new Date(requestTime.getTime());
  startTime.setUTCMinutes(requestTime.getUTCMinutes() + 10);
  const endTime = new Date(startTime.getTime());
  endTime.setUTCMinutes(startTime.getUTCMinutes() + 10);
  const dto: EmissionsForecastBatchParametersDTO = {
    location: 'westus',
    requestedAt: requestTime.toISOString(),
    dataStartAt: startTime.toISOString(),
    dataEndAt: endTime.toISOString(),
    windowSize: 10,
  };

  return caApi.batchForecastDataAsync([dto])
              .then((response) => response.data);
};

/**
 * Call /emissions/average-carbon-intensity .
 * Returns average data for westus yesterday as Promise.
 */
const processAverageData = async() => {
  const startTime = new Date();
  startTime.setUTCDate(startTime.getUTCDate() - 1);
  startTime.setUTCHours(0, 0, 0, 0);
  const endTime = new Date(startTime.getTime());
  endTime.setUTCHours(23, 59, 59, 999);

  return caApi.getAverageCarbonIntensity('westus', startTime.toISOString(), endTime.toISOString())
              .then((response) => response.data);
};

/**
 * Call /emissions/average-carbon-intensity/batch .
 * Returns average data for westus yesterday as Promise.
 */
const processAverageBatchData = async() => {
  const startTime = new Date();
  startTime.setUTCDate(startTime.getUTCDate() - 1);
  startTime.setUTCHours(0, 0, 0, 0);
  const endTime = new Date(startTime.getTime());
  endTime.setUTCHours(23, 59, 59, 999);
  const dto: CarbonIntensityBatchParametersDTO = {
    location: 'westus',
    startTime: startTime.toISOString(),
    endTime: endTime.toISOString(),
  };

  return caApi.getAverageCarbonIntensityBatch([dto])
              .then((response) => response.data);
};


async function runAll(){
  console.log('--- /emissions/bylocations/best ---');
  console.log(await processBestEmissionsDataByLocations());
  console.log();

  console.log('--- /emissions/bylocations ---');
  console.log(await processEmissionsDataByLocations());
  console.log();

  console.log('--- /emissions/bylocation ---');
  console.log(await processEmissionsDataByLocation());
  console.log();

  console.log('--- /emissions/forecasts/current ---');
  console.log(await processCurrentForecastData());
  console.log();

  console.log('--- /emissions/forecasts/batch ---');
  console.log(await processForecastBatchData());
  console.log();

  console.log('--- /emissions/average-carbon-intensity ---');
  console.log(await processAverageData());
  console.log();

  console.log('--- /emissions/average-carbon-intensity/batch ---');
  console.log(await processAverageBatchData());
  console.log();
}

runAll();
