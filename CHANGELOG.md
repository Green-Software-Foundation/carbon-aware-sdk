# Changelog

All notable changes to the Carbon Aware SDK will be documented in this file.

## [Unreleased]

### Added 

- Added Electricity Maps (paid api) support for forecasting and historical data.  
- Added ElectricityMaps (free api) support for historical data.  Note that this API does not support forecast capabilities. 

### Fixed

- Fixed generated test data that had time bomb bug that was in test data, which caused integration tests to fail.  This is now automatically generated each time.

### Changed

#### API

- `/locations` - Show the list of configured named locations that can be used in the API.

#### API Deployment

- Configuration has changed.  Refer to upgrading from 1.0.0 to 1.1.0 below.

#### SDK 

- Funcationality for forecast and historical data have been seperated into seperate interfaces.  This impacts configuration, see upgrading from 1.0.0 to 1.1.0 for more information
- Additional tests

### Upgrading from 1.0.0 to 1.1.0 

- Configuration changes are required due to historical and forecast configuration now being decoupled.  Refer to ___ for a guide.

## [1.0.0] - 2022-10-01

### Added

- Added CLI for carbon data 
- Added WebApi for carbon data 
- Added support for WattTime in forecasts and historical data sets
- Added following API's 
  - `/emissions/bylocation` - Calculate the best emission data by location for a specified time period 
  - `/emissions/bylocations` - Calculate the observed emission data by list of locations for a specified time period
  - `/emissions/bylocations/best` - Calculate the best emission data by list of locations for a specified time period
  - `/emissions/average-carbon-intensity` - Retrieves the measured carbon intensity data between the time boundaries and calculates the average carbon intensity during that period
  - `/emissions/forecasts/current` - Retrieves the most recent forecasted data and calculates the optimal marginal carbon intensity window
  - `/emissions/forecasts/batch` - Given an array of historical forecasts, retrieves the data that contains forecasts metadata, the optimal forecast and a range of forecasts filtered by the attributes [start...end] if provided
  - `/emissions/average-carbon-intensity/batch` - Given an array of request objects, each with their own location and time boundaries, calculate the average carbon intensity for that location and time period and return an array of carbon intensity objects.


### Changed

- First major release to main, no previous version

### Security

- No known security vulnerabilities or concerns 
