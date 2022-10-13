using CarbonAware.Model;
using CarbonAware.Tools;
using Newtonsoft.Json;

/// <summary>
/// Generates a test data json string as output based on the 
/// azure-regions.json structure built from the Azure CLI
/// </summary>
var generator = new AzureRegionTestDataGenerator(@"azure-regions.json");

var data = generator.GetRegionData();

var emissions = generator.GenerateTestData(data);

var jsonFile = new EmissionsJsonFile()
{
    Date = DateTime.Now.ToString(),
    Emissions = emissions,
};

Console.WriteLine(JsonConvert.SerializeObject(jsonFile));