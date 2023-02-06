namespace CarbonAware.WepApi.UnitTests;

using GSF.CarbonAware.Models;
using CarbonAware.WebApi.Models;
using NUnit.Framework;

public class EmissionsForecastDTOTests
{
    [Test]
    public void FromEmissionsForecast()
    {
        var expectedGeneratedAt = new DateTimeOffset(2022,1,1,0,0,0,TimeSpan.Zero);
        var expectedOptimalValue = 98.76d;
        var expectedDataPointValue = 123.456d;

        var emissionsForecast = new EmissionsForecast()
        {
            GeneratedAt = expectedGeneratedAt,
            EmissionsDataPoints = new List<EmissionsData>(){ new EmissionsData(){ Rating = expectedDataPointValue } },
            OptimalDataPoints = new List<EmissionsData>(){ new EmissionsData(){ Rating = expectedOptimalValue } }
        };

        var emissionsForecastDTO = EmissionsForecastDTO.FromEmissionsForecast(emissionsForecast);
        var emissionsDataDTO = emissionsForecastDTO.ForecastData?.ToList();

        Assert.AreEqual(expectedGeneratedAt, emissionsForecastDTO.GeneratedAt);
        Assert.AreEqual(expectedOptimalValue, emissionsForecastDTO.OptimalDataPoints?.First().Value);
        Assert.AreEqual(1, emissionsDataDTO?.Count());
        Assert.AreEqual(expectedDataPointValue, emissionsDataDTO?.First().Value);
    }
}