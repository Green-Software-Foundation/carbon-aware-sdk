using CarbonAware.Model;
using System.Text.Json;

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

    public List<AzureRegionData>? GetRegionData()
    {
        using StreamReader file = File.OpenText(_fileName);
        return JsonSerializer.Deserialize<List<AzureRegionData>>(file.ReadToEnd(), new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });
    }

    public List<EmissionsData> GenerateTestEmissionsData(List<AzureRegionData> regionData)
    {
        List<EmissionsData> emData = new List<EmissionsData>();
        var ran = new Random(DateTimeOffset.Now.Millisecond);
        var startTime = DateTimeOffset.Now;

        foreach (var region in regionData)
        {
            for (var days = 0; days < 365; days++)
            {
                for (var hours = 0; hours < 3; hours++)
                {
                    var e = new EmissionsData()
                    {
                        // 3 times per day (8 hours apart), 365 days per year 
                        Time = startTime + TimeSpan.FromHours(8 * hours) + TimeSpan.FromDays(days),
                        Location = region.name ?? string.Empty,
                        Rating = ran.Next(99) + 1
                    };
                    emData.Add(e);
                }
            }
        }

        return emData;
    }

    public List<EmissionsForecast> GenerateTestEmissionsForecastsData(List<AzureRegionData> regionData)
    {
        List<EmissionsForecast> emissionsForecasts = new List<EmissionsForecast>();

        var ran = new Random(DateTimeOffset.Now.Millisecond);
        var startTime = DateTimeOffset.Now;
        var maxMinutesOffset = 24 * 60;

        foreach (var region in regionData)
        {
            List<EmissionsData> emissionsData = new List<EmissionsData>();
            
            for (var minutes = 0; minutes < maxMinutesOffset; minutes += 5)
            {

                var e = new EmissionsData()
                {                    
                    Time = startTime + TimeSpan.FromMinutes(minutes),
                    Rating = ran.Next(99) + 1,
                    // Setting Location to default to ignore it in the JSON serialization
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type. 
                    Location = default
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                };
                emissionsData.Add(e);
            }

            emissionsForecasts.Add(new EmissionsForecast
            {
                Location = new Location { Name = region.name },
                ForecastData = emissionsData,
                // Setting OptimalDataPoints to default to ignore it in the JSON serialization
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type. 
                OptimalDataPoints = default
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }); 
        }

        return emissionsForecasts;
    }

}
