using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.ElectricityMapClient;
using CarbonAware.Tools.ElectricityMapClient.Model;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        this.Logger = new Mock<ILogger<ElectricityMapDataSource>>();
        this.ElectricityMapClient = new Mock<IElectricityMapClient>();
        this.LocationSource = new Mock<ILocationSource>();

        this.DataSource = new ElectricityMapDataSource(this.Logger.Object, this.ElectricityMapClient.Object, this.LocationSource.Object);
    }

    [Test]
    public async Task GetCurrentCarbonIntensityForecastAsync_ReturnsResultsWhenRecordsFound()
    {
        // Need converter for electricity map from cloud, local region to electricity map region
        var location = new Location() { RegionName = "AUS-NSW", LocationType = LocationType.CloudProvider, CloudProvider = CloudProvider.Azure };
        var zone = new Zone() { countryCode = "AUS-NSW" };
        var generatedAt = new DateTimeOffset(2022, 4, 18, 12, 30, 00, TimeSpan.FromHours(-6));
        var data = new Data()
        {
            Datetime = new DateTimeOffset(2022, 8, 16, 3, 0, 0, new TimeSpan(0, 0, 0)),
            CarbonIntensity = 300,
            FossilFuelPercentage = 70.59F
        };

        var emissionData = new List<GridEmissionDataPoint>();
        var emission_data = new GridEmissionDataPoint()
        {
            Status = "ok",
            CountryCodeAbbreviation = zone.countryCode,
            Data = data
        };
        emissionData.Add(emission_data);

        var forecast = new Forecast()
        {
            GeneratedAt = generatedAt,
            ForecastData = emissionData
        };

        this.ElectricityMapClient.Setup(w => w.GetCurrentForecastAsync(zone.countryCode)
            ).ReturnsAsync(() => forecast);

        // Act
        var result = await this.DataSource.GetCurrentCarbonIntensityForecastAsync(location);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(generatedAt, result.GeneratedAt);
        Assert.AreEqual(location, result.Location);

        var firstDataPoint = result.ForecastData.First();
        var lastDataPoint = result.ForecastData.Last();
        Assert.IsNotNull(firstDataPoint);
        Assert.AreEqual(data.CarbonIntensity, firstDataPoint.Rating);
        Assert.AreEqual(zone.countryCode, firstDataPoint.Location);

        Assert.IsNotNull(lastDataPoint);
        Assert.AreEqual(data.CarbonIntensity, lastDataPoint.Rating);
        Assert.AreEqual(zone.countryCode, lastDataPoint.Location);
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
