using System.Text.Json.Nodes;
using System;

namespace CarbonAware.DataSources.ElectricityMapsFree.Tests;

public static class TestData
{
    private static readonly string disclaimer = "This data is the exclusive property of Electricity Maps and/or related parties. "
                                              + "If you're in doubt about your rights to use this data, please contact api@co2signal.com";
    private static readonly string TestZoneId1 = "US-CAL-CISO";
    private static readonly string TestZoneName1 = "California Independent System Operator";

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

    public static string GetLatestCarbonIntensityDataJsonString()
    {
        var json = new JsonObject
        {
            ["_disclaimer"] = disclaimer,
            ["status"] = "ok",
            ["countryCode"] = TestZoneId1,
            ["data"] = new JsonObject
            {
                ["datetime"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ["carbonIntensity"] = 999,
                ["fossilFuelPercentage"] = 53.33
            },
            ["units"] = new JsonObject
            {
                ["carbonIntensity"] = "gCO2eq/kWh"
            }
        };

        return json.ToString();
    }
}