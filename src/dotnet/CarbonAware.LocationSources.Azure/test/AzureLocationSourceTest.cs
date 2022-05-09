using CarbonAware.Model;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using CarbonAware.Exceptions;

namespace CarbonAware.LocationSources.Azure.Test;

public class AzureLocationSourceTest
{   
    [Test]
    public async Task TestToGeopositionLocation_ValidLocation()
    {
        var logger = Mock.Of<ILogger<AzureLocationSource>>();

        var mockLocationSource = SetupMockLocationSource().Object;
        Location inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            CloudProvider = CloudProvider.Azure,
            RegionName = "eastus"
        };

        var eastResult = await mockLocationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);

        inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            CloudProvider = CloudProvider.Azure,
            RegionName = "westus"
        };

        var westResult = await mockLocationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);

    }

    // <summary>
    // If an Azure Location with invalid RegionName is passed, should fail.
    // </summary>
    [Test]
    public void TestToGeopositionLocation_InvalidLocation()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        Location invalidLocation = new Location()
        {
            RegionName = "invalid location"
        };
        Assert.ThrowsAsync<LocationConversionException>(async() =>
        {
            await mockLocationSource.ToGeopositionLocationAsync(invalidLocation);
        });
    }

    /// <summary>
    /// If a Location with type LocationType.Geoposition is passed in, function
    /// returns original Location.
    /// </summary>
    [Test]
    public async Task TestToGeopositionLocation_AlreadyGeopositionLocation()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        Location location = new Location {
            LocationType = LocationType.Geoposition
        };
        var result = await mockLocationSource.ToGeopositionLocationAsync(location);
        Assert.AreEqual(location, result);
    }

    private static Mock<AzureLocationSource> SetupMockLocationSource() {
        var logger = Mock.Of<ILogger<AzureLocationSource>>();
        var mockLocationSource = new Mock<AzureLocationSource>(logger);
        
        mockLocationSource.Protected()
            .Setup<Task<Dictionary<string, NamedGeoposition>>>("LoadRegionsFromJsonAsync")
            .Returns(Task.FromResult(GetTestDataRegions()))
            .Verifiable();

        return mockLocationSource;
    }

    private static Dictionary<string, NamedGeoposition> GetTestDataRegions() {
        // All the tests above correspond to values in this mock data. If the mock values are changed, the tests need to be updated 
        return new Dictionary<string, NamedGeoposition>() {
            {"eastus", Constants.EastUsRegion },
            {"westus", Constants.WestUsRegion },
            {"northcentralus", Constants.NorthCentralRegion }
        };
    }

    private static void AssertLocationsEqual(Location expected, Location actual)
    {
        Assert.AreEqual(expected.LocationType, actual.LocationType);
        Assert.AreEqual(expected.Latitude, actual.Latitude);
        Assert.AreEqual(expected.Longitude, actual.Longitude);
    }
}
