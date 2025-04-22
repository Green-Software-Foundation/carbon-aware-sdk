using CarbonAware.Interfaces;
using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.Model;
using System.Text.Json;

namespace CarbonAware.DataSources.Json.Mocks;
public class JsonDataSourceMocker : IDataSourceMocker
{

    public string DataFileName { get; set; }

    public JsonDataSourceMocker() {
        DataFileName = "test-data-azure-emissions.json";
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        var config = new JsonDataSourceConfiguration();
        config.DataFileLocation = DataFileName;
        string path = config.DataFileLocation;

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
        
        var json = new 
        {
            Emissions = data
        };

        File.WriteAllText(path, JsonSerializer.Serialize(json));
    }
    public void SetupForecastMock() { }
    public void Initialize() { }
    public void Reset() { }
    public void Dispose() { }

    public void SetupHistoricalBatchForecastMock()
    {
        throw new NotImplementedException();
    }
}