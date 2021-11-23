
using CarbonAware.Data.Sample;
using CarbonAware.Tools;
using Newtonsoft.Json;

/// <summary>
/// Generates a dummy data json string as output based on the 
/// azure-regions.json structure built from the Azure CLI
/// </summary>
var generator = new AzureRegionDummyDataGenerator(@"azure-regions.json");

var data = generator.GetRegionData();

var emissions = generator.GenerateDummyData(data);

var jsonFile = new EmissionsJsonFile()
{
    Date = DateTime.Now.ToString(),
    Emissions = emissions,
};

Console.WriteLine(JsonConvert.SerializeObject(jsonFile));