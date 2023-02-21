using System.Text.Json.Nodes;

namespace CarbonAware.DataSources.ElectricityMaps.Tests;

public static class TestData
{
    private static readonly string TestZoneId1 = "NL";
    private static readonly string TestZoneName1 = "Netherlands";

    public static string GetZonesAllowedJsonString()
    {
        var json = new JsonObject
        {
            [TestZoneId1] = new JsonObject
            {
                ["zoneName"] = TestZoneName1,
                ["access"] = new JsonArray
                {
                    "carbon-intensity/history",
                    "carbon-intensity/forecast",
                }
            }
        };

        return json.ToString();
    }

    public static string GetNoPathsSupportedJsonString()
    {
        var json = new JsonObject
        {
            [TestZoneId1] = new JsonObject
            {
                ["zoneName"] = TestZoneName1,
                ["access"] = new JsonArray
                {
                }
            }
        };

        return json.ToString();
    }

    public static string GetNoZonesSupportedJsonString()
    {
        var json = new JsonObject
        {
        };

        return json.ToString();
    }

    public static string GetHistoryCarbonIntensityDataJsonString()
    {
        var json = new JsonObject
        {
            ["zone"] = TestZoneId1,
            ["history"] = new JsonArray
            {
                new JsonObject
                {
                    ["datetime"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    ["updatedAt"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    ["createdAt"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    ["carbonIntensity"] = 999,
                    ["emissionFactorType"] = "lifecycle",
                    ["isEstimated"] = false,
                    ["estimatedMethod"] = null,
                }
            }
        };

        return json.ToString();
    }

    public static string GetCurrentForecastJsonString()
    {

        var json = new JsonObject
        {
            ["zone"] = TestZoneId1,
            ["updatedAt"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ["forecast"] = new JsonArray
            {
                new JsonObject
                {
                    ["datetime"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    ["carbonIntensity"] = 999,
                }
            }
        };

        return json.ToString();
    }
}