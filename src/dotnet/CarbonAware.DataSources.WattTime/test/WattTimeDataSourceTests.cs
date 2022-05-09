using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
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

    private Mock<ILocationSource> LocationSource { get; set; }


    [SetUp]
    public void Setup()
    {
        this.ActivitySource = new ActivitySource("WattTimeDataSourceTests");

        this.Logger = new Mock<ILogger<WattTimeDataSource>>();
        this.WattTimeClient = new Mock<IWattTimeClient>();
        this.LocationSource = new Mock<ILocationSource>();

        this.DataSource = new WattTimeDataSource(this.Logger.Object, this.WattTimeClient.Object, this.ActivitySource, this.LocationSource.Object);
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsResultsWhenRecordsFound()
    {
        var location = new Location() { RegionName = "eastus", LocationType = LocationType.CloudProvider, CloudProvider = CloudProvider.Azure };
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

        SetupBalancingAuthority(balancingAuthority, location);
        var result = await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { location }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.AreEqual(5m, first.Rating);
        Assert.AreEqual(balancingAuthority.Abbreviation, first.Location);
        Assert.AreEqual(startDate.DateTime, first.Time);

        this.LocationSource.Verify(r => r.ToGeopositionLocationAsync(location));
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsEmptyListWhenNoRecordsFound()
    {
        var location = new Location() { RegionName = "eastus", LocationType = LocationType.CloudProvider, CloudProvider = CloudProvider.Azure };
        var balancingAuthority = new BalancingAuthority() { Abbreviation = "BA" };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));

        this.WattTimeClient.Setup(w => w.GetDataAsync(
            balancingAuthority,
            startDate,
            endDate)
        ).ReturnsAsync(() => new List<GridEmissionDataPoint>());

        SetupBalancingAuthority(balancingAuthority, location);

        var result = await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { location }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void GetCarbonIntensity_ThrowsWhenRegionNotFound()
    {
        var location = new Location() { RegionName = "eastus", LocationType = LocationType.CloudProvider, CloudProvider = CloudProvider.Azure };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));

        this.LocationSource.Setup(l => l.ToGeopositionLocationAsync(location)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { location }, startDate, endDate));
    }
    private void SetupBalancingAuthority(BalancingAuthority balancingAuthority, Location location)
    {
        this.LocationSource.Setup(r => r.ToGeopositionLocationAsync(location)).Returns(Task.FromResult(location));
        var latitude = location.Latitude.ToString() ?? throw new ArgumentNullException(String.Format("Could not find location"));
        var longitude = location.Longitude.ToString() ?? throw new ArgumentNullException(String.Format("Could not find location"));

        this.WattTimeClient.Setup(w => w.GetBalancingAuthorityAsync(latitude, longitude)
        ).ReturnsAsync(() => balancingAuthority);

    }

}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
