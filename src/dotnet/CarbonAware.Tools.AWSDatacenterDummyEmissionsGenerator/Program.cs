using CarbonAware.Tools;
using Newtonsoft.Json;

/// <summary>
/// Generates a dummy data json string as output based on the 
/// aws-regions.json publicly available from AWS
/// </summary>
var generator = new AWSRegionDummyDataGenerator(@"aws-regions.json");

var data = generator.GetRegionData();

var emissions = generator.GenerateDummyData(data);

Console.WriteLine(JsonConvert.SerializeObject(emissions));