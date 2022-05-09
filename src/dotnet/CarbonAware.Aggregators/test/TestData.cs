using System;
using System.Collections.Generic;
using System.Linq;
using CarbonAware.Model;

namespace CarbonAware.Aggregators.Tests;

public static class TestData
{
  public static IEnumerable<EmissionsData> GetFilteredEmissionDataList(string location, string startTime, string endTime)
    {
        DateTime start = DateTime.Parse(startTime);
        DateTime end = DateTime.Parse(endTime);
        return GetAllEmissionDataList().Where(x => x.Location == location && x.Time >= start && x.Time <= end);
    }

  public static IEnumerable<EmissionsData> GetAllEmissionDataList()
  {
     return  new List<EmissionsData>()
        {
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17"),
                Rating = 10
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17"),
                Rating = 20
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17"),
                Rating = 30
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-19"),
                Rating = 40
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-11-18"),
                Rating = 60
            }
        };
  }

}