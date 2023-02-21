namespace CarbonAware.WepApi.UnitTests;

using GSF.CarbonAware.Models;
using CarbonAware.WebApi.Models;
using NUnit.Framework;

public class EmissionsDataDTOTests
{
    [Test]
    public void FromEmissionsData()
    {
        var expectedLocationName = "test location";
        var expectedTimestamp = new DateTimeOffset(2022,1,1,0,0,0, TimeSpan.Zero);
        var expectedDuration = 120;
        var expectedValue = 123.45;
        var emissionsData = new EmissionsData()
        {
            Location = expectedLocationName,
            Time = expectedTimestamp,
            Duration = TimeSpan.FromMinutes(expectedDuration),
            Rating = expectedValue
        };

        var emissionsDataDTO = EmissionsDataDTO.FromEmissionsData(emissionsData);

        Assert.AreEqual(expectedLocationName, emissionsDataDTO?.Location);
        Assert.AreEqual(expectedTimestamp, emissionsDataDTO?.Timestamp);
        Assert.AreEqual(expectedDuration, emissionsDataDTO?.Duration);
        Assert.AreEqual(expectedValue, emissionsDataDTO?.Value);
    }
}