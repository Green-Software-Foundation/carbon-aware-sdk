namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.WebApi.Models;
using Moq;
using NUnit.Framework;

public class EmissionsForecastDTOTests
{
    [Test]
    public void FromEmissionsForecast()
    {
        var expectedGeneratedAt = new DateTimeOffset(2022,1,1,0,0,0,TimeSpan.Zero);
        var expectedStartTime = new DateTimeOffset(2022,1,1,0,1,0,TimeSpan.Zero);
        var expectedEndTime = new DateTimeOffset(2022,1,1,0,2,0,TimeSpan.Zero);
        var expectedLocationName = "test location";
        var expectedWindowSize = 10;
        var expectedOptimalValue = 98.76d;
        var expectedDataPointValue = 123.456d;

        var emissionsForecast = new EmissionsForecast()
        {
            GeneratedAt = expectedGeneratedAt,
            Location = new Location(){ LocationType = LocationType.CloudProvider, RegionName = expectedLocationName },
            DataStartAt =  expectedStartTime,
            DataEndAt =  expectedEndTime,
            WindowSize = TimeSpan.FromMinutes(expectedWindowSize),
            ForecastData = new List<EmissionsData>(){ new EmissionsData(){ Rating = expectedDataPointValue } },
            OptimalDataPoint = new EmissionsData(){ Rating = expectedOptimalValue }
        };

        var emissionsForecastDTO = EmissionsForecastDTO.FromEmissionsForecast(emissionsForecast);
        var emissionsDataDTO = emissionsForecastDTO.ForecastData?.ToList();

        Assert.AreEqual(expectedGeneratedAt, emissionsForecastDTO.GeneratedAt);
        Assert.AreEqual(expectedLocationName, emissionsForecastDTO.Location);
        Assert.AreEqual(expectedStartTime, emissionsForecastDTO.DataStartAt);
        Assert.AreEqual(expectedEndTime, emissionsForecastDTO.DataEndAt);
        Assert.AreEqual(expectedWindowSize, emissionsForecastDTO.WindowSize);
        Assert.AreEqual(expectedOptimalValue, emissionsForecastDTO.OptimalDataPoint?.Value);
        Assert.AreEqual(1, emissionsDataDTO?.Count());
        Assert.AreEqual(expectedDataPointValue, emissionsDataDTO?.First().Value);
    }
}