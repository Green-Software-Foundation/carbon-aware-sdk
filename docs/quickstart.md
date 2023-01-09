# Setting up and using the Carbon Aware SDK

This guide will provide you with knowledge and examples necessary to use the
SDK, either as a CLI, by directly calling the Web API endpoints or by using
generated libraries for your language of choice!

## Using the CLI

### Setting up the CLI

Prerequisites:

- .NET Core 6.0
- Alternatively:
  - Docker
  - VSCode (it is recommended to work in a Dev Container)
  - Remote Containers extension for VSCode:
    <https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers>

The CLI can either be run locally with `dotnet` or in a container, e.g. using
VSCode Remote Containers (Dev Container). To run locally:

1. Make sure you have the repository cloned:
   <https://github.com/Green-Software-Foundation/carbon-aware-sdk.git>
2. `git clone https://github.com/Green-Software-Foundation/carbon-aware-sdk.git`
3. Change directory to: `cd carbon-aware-sdk/src/CarbonAware.CLI/src`
4. If you have a WattTime account registered (or other data source) - you will
   need to configure the application to use them. By default the SDK will use a
   pre-generated JSON file with random data. To configure the application, you
   will need to set up specific environment variables or modify
   `appsetttings.json` inside of `src/CarbonAware.WebApi/src` directory.
   Detailed information on configuration can be found in the `GettingStarted.md`
   file:
   <https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/GettingStarted.md>.
   Otherwise, you can follow an example configuration below (export these
   environment variables in the Terminal):

   ```bash
   export DataSources__EmissionsDataSource="WattTime"
   export DataSources__ForecastDataSource="WattTime"
   export DataSources__Configurations__WattTime__Type="WattTime"
   export DataSources__Configurations__WattTime__username="<YOUR_WATTTIME_USERNAME>"
   export DataSources__Configurations__WattTime__password="<YOUR_WATTTIME_PASSWORD>"
   ```

   or

   ```bash
   export DataSources__ForecastDataSource="ElectricityMaps"
   export DataSources__Configurations__ElectricityMaps__Type="ElectricityMaps"
   export DataSources__Configurations__ElectricityMaps__APITokenHeader="auth-token"
   export DataSources__Configurations__ElectricityMaps__APIToken="<YOUR_ELECTRICITYMAPS_TOKEN>"
   ```

5. Run the CLI using `dotnet run`

The CLI will ask you to at minimum provide a `--location (-l)` parameter.

### Calling the SDK via CLI

To run the CLI, simply call `dotnet run` and provide it with any parameters. If
you fail to pass any parameters, a help screen will be printed out with possible
parameters and short explanations. For example, to get emissions in the `eastus`
and `uksouth` region between `2022-08-23 at 11:15am` and
`2022-08-23 at 11:20am`, run:
`dotnet run -l eastus,uksouth -t 2022-08-23T11:15 --toTime 2022-08-23T11:20`
Expected output:

```JSON
[
  {
    "Location": "PJM_ROANOKE",
    "Time": "2022-08-23T11:20:00+00:00",
    "Rating": 567.44405487,
    "Duration": "00:05:00"
  },
  {
    "Location": "PJM_ROANOKE",
    "Time": "2022-08-23T11:15:00+00:00",
    "Rating": 564.72250065,
    "Duration": "00:05:00"
  },
  {
    "Location": "PJM_ROANOKE",
    "Time": "2022-08-23T11:10:00+00:00",
    "Rating": 564.72250065,
    "Duration": "00:05:00"
  },
  {
    "Location": "UK",
    "Time": "2022-08-23T11:20:00+00:00",
    "Rating": 422.74808884000004,
    "Duration": "00:05:00"
  },
  {
    "Location": "UK",
    "Time": "2022-08-23T11:15:00+00:00",
    "Rating": 422.74808884000004,
    "Duration": "00:05:00"
  },
  {
    "Location": "UK",
    "Time": "2022-08-23T11:10:00+00:00",
    "Rating": 422.74808884000004,
    "Duration": "00:05:00"
  }
]
```

To get the best time and location from a list of locations and a specified time
window, use the `--lowest` flag. E.g. to get the best time and location in a 24
hour window on the 23rd of August in the regions: `eastus`, `westus`,
`westus3`,`uksouth`, run the command:

