# Architecture Overview

The Carbon Aware SDK is built with three tiers where each tier has its own set of responsibilities.
1. Consumer Tier
2. Business Logic Tier
3. Data Tier

Each tier only has knowledge of its own responsibilities, and specific channels of communication between adjacent tiers. This allows the SDK to switch to different implementations withing the same tier without any necessary code changes.

![Tiers architecture diagram](../images/tiers-architecture.drawio.png)

## Consumer Tier
The consumer tier is the entry point for interacting with the SDK. It handles taking the user input and deciding which handler(s) should handle processing the input. There are currently 2 different consumers that can be used in the SDK
1. A RESTful WebAPI that can be called using HTTP requests.
2. A command line tool that runs directly on a host machine.

Both consumers access the same components in the tiers below.

## Business Logic Tier
The business logic tier functions as the processor, taking in the user input and figuring out how to fulfill it. It is comprised of a set of handlers that know what type of data they need and how to calculate the result. 

### Handlers
Handlers have knowledge of the underlying data source interfaces in the data tier. A Handler takes in consumer requests, calls the specified data source, and performs any data aggregation required before returning the result to the consumer. Each Handler is responsible for handling requests specific to a functionality.

Currently, the SDK provides 2 handlers - `EmissionsHandler` and `ForecastHandler` to handle requests for actual carbon emissions and forecasted carbon emissions respectively. The `EmissionsHandler` handles requests for various carbon emissions information. It can calculate the average carbon emissions over a time period, or the best carbon emissions given a set of locations. It can transform forecasted carbon emissions to suit particular use-cases. It can also just deliver the emissions data points in a standard schema without performing any calculations.
The `ForecastHandler` 

See the [c-sharp-client-library README](./c-sharp-client-library.md) for more detailed information.

## Data Tier
The data tier is responsible for ingesting data into the SDK. It is comprised of a set of data source interfaces representing the broad categories of data used by the SDK. 

### Data Source Interface
A data source interface defines all the necessary parameters and functions needed to access that category of data. These definitions must be independent of specific data sources and use abstracted input/output parameters expected from the business logic tier. This abstraction enables multiple data sources to extend a multiple interfaces and be switched out and configured based on the needs of the operator.

### Data Source Implementation
A data source inherits from a single or multiple data source interfaces and does the work of accessing a specific data provider. Each data source the information specific to the data provider it is accessing, including any authentication needs, the format of the request and response, etc. It is also responsible for converting the request from the business logic tier into the required provider request, and similarly, converting the provider response back into the expected result.

See the [data source README](./data-sources.md) for more detailed information.

## Dependency Registration
The SDK uses dependency injection to load the data sources based on set environment variables. To register a new dependency, a new ServiceCollectionExtension method must be defined. These dependencies are loaded in a hierarchical structure such that:
1. Each data source defines a `ServiceCollectionExtension` method.
2. All available data sources are registered in the `DataSource.Registration` project.
3. The GSF library defines a `ServiceCollectionExtension` method where it registers the data sources for the handlers to use.
4. The `Program.cs` file registers the GSF library classes at startup

## Example Call Flow
The swimlanes diagram below follows an example call through all 3 tiers of the architecture. In this diagram, the _Client_ is a user of the SDK and the _Data Provider_ is a source of raw data, such as [WattTime](https://www.wattime.org).

![Tiers flow diagram](../images/overview-tiers-swimlanes.png)