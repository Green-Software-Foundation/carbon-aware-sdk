## Carbon Aware CLI Reference

The following is the documentation for the Carbon Aware CLI

### Format

>   `$ CarbonAwareCLI -t <time> -l <location 1> <location 2> -o json -d <path to data file>`


### Parameters

| Short  | Long         | Required / Optional | Description | 
|--------|--------------|---------------------|-------------|
| -l     | --location   | Required            |  The location is a comma seperated list of named locations or regions specific to the emissions data provided.           |
| -t     | --fromTime   | Optional            |  The desired date and time to retrieve the emissions for.  Defaults to 'now'. |
| -o      | --output    | Optional            | Output format.  Options: console, json.  Default is `json` | 
| -v      | --verbose   | Optional            | Verbose output | 
| -d      | --data-file   | Required            | Path to the emissions source data file | 
|         | --lowest | Optional | Only return the results with the lowest emissions.  |

## Examples

### Get the current emissions for a location
> `$ ./CarbonAwareCLI -l westus -d "azure-emissions-data.json"`
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

### Get the current emissions for multiple locations
> `$ ./CarbonAwareCLI -l westus eastus -d "azure-emissions-data.json"`
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


### Get the emissions for multiple locations at a specified time
> `$ ./CarbonAwareCLI -l westus eastus -t 2021-11-28 -d "azure-emissions-data.json"`
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

### Get the lowest emissions for multiple locations at a specified time 
> `$ ./CarbonAwareCLI -l westus eastus -t 2021-11-28 --lowest -d "azure-emissions-data.json"`
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

### Get the lowest emissions for multiple locations at a specified time window
> `$ ./CarbonAwareCLI -l westus eastus -t 2021-11-27 --toTime 2021-11-29 --lowest -d "azure-emissions-data.json"`
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

