﻿using CarbonAware.DataSources.Configuration;
using NUnit.Framework;
using System.Text.Json;

namespace CarbonAware.CLI.IntegrationTests.Commands.Location;

/// <summary>
/// Tests that the CLI handles responses from location sources.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
[TestFixture(DataSourceType.ElectricityMaps)]
class LocationCommandTests : IntegrationTestingBase
{
    public LocationCommandTests(DataSourceType dataSource) : base(dataSource) { }

    [Test]
    public async Task Locations_Help_ReturnsHelpText()
    {
        var exitCode = await InvokeCliAsync("locations -h");
        Assert.AreEqual(0, exitCode, _console.Error.ToString());
        var output = _console.Out.ToString()!;
        StringAssert.Contains("supported locations", output);
    }

    [Test]
    public async Task Locations_ReturnsKnownData()
    {
        // Act
        var exitCode = await InvokeCliAsync("locations");

        // Assert
        Assert.AreEqual(0, exitCode, _console.Error.ToString());
        var data = JsonSerializer.Deserialize<IDictionary<string, dynamic>>(_console.Out.ToString()!);
        Assert.That(data!.ContainsKey("eastus"), Is.True);
        Assert.That(data!.ContainsKey("northeurope"), Is.True);
    }
}
