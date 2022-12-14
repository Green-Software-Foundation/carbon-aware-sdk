using CarbonAware.DataSources.Configuration;
using NUnit.Framework;
using System.Text.Json.Nodes;

namespace CarbonAware.CLI.IntegrationTests.Commands.EmissionsForecasts;

/// <summary>
/// Tests that the CLI handles and packages various responses from aggregators 
/// and data sources properly, including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.WattTime)]
[TestFixture(DataSourceType.ElectricityMaps)]
public class EmissionsForecastsCommandTests : IntegrationTestingBase
{
    public EmissionsForecastsCommandTests(DataSourceType dataSource) : base(dataSource) { }

    [Test]
    public async Task EmissionsForecasts_Help_ReturnsHelpText()
    {
        // Arrange
        var expectedAliases = new[]
        {
            "-l", "--location",
            "-s", "--data-start-at",
            "-e", "--data-end-at",
            "-r", "--requested-at",
            "-w", "--window-size"
        };

        // Act
        var exitCode = await InvokeCliAsync("emissions-forecasts -h");
        var output = _console.Out.ToString()!;

        // Assert
        Assert.AreEqual(0, exitCode, _console.Error.ToString());
        foreach (var expectedAlias in expectedAliases)
        {
            StringAssert.Contains(expectedAlias, output);
        }
    }

    [Test]
    public async Task EmissionsForecasts_OnlyRequiredOptions_ReturnsExpectedData()
    {
        // Arrange
        var location = "eastus";

        _dataSourceMocker.SetupForecastMock();

        // Act
        var exitCode = await InvokeCliAsync($"emissions-forecasts -l {location}");
        // Assert
        Assert.AreEqual(0, exitCode, _console.Error.ToString());

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!;
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["GeneratedAt"]);
        Assert.IsNotNull(firstResult["DataStartAt"]);
        Assert.IsNotNull(firstResult["DataEndAt"]);
        Assert.IsNotNull(firstResult["RequestedAt"]);
        Assert.IsNotNull(firstResult["OptimalDataPoints"]);
        Assert.IsNotNull(firstResult["ForecastData"]);
    }

    [Test]
    public async Task EmissionsForecasts_StartAndEndOptions_ReturnsExpectedData()
    {
        // Arrange
        var location = "eastus";
        var start = DateTimeOffset.UtcNow.AddMinutes(10);
        var end =  start.AddHours(5);
        var dataStartAt = start.ToString("yyyy-MM-ddTHH:mm:ss");
        var dataEndAt = end.ToString("yyyy-MM-ddTHH:mm:ss");
       
        _dataSourceMocker.SetupForecastMock();
        // Act
        var exitCode = await InvokeCliAsync($"emissions-forecasts -l {location} -s {dataStartAt} -e {dataEndAt}");

        // Assert
        Assert.AreEqual(0, exitCode, _console.Error.ToString());
     
        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!.AsObject();
        Assert.AreEqual(1, jsonResults.Count);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["GeneratedAt"]);
        Assert.IsNotNull(firstResult["DataStartAt"]);
        Assert.IsNotNull(firstResult["DataEndAt"]);
        Assert.IsNotNull(firstResult["RequestedAt"]);
        Assert.IsNotNull(firstResult["OptimalDataPoints"]);
        Assert.IsNotNull(firstResult["ForecastData"]);
    }

    [Test]
    public async Task EmissionsForecasts_RequestedAtOptions_ReturnsExpectedData()
    {
        IgnoreTestForDataSource("data source does not implement '--requested-at'", DataSourceType.ElectricityMaps);

        // Arrange
        _dataSourceMocker.SetupBatchForecastMock();

        // Act
        var exitCode = await InvokeCliAsync($"emissions-forecasts -l eastus -r 2022-09-01");

        // Assert
        Assert.AreEqual(0, exitCode, _console.Error.ToString());

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!.AsObject();
        Assert.AreEqual(1, jsonResults.Count);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["GeneratedAt"]);
        Assert.IsNotNull(firstResult["DataStartAt"]);
        Assert.IsNotNull(firstResult["DataEndAt"]);
        Assert.IsNotNull(firstResult["RequestedAt"]);
        Assert.IsNotNull(firstResult["OptimalDataPoints"]);
        Assert.IsNotNull(firstResult["ForecastData"]);
    }

    private void IgnoreTestForDataSource(string reasonMessage, params DataSourceType[] ignoredDataSources)
    {
        if (ignoredDataSources.Contains(_dataSource))
        {
            Assert.Ignore($"Ignoring test: {reasonMessage}");
        }
    }
}
