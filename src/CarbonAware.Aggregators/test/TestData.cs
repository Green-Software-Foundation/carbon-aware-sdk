using System;
using System.Collections.Generic;
using System.Linq;
using CarbonAware.Model;

namespace CarbonAware.Aggregators.Tests;

public static class TestData
{
  public static IEnumerable<EmissionsData> GetFilteredEmissionDataList(string location, string startTime, string endTime)
    {
        DateTimeOffset start = DateTimeOffset.Parse(startTime);
        DateTimeOffset end = DateTimeOffset.Parse(endTime);
        return GetAllEmissionDataList().Where(x => x.Location == location && x.Time >= start && x.Time <= end);
    }

  public static IEnumerable<EmissionsData> GetAllEmissionDataList()
  {
     return  new List<EmissionsData>()
        {
            new EmissionsData {
                Location = "westus",
                Time = DateTimeOffset.Parse("2021-11-17"),
                Rating = 10
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTimeOffset.Parse("2021-11-17"),
                Rating = 20
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTimeOffset.Parse("2021-11-17"),
                Rating = 30
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTimeOffset.Parse("2021-11-19"),
                Rating = 40
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTimeOffset.Parse("2021-11-18"),
                Rating = 60
            }
        };
    }

    public static EmissionsForecast GetForecast(int durationMinutes = 5)
    {
        var startTime = DateTimeOffset.Parse("2022-01-01T00:00:00Z");
        var duration = TimeSpan.FromMinutes(durationMinutes);
        var forecastData = new List<EmissionsData>()
        {
            new EmissionsData {
                Location = "westus",
                Time = startTime,
                Rating = 10,
                Duration = duration
            },
            new EmissionsData {
                Location = "westus",
                Time = startTime.AddMinutes(durationMinutes),
                Rating = 20,
                Duration = duration
            },
            new EmissionsData {
                Location = "westus",
                Time = startTime.AddMinutes(durationMinutes*2),
                Rating = 30,
                Duration = duration
            },
            new EmissionsData {
                Location = "westus",
                Time = startTime.AddMinutes(durationMinutes*3),
                Rating = 40,
                Duration = duration
            }
        };

        return new EmissionsForecast(){
            GeneratedAt = DateTimeOffset.Parse("2022-01-01T00:00:00Z"),
            Location = new Location() {
                RegionName = "westus",
                CloudProvider = CloudProvider.Azure
            },
            ForecastData = forecastData
        };
    }

}