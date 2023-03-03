
# 0013. Add option to display CLI output in CSV format

## Status

Proposed

## Context
There is a command line interface in the Carbon Aware SDK that currently contains 2 commands - emissions and emissions-forecasts. Both the commands produces response in a JSON format. There is no option to specify tje output in a different format such as CSV.  

## Decision

The proposal is to have a global option that can be used to specify the output response as CSV. 

### New option

```text
  -o, --output <csv/json> 
```

By default, if not specified, the output response for any command will be presented in JSON format. 
If option value is specified as ‘csv’, the output will be displayed in csv format. 

command:

```bash
.\caw emissions -l eastus --start-time 2023-01-30T00:00:00Z --end-time 2023-01-31T23:59:59Z --output csv
```

```csv 
westeurope,2023-01-31T12:00:00.0000000+00:00,170,PT1H
westeurope,2023-01-31T13:00:00.0000000+00:00,169,PT1H
westeurope,2023-01-31T14:00:00.0000000+00:00,178,PT1H
westeurope,2023-01-31T15:00:00.0000000+00:00,196,PT1H
westeurope,2023-01-31T16:00:00.0000000+00:00,236,PT1H
```
### Limitations of CSV format 

CSV format works best for simple, unnested data. If the data contains nested and repeated objects, csv format makes it less readable and may also be prone to errors. 

In the current SDK, we support ‘emissions’ and ‘emissions-forecasts’ commands. The output of ‘emissions’ command is a simple list of EmissionDTO objects and can be well represented in csv. However, for forecasts, the output contains nested list of forecast objects along with nested list of optimal data points. This is a complicated structure to be represented in a csv format and would provide less value for the end user to decipher this complex structure.  

## Proposed implementation options 


### 1. Design new custom tool in the SDK 

To build a new in-house implementation, we need the following -  

- Create a library of classes that converts an object into a flattened csv format.  
- Write code for serialization/deserialization of the objects. 
- Ensure that the code is generic, and not strictly tied to emissions/forecast data. It should be able to convert any generic object into a csv structure. 
- Use Reflection to access class variables and their types to keep the implementation generic. 

#### Challenges/Risks 

- Time consuming since it must be well designed to make the implementation generic. 
- Representation of nested list of objects in CSV is not much readable and may not provide much value  
- Adding more code in the SDK to be maintained. 
- Thorough testing required. 
- JSON is more acceptable format and is much more readable especially for nested objects. 

 
### 2. Use third party library 

There are several libraries available at nuget,org that provide the tools for converting and serializing to CSV format. Some of the libraries that have been popular are ServiceStack. CSVSeriliazer, CsvHelper etc. 

#### Sample implementation using ServiceStack library 

Add the ServiceStack nuget package using nuget package explorer.  

Create a class that contains only primitive member variables. For eg – From EmiussionsForecastDTO, extract out all the primitive variables. For nested object types, extract the inner primitive variables and add them to the new class. In the following example, ForecastData object is flattened into 2 variables – ForecastRating and ForecastDuration. Also, OptimalDataPoints object is flattened into OptimalTime and OptimalRating. 

```c# 
class EmissionsForecastDTO 
{ 
    public DateTimeOffset GeneratedAt { get; set; } 
    public DateTimeOffset RequestedAt { get; set;}; 
    public string Location {get; set;}  
    public DateTimeOffset DataStartAt {get; set;} 
    public DateTimeOffset DataEndAt {get; set;} 
    public int WindowSize {get; set;} 
    public IEnumerable<EmissionsDataDTO>? OptimalDataPoints{get; set;} 
    public IEnumerable<EmissionsDataDTO>? ForecastData{get; set;} 
}  

class EmissionsForecastCsvDTO 
{ 
   public DateTimeOffset GeneratedAt {get; set;} 
   public DateTimeOffset RequestedAt {get; set;} 
   public string Location {get; set;}; 
   public DateTimeOffset DataStartAt {get; set;} 
   public DateTimeOffset DataEndAt {get; set;} 
   public int WindowSize {get; set;} 
   public DateTimeOffset? ForecastTime {get; set;} 
   public double ForecastRating {get; set;} 
   public TimeSpan? ForecastDuration {get; set;} 
   public DateTimeOffset? OptimalTime {get; set;} 
   public double OptimalRating {get; set;} 
} 
```
Once we have a flattened object, we then use CsvSerializer to do the conversion as follows -  

```c#
 var flattenedObj = emissionsForecast.SelectMany(d => d.ForecastData.Select(s => new EmissionsForecastCsvDTO 
    { 
       DataStartAt = d.DataStartAt, 
       DataEndAt = d.DataEndAt, 
       RequestedAt = d.RequestedAt, 
       WindowSize = d.WindowSize, 
       Location = d.Location, 
       ForecastRating = s.Rating, 
       ForecastDuration = s.Duration, 
       ForecastTime = d.GeneratedAt, 
    })); 


 var csvOutput = CsvSerializer.SerializeToCsv(flattenedObj); 
 context.Console.WriteLine(csvOutput); 
```
This code produces the result in the following format: 

![CSV response](../../images/emissions-forecast-csv.png)

The data is a flattened representation of the nested objects hence it is duplicated across multiple rows. For large data, this representation can be very complex to decipher the actual data. 

Another possible way to represent the data would be to create multiple csv ouputs, each representing specific data. The user would have to create manual links to reference the specific data structures.   

#### Benefits of using Third party 
- Faster implementation since library provides all the tools for conversion and serialization into CSV format 
- Less code to maintain 

#### Limitations/Challenges 
- Choosing a reliable library can be challenging 
- Reduced flexibility since the code is tied to library API 
- Security risks 
- Update library version as new changes/fixes are added 


## Green Impact   

Neutral

