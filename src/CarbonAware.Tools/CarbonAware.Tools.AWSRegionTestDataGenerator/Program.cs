using CarbonAware.Tools;
using Newtonsoft.Json;

/// <summary>
/// Generates a test data json string as output based on the 
/// aws-regions.json publicly available from AWS
/// </summary>
var generator = new AWSRegionTestDataGenerator(@"aws-regions.json");

var data = generator.GetRegionData();

var emissions = generator.GenerateTestData(data);

Console.WriteLine(JsonConvert.SerializeObject(emissions));