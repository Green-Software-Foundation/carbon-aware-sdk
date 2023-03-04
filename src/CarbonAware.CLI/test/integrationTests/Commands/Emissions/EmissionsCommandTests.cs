using CarbonAware.DataSources.Configuration;
using NUnit.Framework;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CarbonAware.CLI.IntegrationTests.Commands.Emissions;

/// <summary>
/// Tests that the CLI handles and packages various responses from handlers 
/// and data sources properly, including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
public class EmissionsCommandTests : IntegrationTestingBase
{
    public EmissionsCommandTests(DataSourceType dataSource) : base(dataSource) { }

    [Test]
    public async Task Emissions_Help_ReturnsHelpText()
    {
        // Arrange
        var expectedAliases = new[]
        {
            "-l", "--location",
            "-s", "--start-time",
            "-e", "--end-time",
        };

        // Act
        var exitCode = await InvokeCliAsync("emissions -h");
        var output = _console.Out.ToString()!;

        // Assert
        Assert.AreEqual(0, exitCode);
        foreach (var expectedAlias in expectedAliases)
        {
            StringAssert.Contains(expectedAlias, output);
        }
    }

    [Test]
    public async Task Emissions_OnlyRequiredOptions_ReturnsExpectedData()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddMinutes(-30);
        var end = start.AddHours(1);
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);

        // Act
        var exitCode = await InvokeCliAsync($"emissions -l {location}");

        // Assert
        Assert.AreEqual(0, exitCode);

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!;
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["Time"]);
        Assert.IsNotNull(firstResult["Rating"]);
        Assert.IsNotNull(firstResult["Duration"]);
    }

    [Test]
    public async Task Emissions_StartAndEndOptions_ReturnsExpectedData()
    {
        // Arrange
        var start = DateTimeOffset.Parse("2022-09-01T00:00:00Z");
        var end = DateTimeOffset.Parse("2022-09-01T03:00:00Z");
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);

        // Act
        var exitCode = await InvokeCliAsync($"emissions -l {location} -s 2022-09-01T02:01:00Z -e 2022-09-01T02:04:00Z");

        // Assert
        Assert.AreEqual(0, exitCode);

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!.AsObject();
        Assert.AreEqual(1, jsonResults.Count);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["Time"]);
        Assert.IsNotNull(firstResult["Rating"]);
        Assert.IsNotNull(firstResult["Duration"]);
    }

    [Test]
    public async Task Emissions_InvalidStartAndEnd_ReturnsExpectedError()
    {
        // Arrange
        var start = DateTimeOffset.Parse("2022-09-01T00:00:00Z");
        var end = DateTimeOffset.Parse("2022-09-01T03:00:00Z");
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);

        string expectedError = "Invalid parameters Start: Start must be before End ";
        // Act
        var exitCode = await InvokeCliAsync($"emissions -l {location} -s 2022-09-01T02:05:00Z -e 2022-09-01T01:00:00Z");
        // Whitespace characters regex 
        var regex = @"\s+";
        var output = Regex.Replace(_console.Error.ToString()!, regex, " ");

        // Assert
        Assert.AreEqual(2, exitCode);
        Assert.AreEqual(expectedError, output);
    }

    [Test]
    public async Task Emissions_BestOption_ReturnsExpectedData()
    {
        // Arrange
        var start = DateTimeOffset.Parse("2022-09-01T00:00:00Z");
        var end = DateTimeOffset.Parse("2022-09-01T03:00:00Z");
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);

        // Act
        var exitCode = await InvokeCliAsync($"emissions -l {location} -s 2022-09-01T02:01:00Z -e 2022-09-01T02:04:00Z -b");

        // Assert
        Assert.AreEqual(0, exitCode);

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!.AsObject();
        Assert.AreEqual(1, jsonResults.Count);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["Time"]);
        Assert.IsNotNull(firstResult["Rating"]);
        Assert.IsNotNull(firstResult["Duration"]);
    }

    [Test]
    public async Task Emissions_AverageOption_ReturnsExpectedData()
    {
        // Arrange
        var start = DateTimeOffset.Parse("2022-09-01T00:00:00Z");
        var end = DateTimeOffset.Parse("2022-09-01T03:00:00Z");
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);

        // Act
        var exitCode = await InvokeCliAsync($"emissions -l {location} -s 2022-09-01T02:01:00Z -e 2022-09-01T02:04:00Z -a");

        // Assert
        Assert.AreEqual(0, exitCode);

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!.AsObject();
        Assert.AreEqual(1, jsonResults.Count);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["Time"]);
        Assert.IsNotNull(firstResult["Rating"]);
        Assert.IsNotNull(firstResult["Duration"]);
    }

    [Test]
    public async Task Average_Best_ReturnsExpectedError()
    {
        // Arrange
        var start = DateTimeOffset.Parse("2022-09-01T00:00:00Z");
        var end = DateTimeOffset.Parse("2022-09-01T03:00:00Z");
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);

        // Act
        var exitCode = await InvokeCliAsync($"emissions -l {location} -s 2022-09-01T02:01:00Z -e 2022-09-01T02:04:00Z -a -best");
        // Assert
        Assert.AreEqual(1, exitCode);
        var expectedError = "Options --average and --best cannot be used together Option '-s' expects a single argument but 2 were provided. ";
        // Whitespace characters regex 
        var regex = @"\s+";
        var output = Regex.Replace(_console.Error.ToString()!, regex, " ");

        Assert.AreEqual(expectedError, output);
    }
}
