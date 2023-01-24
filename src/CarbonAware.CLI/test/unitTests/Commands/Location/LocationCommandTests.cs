using CarbonAware.CLI.Commands.Location;
using Moq;
using NUnit.Framework;
using CarbonAware.Model;

namespace CarbonAware.CLI.UnitTests;

[TestFixture]
public class LocationCommandTests : TestBase
{
    [Test]
    public async Task Run_CallsLocationSourceAndWritesResults()
    {
        // Arrange
        var locationsCommand = new LocationsCommand();
        var invocationContext = SetupInvocationContext(locationsCommand, "locations");

        var expectedLocations = new Dictionary<string, Location>()
        {
            {
                "eastus", new Location() { Name = "eastus"}
            },
            {
                "westus", new Location() { Name = "westus"}
            }
        };

        _mockLocationSource.Setup(ls => ls.GetGeopositionLocationsAsync())
            .ReturnsAsync(expectedLocations);

        // Act
        await locationsCommand.Run(invocationContext);

        // Assert
        string? consoleOutput = _console.Out.ToString();
        foreach (var location in expectedLocations.Keys)
        {
            StringAssert.Contains(location, consoleOutput);
        }

        _mockLocationSource.Verify(ls => ls.GetGeopositionLocationsAsync(), Times.Once);
    }
}
