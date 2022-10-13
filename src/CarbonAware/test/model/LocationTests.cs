namespace CarbonAware.Tests;

using CarbonAware.Model;
using NUnit.Framework;

public class LocationTests
{
    [TestCase(LocationType.Geoposition, ExpectedResult="12.345, 67.89")]
    [TestCase(LocationType.CloudProvider, ExpectedResult="test region")]
    [TestCase(LocationType.NotProvided, ExpectedResult="Not Provided")]
    public string DisplayName(LocationType locationType)
    {
        var location = new Location()
        {
            LocationType = locationType,
            Latitude = 12.345m,
            Longitude = 67.89m,
            RegionName = "test region"
        };

        return location.DisplayName;
    }
}