# Carbon Aware SDK Dotnet

# Using the Carbon Aware SDK
There are 4 ways to use the Carbon Aware Dotnet SDK being:
* WebApi - Deploy as a REST end point
* CLI - Invoke via command line (or use as a Docker image)
* Github Action - Invoke the SDK CLI as a step in your CICD pipeline
* Native Library - Write code against the .NET library


## WebApi
***Highly Recommended*** - Best for when you can change the code, and deploy separately.  This also allows you to manage the Carbon Aware logic independently of the system using it.

The WebApi replicates the CLI and SDK functionality, leveraging the same configuration and providing a REST end point with Swagger/OpenAPI definition for client generation.

## CLI
Best for use with systems you can not change the code in but can invoke command line.  For example - build pipelines.

The CLI exposes the primary `getEmissionsByLocationsAndTime` SDK methods via command line and outputs the results as json to stdout.  

**You can use the CLI as a docker image 

## Github action
Based on the CLI as docker image, the Github action allows to use the Carbon Aware metrics as part of your deployment pipeline in Github.

check out the sample Github Action pipeline: https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/sdkCLI-githubaction/.github/workflows/run-sdkCLI-githubaction.yml

## Native Library
Best for when you are using .NET, and you have the ability to change the code, and do not have the ability to deploy the WebApi.  

> NOTE: If you can deploy the REST service, it is still recommended to to this regardless of if you are using .NET due to the cleaner decoupling, deployment lifecycle, and abstraction that it provides.

# Other Components

## Basic Plugin
There is a basic plugin provided that reads static data and queries over them using _basic_ logic.  

## Data Files
Data files are stored in the `data` folder.  The `test-data-cloud-remissions.json` files being the most important for testing.

All files placed here will be copied into teh CLI and WebApi at build time.
## Test Data Generators
There are 2 data generators that help to generate the data files for testing purposes. These created data for all regions in the respective cloud providers, and can be used for demos or recreating more test data.
