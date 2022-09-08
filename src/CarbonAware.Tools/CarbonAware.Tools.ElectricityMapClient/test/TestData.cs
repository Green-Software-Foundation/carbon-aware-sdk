using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CarbonAware.Tools.ElectricityMapClient.Tests;

public static class TestData
{

    public static string GetCurrentForecastJsonString()
    {
        var json = new JsonObject
          {

            ["_disclaimer"] = "This data is the exclusive property of electricityMap and/or related parties. If you're in doubt about your rights to use this data, please contact api@co2signal.com",
            ["status"] = "ok",
            ["countryCode"] = "AUS-NSW",
            ["data"] = new JsonObject
            {
                ["datetime"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ["carbonIntensity"] = 999.99,
                ["fossilFuelPercentage"] = 99.99
            },
            ["units"] = new JsonObject
            {
                ["carbonIntensity"] = "gCO2eq/kWh"
            }
          };
        return json.ToString();
    }
}