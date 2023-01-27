using CarbonAware.CLI.Commands.EmissionsForecasts;
using GSF.CarbonAware.Models;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CarbonAware.CLI.UnitTests.Commands.EmissionsForecasts;

[TestFixture]
public class EmissionsForecastsCommandTests : TestBase
{
    [Test]
    public async Task Run_CallsAggregatorWithLocationOptions()
    {
        // Arrange
        var forecastCommand = new EmissionsForecastsCommand();
        var longAliasLocation = "eastus";
        var shortAliasLocation = "westus";
        var invocationContext = SetupInvocationContext(forecastCommand, $"emissions-forecasts --location {longAliasLocation} -l {shortAliasLocation}");
        var expectedLocations = new List<string>() { longAliasLocation, shortAliasLocation };
        IEnumerable<string> actualLocations = Array.Empty<string>();

        _mockForecastHandler.Setup(agg => agg.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()))
            .Callback((string[] locations, DateTimeOffset? start, DateTimeOffset? end, int? windowSize) =>
            {
                actualLocations = locations;
            });

        // Act
        await forecastCommand.Run(invocationContext);

        // Assert
        _mockForecastHandler.Verify(agg => agg.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()), Times.Once);
        CollectionAssert.AreEquivalent(expectedLocations, actualLocations);
    }

    [TestCase("--data-start-at", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastsCommandTests.Run DataStartTimeOption: long alias")]
    [TestCase("-s", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastsCommandTests.Run DataStartTimeOption: short alias")]
    public async Task Run_CallsCurrentForecast_WithStartTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsForecastsCommand = new EmissionsForecastsCommand();
        var invocationContext = SetupInvocationContext(emissionsForecastsCommand, $"emissions-forecasts {alias} {optionValue}");
        var expectedStartTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset? actualStartTime = default;

        _mockForecastHandler.Setup(agg => agg.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()))
            .Callback((string[] locations, DateTimeOffset? start, DateTimeOffset? end, int? windowSize) =>
            {
                actualStartTime = start;
            });
        // Act
        await emissionsForecastsCommand.Run(invocationContext);

        // Assert
        _mockForecastHandler.Verify(agg => agg.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()), Times.Once);
        Assert.AreEqual(expectedStartTime, actualStartTime);
    }

    [TestCase("--data-end-at", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastsCommandTests.Run DataEndTimeOption: long alias")]
    [TestCase("-e", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastsCommandTests.Run DataEndTimeOption: short alias")]
    public async Task Run_CallsCurrentForecast_WithEndTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsForecastsCommand = new EmissionsForecastsCommand();
        var invocationContext = SetupInvocationContext(emissionsForecastsCommand, $"emissions-forecasts {alias} {optionValue}");
        var expectedStartTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset? actualEndTime = default;

        _mockForecastHandler.Setup(agg => agg.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()))
            .Callback((string[] locations, DateTimeOffset? start, DateTimeOffset? end, int? windowSize) =>
            {
                actualEndTime = end;
            });
        // Act
        await emissionsForecastsCommand.Run(invocationContext);

        // Assert
        _mockForecastHandler.Verify(agg => agg.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()), Times.Once);
        Assert.AreEqual(expectedStartTime, actualEndTime);
    }

    [TestCase("--requested-at", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastsCommandTests.Run RequestedAt: long alias")]
    [TestCase("-r", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastsCommandTests.Run RequestedAt: short alias")]
    public async Task Run_CallsHistoricForecast_WhenRequestedAtProvided(string alias, string optionValue)
    {
        // Arrange
        var emissionsForecastsCommand = new EmissionsForecastsCommand();
        var invocationContext = SetupInvocationContext(emissionsForecastsCommand, $"emissions-forecasts -l eastus {alias} {optionValue}");
        var emissions = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = "useast",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        EmissionsForecast expectedForecast = new()
        {
            EmissionsDataPoints = emissions,
            OptimalDataPoints = emissions
        };
        _mockForecastHandler.Setup(agg => agg.GetForecastByDateAsync(It.IsAny<string>(), null, null, It.IsAny<DateTimeOffset?>(), null)).ReturnsAsync(expectedForecast);

        // Act
        await emissionsForecastsCommand.Run(invocationContext);

        // Assert
        _mockForecastHandler.Verify(agg => agg.GetForecastByDateAsync(It.IsAny<string>(), null, null, It.IsAny<DateTimeOffset?>(), null));
    }
}