using GSF.CarbonAware.Handlers;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Interfaces;
using System.Collections.Immutable;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class LocationHandlerTests
{

    [Test]
    public async Task GetEmpty_LocationsAsync_Succeed_Call()
    {
        var mockSource = new Mock<ILocationSource>();
        mockSource
            .Setup(x => x.GetGeopositionLocationsAsync())
            .ReturnsAsync(CreateLocationDictionary(false));

        var handler = new LocationHandler(mockSource.Object);
        var result = await handler.GetLocationsAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAll_LocationsAsync_Succeed_Call()
    {
        var mockSource = new Mock<ILocationSource>();
        mockSource
            .Setup(x => x.GetGeopositionLocationsAsync())
            .ReturnsAsync(CreateLocationDictionary());

        var handler = new LocationHandler(mockSource.Object);
        var result = await handler.GetLocationsAsync();
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.TryGetValue("eastus", out var value1), Is.True);
        Assert.That(value1, Is.Not.Null);
        Assert.That(value1?.Name, Is.EqualTo("eastus"));
        Assert.That(result.TryGetValue("westus", out var value2), Is.True);
        Assert.That(value2, Is.Not.Null);
        Assert.That(value2?.Name, Is.EqualTo("westus"));
    }

    private static IDictionary<string, global::CarbonAware.Model.Location> CreateLocationDictionary(bool withcontent = true)
    {
        if (!withcontent)
        {
            return ImmutableDictionary<string, global::CarbonAware.Model.Location>.Empty;
        }
        return new Dictionary<string, global::CarbonAware.Model.Location>()
        {
            {
                "eastus", new global::CarbonAware.Model.Location() { Name = "eastus" }
            },
            {
                "westus", new global::CarbonAware.Model.Location() { Name = "westus" }
            }
        };
    }
}
