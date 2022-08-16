using CarbonAware.DataSources.ElectricityMap;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.ElectricityMapClient;
using CarbonAware.Tools.ElectricityMapClient.Model;
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

namespace CarbonAware.DataSources.ElectricityMap.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class ElectricityMapDataSourceTests
{
    private Mock<ILogger<ElectricityMapDataSource>> Logger { get; set; }

    private Mock<IElectricityMapClient> ElectricityMapClient { get; set; }

    private ActivitySource ActivitySource { get; set; }

    private ElectricityMapDataSource DataSource { get; set; }

    private Mock<ILocationSource> LocationSource { get; set; }

    // Magic floating point tolerance to allow for minuscule differences in floating point arithmetic.
    private const double FLOATING_POINT_TOLERANCE = 0.00000001;

    // Conversion factors for asserting proper unit conversions
    private const double GRAMS_TO_POUNDS = 0.00220462262185;
    private const double KWH_TO_MWH = 0.001;


    [SetUp]
    public void Setup()
    {
        this.ActivitySource = new ActivitySource("ElectricityMapDataSourceTests");

        this.Logger = new Mock<ILogger<ElectricityMapDataSource>>();
        this.ElectricityMapClient = new Mock<IElectricityMapClient>();
        this.LocationSource = new Mock<ILocationSource>();

        this.DataSource = new ElectricityMapDataSource(this.Logger.Object, this.ElectricityMapClient.Object, this.ActivitySource, this.LocationSource.Object);
    }

    [Test]
    [DefaultFloatingPointTolerance(FLOATING_POINT_TOLERANCE)]
    public async Task GetCarbonIntensity_ReturnsResultsWhenRecordsFound()
    {
        /* TODO
        var location = new Location() { RegionName = "eastus", LocationType = LocationType.CloudProvider, CloudProvider = CloudProvider.Azure };
        var balancingAuthority = new BalancingAuthority() { Abbreviation = "BA" };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));
        var lbsPerMwhEmissions = 10;
        var gPerKwhEmissions = this.DataSource.ConvertMoerToGramsPerKilowattHour(lbsPerMwhEmissions);

        var emissionData = new List<GridEmissionDataPoint>()
        {
            new GridEmissionDataPoint()
            {
                BalancingAuthorityAbbreviation = balancingAuthority.Abbreviation,
                PointTime = startDate.DateTime,
                Value = lbsPerMwhEmissions,
            }
        };

        this.ElectricityMapClient.Setup(w => w.GetDataAsync(
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
        Assert.AreEqual(gPerKwhEmissions, first.Rating);
        Assert.AreEqual(balancingAuthority.Abbreviation, first.Location);
        Assert.AreEqual(startDate.DateTime, first.Time);

        this.LocationSource.Verify(r => r.ToGeopositionLocationAsync(location));
        */
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsEmptyListWhenNoRecordsFound()
    {
        /* TODO
        var location = new Location() { RegionName = "eastus", LocationType = LocationType.CloudProvider, CloudProvider = CloudProvider.Azure };
        var balancingAuthority = new BalancingAuthority() { Abbreviation = "BA" };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));

        this.ElectricityMapClient.Setup(w => w.GetDataAsync(
            balancingAuthority,
            startDate,
            endDate)
        ).ReturnsAsync(() => new List<GridEmissionDataPoint>());

        SetupBalancingAuthority(balancingAuthority, location);

        var result = await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { location }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        */
    }

    /* TODO: Need to implement location conversion.
    [Test]
    public void GetCarbonIntensity_ThrowsWhenRegionNotFound()
    {
        var location = new Location() { RegionName = "eastus", LocationType = LocationType.CloudProvider, CloudProvider = CloudProvider.Azure };
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));

        this.LocationSource.Setup(l => l.ToGeopositionLocationAsync(location)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { location }, startDate, endDate));
    }
    */

    [DatapointSource]
    public float[] moerValues = new float[] { 0.0F, 10.0F, 100.0F, 1000.0F, 596.1367F};

    [Theory]
    public void GetCarbonIntensity_ConvertsMoerToGramsPerKwh(float lbsPerMwhEmissions)
    {
        /* TODO
        Assume.That(lbsPerMwhEmissions >= 0.0);

        var result = this.DataSource.ConvertMoerToGramsPerKilowattHour(lbsPerMwhEmissions);

        Assert.That(result >= 0.0);
        Assert.That(result * GRAMS_TO_POUNDS / KWH_TO_MWH, Is.EqualTo(lbsPerMwhEmissions).Within(FLOATING_POINT_TOLERANCE));
        */
    }

    /* TODO
    private void SetupBalancingAuthority(BalancingAuthority balancingAuthority, Location location)
    {
        this.LocationSource.Setup(r => r.ToGeopositionLocationAsync(location)).Returns(Task.FromResult(location));
        var latitude = location.Latitude.ToString() ?? throw new ArgumentNullException(String.Format("Could not find location"));
        var longitude = location.Longitude.ToString() ?? throw new ArgumentNullException(String.Format("Could not find location"));

        this.ElectricityMapClient.Setup(w => w.GetBalancingAuthorityAsync(latitude, longitude)
        ).ReturnsAsync(() => balancingAuthority);

    }
    */
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