```bash
dotnet run -l eastus,westus,westus3,uksouth -t 2022-08-23T00:00 --toTime 2022-08-23T23:59 --lowest
```

Expected output:

```JSON
[
  {
    "Location": "UK",
    "Time": "2022-08-23T08:50:00+00:00",
    "Rating": 384.64632976,
    "Duration": "00:05:00"
  }
]
```

## Using the Web API

### Setting up the Web API

Prerequisites:

- Docker Desktop/CLI
- VSCode (it is recommended to work in a Dev Container)
- Remote Containers extension for VSCode:
  <https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers>

First we need to set up the GitHub repository
(<https://github.com/Green-Software-Foundation/carbon-aware-sdk.git>):

1. `git clone https://github.com/Green-Software-Foundation/carbon-aware-sdk.git`
2. Change directory into the repository: `cd carbon-aware-sdk`
3. Open VSCode: `code .`
4. Open VSCode Command Palette: (Linux/Windows: `ctrl + shift + P`, MacOS:
   `cmd + shift + P`), and run the command:
   - `Remote-Containers: Open Folder in Container`
5. If you have a WattTime account registered (or other data source) - you will
   need to configure the application to use them. By default the SDK will use a
   pre-generated JSON file with random data. To configure the application, you
   will need to set up specific environment variables or modify
   `appsetttings.json` inside of `src/CarbonAware.WebApi/src` directory.
   Detailed information on configuration can be found in the `GettingStarted.md`
   file:
   <https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/GettingStarted.md>.
   Otherwise, you can follow an example configuration below (export these
   environment variables in the Terminal):

   ```bash
   export DataSources__EmissionsDataSource="WattTime"
   export DataSources__ForecastDataSource="WattTime"
   export DataSources__Configurations__WattTime__Type="WattTime"
   export DataSources__Configurations__WattTime__username="<YOUR_WATTTIME_USERNAME>"
   export DataSources__Configurations__WattTime__password="<YOUR_WATTTIME_PASSWORD>"
   ```

   or

   ```bash
   export DataSources__ForecastDataSource="ElectricityMaps"
   export DataSources__Configurations__ElectricityMaps__Type="ElectricityMaps"
   export DataSources__Configurations__ElectricityMaps__APITokenHeader="auth-token"
   export DataSources__Configurations__ElectricityMaps__APIToken="<YOUR_ELECTRICITYMAPS_TOKEN>"
   ```

6. In the VSCode Terminal:
7. Change directory to: `cd src/CarbonAware.WebApi/src`
8. And run the application using: `dotnet run`
9. By default, it will be hosted on `localhost:5073`

### Calling the Web API via command line

Prerequisites:

- `curl` or other tool that allows making HTTP requests (e.g. `wget`)
- Recommended: `jq` for parsing JSON output: <https://stedolan.github.io/jq/>

With the API running on `localhost:5073`, we can make HTTP requests to its
endpoints, full endpoint description can be found here:
<https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/src/CarbonAware.WebApi/src/README.md>

#### Calling the `/emissions/bylocation` endpoint

In console, we can run the below command, to request data for a single location
(currently Azure region names supported) in a particular timeframe:

```bash
curl "http://localhost:5073/emissions/bylocation?location=westus&time=2022-08-23T14%3A00&toTime=2022-08-23T14%3A30" | jq
```

You can omit the `| jq` to get the JSON data raw and unparsed. This is a request
for data in the `westus` region from the date `2022-08-23 at 14:00` to
`2022-08-23 at 14:30`. (Note: semicolons `:` are encoded as `%3A` in URLs).

The sample data output should be:

```JSON
[
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T14:30:00+00:00",
    "rating": 439.07741416000005,
    "duration": "00:05:00"
  },
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T14:25:00+00:00",
    "rating": 438.62382179,
    "duration": "00:05:00"
  },
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T14:20:00+00:00",
    "rating": 438.62382179,
    "duration": "00:05:00"
  },
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T14:15:00+00:00",
    "rating": 439.53100653,
    "duration": "00:05:00"
  },
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T14:10:00+00:00",
    "rating": 439.98459890000004,
    "duration": "00:05:00"
  },
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T14:05:00+00:00",
    "rating": 456.31392422000005,
    "duration": "00:05:00"
  },
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T14:00:00+00:00",
    "rating": 439.98459890000004,
    "duration": "00:05:00"
  },
  {
    "location": "CAISO_NORTH",
    "time": "2022-08-23T13:55:00+00:00",
    "rating": 445.42770734000004,
    "duration": "00:05:00"
  }
]
```

#### Calling the `/emissions/bylocations/best` endpoint

This endpoint, unlike the previous one, accepts a list of locations and outputs
a single time and location with the LOWEST Carbon Intensity index.

In console, we can run the below command:

```bash
curl "http://localhost:5073/emissions/bylocations/best?location=westus&location=eastus&location=westus3&time=2022-08-23T00%3A00&toTime=2022-08-23T23%3A59" | jq
```

You can omit the `| jq` to get the JSON data raw and unparsed. This is a request
for the best location and time out of the locations: `westus`, `eastus`,
`westus3` in the time window from `2022-08-23 at 00:00` to `2022-08-23 at 23:59`

The sample data output should be:

```JSON
{
  "location": "AZPS",
  "time": "2022-08-23T08:05:00+00:00",
  "rating": 398.70769323,
  "duration": "00:05:00"
}
```

### Calling the Web API via client libraries

The SDK can work with libraries for up to 50 languages generated with the
[Open API Generator (Swagger)](https://openapi-generator.tech/). This guide will
provide a tutorial to generating clients for java, Python, JavaScript, .NET and
GoLang. There is also a walkthrough of an example Python script interacting with
the SDK.

#### Client generation

Prerequisites:

- Docker
- Web API running (locally or hosted online)
- (Optionally) `openapi-generator-cli`

The clients can be generated either by hand with the openapi-generator CLI, or
by running shell scripts which also call these generators. The easiest way to
generate them after using the Web API, is to do it Terminal **while** the Web
API is running.

1. In Terminal: Change into directory `carbon-aware-sdk/src/clients`
2. Run the client generation script, passing the API URL (omitting the initial
   `http://`): `./docker-generate-clients.sh host.docker.internal:5073`
   - If your API is available at a different URL/Port, replace
     `host.docker.internal:5073` with that url.
3. You should now see multiple generated clients in that directory (check with
   `ls`)

There is an alternative script for generating the tests - `generate-clients.sh`
which can be ran if you have the `openapi-generator-cli` installed locally.

#### Python Client installation + example usage

After generating the clients, we can now install them. Most generated clients
(with OpenAPI) should have a `README` file containing instructions on
installation and example usage.

1. Change directory to `cd carbon-aware-sdk/src/clients/python`. This is the
   generated Python client
2. Install the requirements using `pip install -r requirements.txt`
3. Install the Python client library using
   [`setuptools`](http://pypi.python.org/pypi/setuptools)):
   `python setup.py install --user`
4. The library is now succesfully installed!

There should be an example script in the `README` file, but this guide suggests
trying the following example first:

```Python
import time
import openapi_client
from pprint import pprint
from openapi_client.api import carbon_aware_api
from openapi_client.model.emissions_data import EmissionsData
from  dateutil.parser import parse
# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
        host = "http://localhost:5073"
)



# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = carbon_aware_api.CarbonAwareApi(api_client)
    location = "westus" # str |  (optional)
    time = parse('2022-07-22T10:30:00.00Z') # datetime |  (optional)
    to_time = parse('2022-07-22T11:00:00.00Z') # datetime |  (optional)
    duration_minutes = 0 # int |  (optional) (default to 0)

    try:
        api_response = api_instance.get_emissions_data_for_location_by_time(location=location, time=time, to_time=to_time, duration_minutes=duration_minutes)
        pprint(api_response)
    except openapi_client.ApiException as e:
        print("Exception when calling CarbonAwareApi->emissions_bylocation_get: %s\n" % e)

```

Here, we import the `openapi_client` along with other modules generated by the
API. We create a default configuration pointing to Web API at `localhost:5073`,
change it to a different URL if your API is deployed at a different URL/port.
This line of code:

```Python
        api_response = api_instance.get_emissions_data_for_location_by_time(location=location, time=time, to_time=to_time, duration_minutes=duration_minutes)
```

Calls the Python Client to send a request to the Carbon Aware SDK Web API, for
the `/emissions/bylocation` endpoint, similarly to what's shown above, when
polling the API directly with HTTP requests. This is an example request for the
`westus` region, in the time window from `2022-07-22 at 10:30am` to
`2022-07-22 at 11:00am`.
