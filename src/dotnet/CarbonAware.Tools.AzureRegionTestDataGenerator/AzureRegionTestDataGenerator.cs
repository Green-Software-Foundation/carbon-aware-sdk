using CarbonAware.Model;
using Newtonsoft.Json;

namespace CarbonAware.Tools;

/// <summary>
/// Generates a test data json string as output based on the 
/// azure-regions.json structure 
/// </summary>
public class AzureRegionTestDataGenerator
{
    public class AzureRegionData
    {
        public string? name { get; set; }
    }

    private string _fileName { get; }

    public AzureRegionTestDataGenerator(string fileName)
    {
        _fileName = fileName;
    }

    public List<AzureRegionData> GetRegionData()
    {
        using StreamReader file = File.OpenText(_fileName);
        var jsonObject = JsonConvert.DeserializeObject<List<AzureRegionData>>(file.ReadToEnd());

        return jsonObject;
    }

    public List<EmissionsData> GenerateTestData(List<AzureRegionData> regionData)
    {
        List<EmissionsData> emData = new List<EmissionsData>();
        var ran = new Random(DateTime.Now.Millisecond);

        foreach (var region in regionData)
        {
            for (var days = 0; days < 365; days++)
            {
                for (var hours = 0; hours < 3; hours++)
                {
                    var e = new EmissionsData()
                    {
                        // 3 times per day (8 hours apart), 365 days per year 
                        Time = DateTime.Now + TimeSpan.FromHours(8 * hours) + TimeSpan.FromDays(days),
                        Location = region.name,
                        Rating = ran.Next(100)
                    };
                    emData.Add(e);
                }
            }
        }

        return emData;
    }
}
