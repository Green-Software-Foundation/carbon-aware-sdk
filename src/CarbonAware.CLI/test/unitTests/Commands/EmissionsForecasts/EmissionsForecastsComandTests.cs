using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.CLI.Commands.EmissionsForecasts;
using CarbonAware.Model;
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

        _mockCarbonAwareAggregator.Setup(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) =>
            {
                actualLocations = _parameters.MultipleLocations.Select(l => l.Name!);
            });

        // Act
        await forecastCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
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
        DateTimeOffset actualStartTime = default;

        _mockCarbonAwareAggregator.Setup(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) =>
            {
                actualStartTime = _parameters.Start;
            });
        // Act
        await emissionsForecastsCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
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
        DateTimeOffset actualEndTime = default;

        _mockCarbonAwareAggregator.Setup(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) =>
            {
                actualEndTime = _parameters.End;
            });
        // Act
        await emissionsForecastsCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
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
            ForecastData = emissions,
            OptimalDataPoints = emissions
        };
        _mockCarbonAwareAggregator.Setup(agg => agg.GetForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ReturnsAsync(expectedForecast);

        // Act
        await emissionsForecastsCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
    }
}