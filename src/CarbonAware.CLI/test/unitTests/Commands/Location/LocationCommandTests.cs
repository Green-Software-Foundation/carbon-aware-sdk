using CarbonAware.CLI.Commands.Location;
using GSF.CarbonAware.Models;
using Moq;
using NUnit.Framework;

namespace CarbonAware.CLI.UnitTests;

[TestFixture]
internal class LocationCommandTests : TestBase
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

        _mockLocationHandler.Setup(ls => ls.GetLocationsAsync())
            .ReturnsAsync(expectedLocations);

        // Act
        await locationsCommand.Run(invocationContext);

        // Assert
        string? consoleOutput = _console.Out.ToString();
        foreach (var location in expectedLocations.Keys)
        {
            StringAssert.Contains(location, consoleOutput);
        }

        _mockLocationHandler.Verify(ls => ls.GetLocationsAsync(), Times.Once);
    }
}
