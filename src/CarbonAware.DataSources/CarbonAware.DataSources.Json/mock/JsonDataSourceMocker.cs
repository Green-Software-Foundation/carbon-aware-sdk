using CarbonAware.Interfaces;
using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.Model;
using System.Text.Json;

namespace CarbonAware.DataSources.Json.Mocks;
internal class JsonDataSourceMocker : IDataSourceMocker
{
    public JsonDataSourceMocker() { }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        string path = new JsonDataSourceConfiguration().DataFileLocation;

        var data = new List<EmissionsData>();
        DateTimeOffset pointTime = start;
        TimeSpan duration = TimeSpan.FromHours(8);

        while (pointTime < end)
        {
            var newDataPoint = new EmissionsData()
            {
                Location = location,
                Time = pointTime,
                Rating = 999.99,
                Duration = duration,
            };
            
            data.Add(newDataPoint);
            pointTime = newDataPoint.Time + duration;
        }
        
        var json = new EmissionsJsonFile
        {
            Emissions = data,
        };

        File.WriteAllText(path, JsonSerializer.Serialize(json));
    }
    public void SetupForecastMock() 
    {               
        List<EmissionsForecast> emissionsForecasts = new List<EmissionsForecast>();

        var locations = new string[] { "eastus", "westus" };

        var ran = new Random(DateTimeOffset.Now.Millisecond);
        var startTime = DateTimeOffset.Now;
        var maxMinutesOffset = 24 * 60;

        foreach (var location in locations)
        {
            List<EmissionsData> emissionsData = new List<EmissionsData>();

            for (var minutes = 0; minutes < maxMinutesOffset; minutes += 5)
            {
                var e = new EmissionsData
                {
                    Time = startTime + TimeSpan.FromMinutes(minutes),
                    Location = location ?? string.Empty,
                    Rating = ran.Next(100)
                };
                emissionsData.Add(e);
            }

            emissionsForecasts.Add(new EmissionsForecast
            {
                Location = new Location { Name = location },
                ForecastData = emissionsData,
            });
        }

        string path = new JsonDataSourceConfiguration().DataFileLocation;
        var json = new EmissionsJsonFile
        {
            EmissionsForecasts = emissionsForecasts,
        };

        File.WriteAllText(path, JsonSerializer.Serialize(json));
    }
    public void Initialize() { }
    public void Reset() { }
    public void Dispose() { }

    public void SetupBatchForecastMock()
    {
        throw new NotImplementedException();
    }
}