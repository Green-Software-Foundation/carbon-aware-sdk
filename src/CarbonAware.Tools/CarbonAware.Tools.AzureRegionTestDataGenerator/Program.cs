using CarbonAware.Model;
using CarbonAware.Tools;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Generates a test data json string as output based on the 
/// azure-regions.json structure built from the Azure CLI
/// </summary>
var generator = new AzureRegionTestDataGenerator(@"azure-regions.json");

var data = generator.GetRegionData()!;

var emissions = generator.GenerateTestEmissionsData(data);
var emissionsForecasts = generator.GenerateTestEmissionsForecastsData(data);

var jsonFile = new EmissionsJsonFile()
{
    Date = DateTime.Now.ToString(),
    Emissions = emissions,
    EmissionsForecasts = emissionsForecasts
};

Console.WriteLine(JsonSerializer.Serialize(jsonFile, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault }));