# Carbon Aware CLI

The CLI is best for use with systems you can not change the code in but can invoke command line.  For example - build pipelines.

The CLI exposes the primary `getEmissionsByLocationsAndTime` SDK methods via command line and outputs the results as json to stdout.  

> You can use the CLI via a docker image.

## Format

`$ CarbonAwareCLI -t <time> -l <location 1> <location 2> -d <path to data file>`


## Parameters

| Short  | Long         | Required / Optional | Description | 
|--------|--------------|---------------------|-------------|
| -l     | --location   | Required            |  The location is a comma seperated list of named locations or regions specific to the emissions data provided.           |
| -d     | --data-file  | Required            | Path to the emissions source data file | 
| -t     | --fromTime   | Optional            |  The desired date and time to retrieve the emissions for.  Defaults to 'now'. |
| -o     | --output     | Optional            | Output format.  Options: console, json.  Default is `json` | 
| -v     | --verbose    | Optional            | Verbose output | 
|        | --lowest     | Optional            | Only return the results with the lowest emissions.  |

## Examples

### Example 1 - Get the current emissions data for a specified location
`$ ./CarbonAwareCLI -l westus -d "azure-emissions-data.json"`
#### Response
<pre>
[
  {
    "Location": "westus",
    "Time": "2021-11-17T04:45:11.5104572+00:00",
    "Rating": 31.0
  }
]
</pre>

### Example 2 - Get the current emissions for multiple locations
 `$ ./CarbonAwareCLI -l westus eastus -d "azure-emissions-data.json"`
#### Response
<pre>
[
  {
    "Location": "westus",
    "Time": "2021-11-17T04:45:11.5104572+00:00",
    "Rating": 31.0
  },
  {
    "Location": "eastus",
    "Time": "2021-11-17T04:45:11.509182+00:00",
    "Rating": 59.0
  }
]
</pre>


### Example 3 - Get the emissions for multiple locations at a specified time
`$ ./CarbonAwareCLI -l westus eastus -t 2021-11-28 -d "azure-emissions-data.json"`
#### Response
<pre>
[
  {
    "Location": "westus",
    "Time": "2021-11-27T20:45:11.5104595+00:00",
    "Rating": 14.0
  },
  {
    "Location": "eastus",
    "Time": "2021-11-27T20:45:11.5092264+00:00",
    "Rating": 17.0
  }
]
</pre>

### Example 4 - Get the lowest emissions for multiple locations at a specified time 
`$ ./CarbonAwareCLI -l westus eastus -t 2021-11-28 --lowest -d "azure-emissions-data.json"`
#### Response
<pre>
[
  {
    "Location": "westus",
    "Time": "2021-11-27T20:45:11.5104595+00:00",
    "Rating": 14.0
  }
]
</pre>

### Example 5 - Get the lowest emissions for multiple locations at a specified time window
`$ ./CarbonAwareCLI -l westus eastus -t 2021-11-27 --toTime 2021-11-29 --lowest -d "azure-emissions-data.json"`
#### Response
<pre>
[
  {
    "Location": "eastus",
    "Time": "2021-11-27T12:45:11.5092264+00:00",
    "Rating": 5.0
  }
]
</pre>

