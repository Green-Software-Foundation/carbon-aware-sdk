using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CarbonAware.Tools.WattTimeClient.Tests;

public static class TestData
{
    public static string GetGridDataJsonString()
    {
        var json = new JsonArray(
          new JsonObject
          {
              ["ba"] = "ba",
              ["datatype"] = "dt",
              ["frequency"] = 300,
              ["market"] = "mkt",
              ["point_time"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
              ["value"] = 999.99,
              ["version"] = "1.0"
          }
        );

        return json.ToString();
    }

    public static string GetCurrentForecastJsonString()
    {

        var json = new JsonObject
        {
            ["generated_at"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ["forecast"] = new JsonArray
            {
                new JsonObject
                {
                    ["ba"] = "ba",
                    ["point_time"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    ["value"] = 999.99,
                    ["version"] = "1.0"
                }
            }
        };

        return json.ToString();
    }

    public static string GetForecastByDateJsonString()
    {
        var json = new JsonArray
        {
            new JsonObject
            {
                ["generated_at"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ["forecast"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["ba"] = "ba",
                        ["point_time"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        ["value"] = 999.99,
                        ["version"] = "1.0"
                    }
                }
            }
        };

        return json.ToString();
    }

    public static string GetBalancingAuthorityJsonString()
    {
        var json = new JsonObject
        {
            ["id"] = "12345",
            ["abbrev"] = "TEST_BA",
            ["name"] = "Test Balancing Authority"
        };

        return json.ToString();
    }
}