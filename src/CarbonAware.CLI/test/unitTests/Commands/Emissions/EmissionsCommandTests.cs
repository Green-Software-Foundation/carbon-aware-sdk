using CarbonAware.CLI.Commands.Emissions;
using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Globalization;

namespace CarbonAware.CLI.UnitTests;

[TestFixture]
class EmissionsCommandTests : TestBase
{
    [Test]
    public async Task Run_CallsHandlerAndWritesResults()
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
      
        _mockEmissionsHandler.Setup(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
            .ReturnsAsync(new List<EmissionsData>() { expectedEmissions });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        string? consoleOutput = _console.Out.ToString();
        StringAssert.Contains(expectedEmissions.Location, consoleOutput);
        StringAssert.Contains(expectedEmissions.Rating.ToString(CultureInfo.InvariantCulture), consoleOutput);
        StringAssert.Contains(expectedEmissions.Time.ToString("yyyy-MM-ddTHH:mm:sszzz"), consoleOutput);
        StringAssert.Contains(expectedEmissions.Duration.ToString(), consoleOutput);
       

        _mockEmissionsHandler.Verify(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()), Times.Once);
    }

    [Test]
    public async Task Run_CallsHandlerWithLocationOptions()
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var longAliasLocation = "eastus";
        var shortAliasLocation = "westus";
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions --location {longAliasLocation} -l {shortAliasLocation}");
        var expectedLocations = new List<string>() { longAliasLocation, shortAliasLocation };
        IEnumerable<string> actualLocations = Array.Empty<string>();

        _mockEmissionsHandler.Setup(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
            .Callback((string[] locations, DateTimeOffset? start, DateTimeOffset? end) => {
                actualLocations = locations;
             });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsHandler.Verify(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()), Times.Once);
        CollectionAssert.AreEquivalent(expectedLocations, actualLocations);
    }

    [TestCase("--start-time", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run StartTimeOption: long alias")]
    [TestCase("-s", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run StartTimeOption: short alias")]
    public async Task Run_CallsHandlerWithStartTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias} {optionValue}");
        var expectedStartTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset? actualStartTime = default;

        _mockEmissionsHandler.Setup(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
            .Callback((string[] locations, DateTimeOffset? start, DateTimeOffset? end) => {
                actualStartTime = start;
            });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsHandler.Verify(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()), Times.Once);
        Assert.AreEqual(expectedStartTime, actualStartTime);
    }

    [TestCase("--end-time", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run EndTimeOption: long alias")]
    [TestCase("-e", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run EndTimeOption: short alias")]
    public async Task Run_CallsHandlerWithEndTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias} {optionValue}");
        var expectedEndTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset? actualEndTime = default;

        _mockEmissionsHandler.Setup(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
            .Callback((string[] locations, DateTimeOffset? start, DateTimeOffset? end) => {
                actualEndTime = end;
            });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsHandler.Verify(handler => handler.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()), Times.Once);
        Assert.AreEqual(expectedEndTime, actualEndTime);
    }

    [TestCase("--best", TestName = "EmissionsCommandTests.Run BestOption: long alias")]
    [TestCase("-b", TestName = "EmissionsCommandTests.Run BestOption: short alias")]
    public async Task Run_CallsHandlerWithBestOption(string alias)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias}");

        _mockEmissionsHandler.Setup(handler => handler.GetBestEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
            .ReturnsAsync( new List<EmissionsData>() { new EmissionsData() } ); ;

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsHandler.Verify(handler => handler.GetBestEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()), Times.Once);
    }

    [TestCase("--average", 1, TestName = "EmissionsCommandTests.Run AverageOption: long alias, single location")]
    [TestCase("-a", 1, TestName = "EmissionsCommandTests.Run AverageOption: short alias, single location")]
    [TestCase("--average", 2, TestName = "EmissionsCommandTests.Run AverageOption: long alias, multiple locations")]
    public async Task Run_CallsHandlerWithAverageOption(string alias, int locationCount)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var testLocations = new[] { "-l eastus", "-l westus" };
        string locationArgs = string.Join(" ", testLocations.Take(locationCount));
        string startEndArgs = $"--start-time {DateTimeOffset.Now} --end-time {DateTimeOffset.Now.AddHours(1)}";
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {locationArgs} {startEndArgs} {alias}");

        _mockEmissionsHandler.Setup(handler => handler.GetAverageCarbonIntensityAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()));

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockEmissionsHandler.Verify(handler => handler.GetAverageCarbonIntensityAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(locationCount));
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