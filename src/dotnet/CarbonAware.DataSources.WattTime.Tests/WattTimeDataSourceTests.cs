using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Model;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.WattTime.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class WattTimeDataSourceTests
{
    private Mock<ILogger<WattTimeDataSource>> Logger { get; set; }

    private Mock<IWattTimeClient> WattTimeClient { get; set; }

    private ActivitySource ActivitySource { get; set; }

    private WattTimeDataSource DataSource { get; set; }

    private Mock<ILocationConverter> LocationConverter { get; set; }


    [SetUp]
    public void Setup()
    {
        this.ActivitySource = new ActivitySource("WattTimeDataSourceTests");

        this.Logger = new Mock<ILogger<WattTimeDataSource>>();
        this.WattTimeClient = new Mock<IWattTimeClient>();
        this.LocationConverter = new Mock<ILocationConverter>();

        this.DataSource = new WattTimeDataSource(this.Logger.Object, this.WattTimeClient.Object, this.ActivitySource, this.LocationConverter.Object);
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsResultsWhenRecordsFound()
    {
        var location = new Location() { RegionName = "us-east", LocationType = LocationType.Azure };
        var balancingAuthority = new BalancingAuthority() { Abbreviation = "BA" };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));

        var emissionData = new List<GridEmissionDataPoint>()
        {
            new GridEmissionDataPoint()
            {
                BalancingAuthorityAbbreviation = balancingAuthority.Abbreviation,
                PointTime = startDate.DateTime,
                Value = 5,
            }
        };

        this.WattTimeClient.Setup(w => w.GetDataAsync(
            balancingAuthority,
            startDate,
            endDate)
        ).ReturnsAsync(() => emissionData);

        this.LocationConverter.Setup(r => r.ConvertLocationToBalancingAuthorityAsync(location)).ReturnsAsync(balancingAuthority);

        var result = await this.DataSource.GetCarbonIntensityAsync(location, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.AreEqual(5m, first.Rating);
        Assert.AreEqual(balancingAuthority.Abbreviation, first.Location);
        Assert.AreEqual(startDate.DateTime, first.Time);

        this.LocationConverter.Verify(r => r.ConvertLocationToBalancingAuthorityAsync(location));
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsEmptyListWhenNoRecordsFound()
    {
        var location = new Location() { RegionName = "us-east", LocationType = LocationType.Azure };
        var balancingAuthority = new BalancingAuthority() { Abbreviation = "BA" };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));

        this.WattTimeClient.Setup(w => w.GetDataAsync(
            balancingAuthority,
            startDate,
            endDate)
        ).ReturnsAsync(() => new List<GridEmissionDataPoint>());

        this.LocationConverter.Setup(r => r.ConvertLocationToBalancingAuthorityAsync(location)).ReturnsAsync(balancingAuthority);


        var result = await this.DataSource.GetCarbonIntensityAsync(location, startDate, endDate);


        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void GetCarbonIntensity_ThrowsWhenRegionNotFound()
    {
        var location = new Location() { RegionName = "us-east", LocationType = LocationType.Azure };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));

        this.LocationConverter.Setup(l => l.ConvertLocationToBalancingAuthorityAsync(location)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await this.DataSource.GetCarbonIntensityAsync(location, startDate, endDate));
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
