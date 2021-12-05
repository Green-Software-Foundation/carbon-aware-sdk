using CarbonAware.Model;
using Newtonsoft.Json;

namespace CarbonAware.Tools;

public class AWSRegionTestDataGenerator
{
    public class AwsRegionData
    {
        public string? name { get; set; }
        public string? full_name { get; set; }
        public string? code { get; set; }
        public List<string>? zones { get; set; }
    }

    private string _fileName { get; }

    public AWSRegionTestDataGenerator(string fileName)
    {
        _fileName = fileName;
    }

    public List<AwsRegionData> GetRegionData()
    {
        using StreamReader file = File.OpenText(_fileName);
        var jsonObject = JsonConvert.DeserializeObject<List<AwsRegionData>>(file.ReadToEnd());

        return jsonObject;
    }

    public List<EmissionsData> GenerateTestData(List<AwsRegionData> regionData)
    {
        List<EmissionsData> emData = new List<EmissionsData>();
        var ran = new Random(DateTime.Now.Millisecond);

        foreach (var region in regionData)
        {
            for (var i = 0; i < 24; i++)
            {
                var e = new EmissionsData()
                {
                    Time = DateTime.Now + TimeSpan.FromHours(i),
                    Location = region.code,
                    Rating = ran.Next(100)
                };
                emData.Add(e);
            }
        }

        return emData;
    }
}
