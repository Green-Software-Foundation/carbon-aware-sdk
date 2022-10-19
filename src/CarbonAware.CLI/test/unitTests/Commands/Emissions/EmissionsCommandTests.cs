using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.Model;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CarbonAware.CLI.UnitTests;

[TestFixture]
public class EmissionsCommandTests : TestBase
{
    [Test]
    public async Task Run_CallsAggregatorAndWritesResults()
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, "emissions");

        var expectedEmissions = new EmissionsData()
        {
            Location = "eastus",
            Time = DateTimeOffset.Parse("2022-01-01T00:00:00+00:00"),
            Duration = TimeSpan.Parse("01:00:00"),
            Rating = 100.7
        };
      
        _mockEmissionsAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ReturnsAsync(new List<EmissionsData>() { expectedEmissions });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        string? consoleOutput = _console.Out.ToString();
        StringAssert.Contains(expectedEmissions.Location, consoleOutput);
        StringAssert.Contains(expectedEmissions.Rating.ToString(), consoleOutput);
        StringAssert.Contains(expectedEmissions.Time.ToString("yyyy-MM-ddTHH:mm:sszzz"), consoleOutput);
        StringAssert.Contains(expectedEmissions.Duration.ToString(), consoleOutput);
       

        _mockEmissionsAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
    }

    [Test]
    public async Task Run_CallsAggregatorWithLocationOptions()
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var longAliasLocation = "eastus";
        var shortAliasLocation = "westus";
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions --location {longAliasLocation} -l {shortAliasLocation}");
        var expectedLocations = new List<string>() { longAliasLocation, shortAliasLocation };
        IEnumerable<string> actualLocations = Array.Empty<string>();

        _mockEmissionsAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualLocations = _parameters.MultipleLocations.Select(l => l.Name!);
             });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        CollectionAssert.AreEquivalent(expectedLocations, actualLocations);
    }

    [TestCase("--start-time", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run StartTimeOption: long alias")]
    [TestCase("-s", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run StartTimeOption: short alias")]
    public async Task Run_CallsAggregatorWithStartTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias} {optionValue}");
        var expectedStartTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset actualStartTime = default;

        _mockEmissionsAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualStartTime = _parameters.Start;
            });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        Assert.AreEqual(expectedStartTime, actualStartTime);
    }

    [TestCase("--end-time", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run EndTimeOption: long alias")]
    [TestCase("-e", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run EndTimeOption: short alias")]
    public async Task Run_CallsAggregatorWithEndTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias} {optionValue}");
        var expectedEndTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset actualEndTime = default;

        _mockEmissionsAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualEndTime = _parameters.End;
            });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        Assert.AreEqual(expectedEndTime, actualEndTime);
    }

    [TestCase("--best", TestName = "EmissionsCommandTests.Run BestOption: long alias")]
    [TestCase("-b", TestName = "EmissionsCommandTests.Run BestOption: short alias")]
    public async Task Run_CallsAggregatorWithBestOption(string alias)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias}");

        _mockEmissionsAggregator.Setup(agg => agg.GetBestEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ReturnsAsync( new List<EmissionsData>() { new EmissionsData() } ); ;

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsAggregator.Verify(agg => agg.GetBestEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
    }

    [TestCase("--average", 1, TestName = "EmissionsCommandTests.Run AverageOption: long alias, single location")]
    [TestCase("-a", 1, TestName = "EmissionsCommandTests.Run AverageOption: short alias, single location")]
    [TestCase("--average", 2, TestName = "EmissionsCommandTests.Run AverageOption: long alias, multiple locations")]
    public async Task Run_CallsAggregatorWithAverageOption(string alias, int locationCount)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var testLocations = new[] { "-l eastus", "-l westus" };
        string locationArgs = string.Join(" ", testLocations.Take(locationCount));
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {locationArgs} {alias}");

        _mockEmissionsAggregator.Setup(agg => agg.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>()));

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsAggregator.Verify(agg => agg.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>()), Times.Exactly(locationCount));
    }

    [TestCase("--best --average", TestName = "EmissionsCommandTests.Run MutuallyExclusiveOptions: --best --average")]
    [TestCase("-b --average", TestName = "EmissionsCommandTests.Run MutuallyExclusiveOptions: -b --average")]
    [TestCase("--best -a", TestName = "EmissionsCommandTests.Run MutuallyExclusiveOptions: --best -a")]
    [TestCase("-b -a", TestName = "EmissionsCommandTests.Run MutuallyExclusiveOptions: -b -a")]
    public void Run_ValidateMutuallyExclusiveOptionsBestAndAverage(string commandArgs)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var expectedErrorMessage = "Options --average and --best cannot be used together";

        // Act
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions -l eastus {commandArgs}");

        // Assert
        CollectionAssert.Contains(invocationContext.ParseResult.Errors.Select(e => e.Message), expectedErrorMessage);
    }
}