using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources.Exceptions;
using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.WattTime.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[TestFixture]
public class WattTimeDataSourceTests
{
    private Mock<ILogger<WattTimeDataSource>> Logger { get; set; }

    private Mock<IWattTimeClient> WattTimeClient { get; set; }

    private WattTimeDataSource DataSource { get; set; }

    private Mock<ILocationSource> LocationSource { get; set; }
    private Location DefaultLocation { get; set; }
    private BalancingAuthority DefaultBalancingAuthority { get; set; }
    private DateTimeOffset DefaultDataStartTime { get; set; }

    // Magic floating point tolerance to allow for minuscule differences in floating point arithmetic.
    private const double FLOATING_POINT_TOLERANCE = 0.00000001;

    // Conversion factors for asserting proper unit conversions
    private const double GRAMS_TO_POUNDS = 0.00220462262185;
    private const double KWH_TO_MWH = 0.001;


    [SetUp]
    public void Setup()
    {
        this.Logger = new Mock<ILogger<WattTimeDataSource>>();
        this.WattTimeClient = new Mock<IWattTimeClient>();
        this.LocationSource = new Mock<ILocationSource>();

        this.DataSource = new WattTimeDataSource(this.Logger.Object, this.WattTimeClient.Object, this.LocationSource.Object);

        this.DefaultLocation = new Location() { Name = "eastus" };
        this.DefaultBalancingAuthority = new BalancingAuthority() { Abbreviation = "BA" };
        this.DefaultDataStartTime = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        MockBalancingAuthorityLocationMapping();
    }

    [Test]
    [DefaultFloatingPointTolerance(FLOATING_POINT_TOLERANCE)]
    public async Task GetCarbonIntensity_ReturnsResultsWhenRecordsFound()
    {
        var startDate = this.DefaultDataStartTime;
        var endDate = startDate.AddMinutes(1);
        var lbsPerMwhEmissions = 10;
        var gPerKwhEmissions = this.DataSource.ConvertMoerToGramsPerKilowattHour(lbsPerMwhEmissions);

        var emissionData = GenerateDataPoints(1, value: lbsPerMwhEmissions);

        this.WattTimeClient.Setup(w => w.GetDataAsync(
            this.DefaultBalancingAuthority,
            It.IsAny<DateTimeOffset>(),
            It.IsAny<DateTimeOffset>())
        ).ReturnsAsync(() => emissionData);

        var result = await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { this.DefaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.AreEqual(gPerKwhEmissions, first.Rating);
        Assert.AreEqual(this.DefaultBalancingAuthority.Abbreviation, first.Location);
        Assert.AreEqual(startDate, first.Time);

        this.LocationSource.Verify(r => r.ToGeopositionLocationAsync(this.DefaultLocation));
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsEmptyListWhenNoRecordsFound()
    {
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = startDate.AddMinutes(1);

        this.WattTimeClient.Setup(w => w.GetDataAsync(
            this.DefaultBalancingAuthority,
            startDate,
            endDate)
        ).ReturnsAsync(() => new List<GridEmissionDataPoint>());

        var result = await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { this.DefaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void GetCarbonIntensity_ThrowsWhenRegionNotFound()
    {
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = startDate.AddMinutes(1);

        this.LocationSource.Setup(l => l.ToGeopositionLocationAsync(this.DefaultLocation)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { this.DefaultLocation }, startDate, endDate));
    }

    [TestCase(true, TestName = "Getting current forecast")]
    [TestCase(false, TestName = "Getting forecast on date")]
    public async Task GetCarbonIntensityForecastAsync_ReturnsResultsWhenRecordsFound(bool getCurrentForecast)
    {
        // Arrange
        var startDate = this.DefaultDataStartTime;
        var endDate = startDate.AddMinutes(1);
        var generatedAt = new DateTimeOffset(2022, 4, 18, 12, 30, 00, TimeSpan.FromHours(-6));
        var lbsPerMwhEmissions = 10;
        var gPerKwhEmissions = this.DataSource.ConvertMoerToGramsPerKilowattHour(lbsPerMwhEmissions);
        var expectedDuration = TimeSpan.FromMinutes(5);

        var emissionData = GenerateDataPoints(2, value: lbsPerMwhEmissions);
        var forecast = new Forecast()
        {
            GeneratedAt = generatedAt,
            ForecastData = emissionData
        };

        EmissionsForecast result;

        if (getCurrentForecast)
        {
            this.WattTimeClient.Setup(w => w.GetCurrentForecastAsync(this.DefaultBalancingAuthority)
                ).ReturnsAsync(() => forecast);

            // Act
            result = await this.DataSource.GetCurrentCarbonIntensityForecastAsync(this.DefaultLocation);
        }
        else
        {
            this.WattTimeClient.Setup(w => w.GetForecastOnDateAsync(this.DefaultBalancingAuthority, generatedAt)
                ).ReturnsAsync(() => forecast);

            // Act
            result = await this.DataSource.GetCarbonIntensityForecastAsync(this.DefaultLocation, generatedAt);
        }

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(generatedAt, result.GeneratedAt);
        Assert.AreEqual(this.DefaultLocation, result.Location);

        var firstDataPoint = result.ForecastData.First();
        var lastDataPoint = result.ForecastData.Last();
        Assert.IsNotNull(firstDataPoint);
        Assert.AreEqual(gPerKwhEmissions, firstDataPoint.Rating);
        Assert.AreEqual(this.DefaultBalancingAuthority.Abbreviation, firstDataPoint.Location);
        Assert.AreEqual(startDate, firstDataPoint.Time);
        Assert.AreEqual(expectedDuration, firstDataPoint.Duration);

        Assert.IsNotNull(lastDataPoint);
        Assert.AreEqual(gPerKwhEmissions, lastDataPoint.Rating);
        Assert.AreEqual(this.DefaultBalancingAuthority.Abbreviation, lastDataPoint.Location);
        Assert.AreEqual(startDate + expectedDuration, lastDataPoint.Time);
        Assert.AreEqual(expectedDuration, lastDataPoint.Duration);

        this.LocationSource.Verify(r => r.ToGeopositionLocationAsync(this.DefaultLocation));
    }

    [Test]
    public void GetCarbonIntensityForecastAsync_ThrowsWhenRegionNotFound()
    {
        this.LocationSource.Setup(l => l.ToGeopositionLocationAsync(this.DefaultLocation)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await this.DataSource.GetCurrentCarbonIntensityForecastAsync(this.DefaultLocation));
        Assert.ThrowsAsync<LocationConversionException>(async () => await this.DataSource.GetCarbonIntensityForecastAsync(this.DefaultLocation, new DateTimeOffset()));
    }

    [Test]
    public void GetCarbonIntensityForecastAsync_ThrowsWhenNoForecastFoundForReuqestedTime()
    {
        var generatedAt = new DateTimeOffset();

        this.WattTimeClient.Setup(w => w.GetForecastOnDateAsync(this.DefaultBalancingAuthority, generatedAt)).Returns(Task.FromResult<Forecast?>(null));

        // The datasource throws an exception if no forecasts are found at the requested generatedAt time.  
        Assert.ThrowsAsync<ArgumentException>(async () => await this.DataSource.GetCarbonIntensityForecastAsync(this.DefaultLocation, generatedAt));
    }

    [TestCase(0, TestName = "GetCurrentCarbonIntensityForecastAsync throws for: No datapoints")]
    [TestCase(1, TestName = "GetCurrentCarbonIntensityForecastAsync throws for: 1 datapoint")]
    public void GetCurrentCarbonIntensityForecastAsync_ThrowsWhenTooFewDatapointsReturned(int numDataPoints)
    {
        // Arrange
        var emissionData = GenerateDataPoints(numDataPoints);

        var forecast = new Forecast()
        {
            GeneratedAt = DateTimeOffset.Now,
            ForecastData = emissionData
        };

        this.WattTimeClient.Setup(w => w.GetCurrentForecastAsync(this.DefaultBalancingAuthority)
            ).ReturnsAsync(() => forecast);

        Assert.ThrowsAsync<WattTimeClientException>(async () => await this.DataSource.GetCurrentCarbonIntensityForecastAsync(this.DefaultLocation));
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:00:00Z", TestName = "GetCarbonIntensityForecastAsync returns expected value 2022-01-01T00:00:00Z round to floor")]
    [TestCase("2021-05-01T00:03:20Z", "2021-05-01T00:00:00Z", TestName = "GetCarbonIntensityForecastAsync returns expected value 2021-05-01T00:00:00Z round to floor")]
    [TestCase("2022-01-01T00:07:00Z", "2022-01-01T00:05:00Z", TestName = "GetCarbonIntensityForecastAsync returns expected value 2022-01-01T00:05:00Z round to floor")]
    public async Task GetCarbonIntensityForecastAsync_RequiredAtRounded(string requested, string expected)
    {
        // Arrange
        var requestedAt = DateTimeOffset.Parse(requested);
        var expectedAt = DateTimeOffset.Parse(expected);

        var emissionData = GenerateDataPoints(2, startTime: requestedAt);
        var forecast = new Forecast()
        {
            GeneratedAt = expectedAt,
            ForecastData = emissionData
        };

        this.WattTimeClient.Setup(w => w.GetForecastOnDateAsync(this.DefaultBalancingAuthority, expectedAt)
                ).ReturnsAsync(() => forecast);

        // Act
        var result = await this.DataSource.GetCarbonIntensityForecastAsync(this.DefaultLocation, requestedAt);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(requestedAt, result.RequestedAt);
        this.WattTimeClient.Verify(w => w.GetForecastOnDateAsync(
            It.IsAny<BalancingAuthority>(), It.Is<DateTimeOffset>(date => date.Equals(expectedAt))), Times.Once);
    }

    [DatapointSource]
    public float[] moerValues = new float[] { 0.0F, 10.0F, 100.0F, 1000.0F, 596.1367F};

    [Theory]
    public void GetCarbonIntensity_ConvertsMoerToGramsPerKwh(float lbsPerMwhEmissions)
    {
        Assume.That(lbsPerMwhEmissions >= 0.0);

        var result = this.DataSource.ConvertMoerToGramsPerKilowattHour(lbsPerMwhEmissions);

        Assert.That(result >= 0.0);
        Assert.That(result * GRAMS_TO_POUNDS / KWH_TO_MWH, Is.EqualTo(lbsPerMwhEmissions).Within(FLOATING_POINT_TOLERANCE));
    }

    /// <summary>
    /// Tests that if 'frequency' is not provided in the WattTime response of emission data, it is calculated from the first 2 data points, or defaulted to 0 if fewer than 2 data points are returned 
    /// </summary>
    [TestCase(new double[] { 300, 300 }, 300, null, TestName = "GetCarbonIntensity - for multiple data points, frequency is null for one data point ")]
    [TestCase(new double[] { }, null, TestName = "GetCarbonIntensity - for less than 2 data points, frequency is null for one data point ")]
    [TestCase(new double[] { 300, 300 }, null, null, TestName = "GetCarbonIntensity - for multiple data points, frequency is null for all data points")]
    [TestCase(new double[] { 500 }, 500, TestName = "GetCarbonIntensity - frequency is not null")]
    [TestCase(new double[] { }, TestName = "GetCarbonIntensity - for zero data points, returns empty enumerable")]
    public async Task GetCarbonIntensity_CalculatesDurationBasedOnFrequency(double[] durationValues, params int?[] frequencyValues)
    {
        // Arrange
        var startDate = this.DefaultDataStartTime;
        var endDate = startDate.AddMinutes(10);
        var emissionData = GenerateDataPoints(frequencyValues.Length);
        for( int i = 0; i < frequencyValues.Length; i++)
        {
            emissionData[i].Frequency = frequencyValues[i];
        }
        
        List<double> expectedDurationList = durationValues.ToList<double>();

        this.WattTimeClient.Setup(w => w.GetDataAsync(
            It.IsAny<BalancingAuthority>(),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<DateTimeOffset>())
        ).ReturnsAsync(() => emissionData);

        // Act
        var result = await this.DataSource.GetCarbonIntensityAsync(new List<Location>() { this.DefaultLocation }, startDate, endDate);

        // Assert
        List<double> actualDurationList = result.Select(e => e.Duration.TotalSeconds).ToList();
        
        CollectionAssert.AreEqual(expectedDurationList, actualDurationList);
    }

    private void MockBalancingAuthorityLocationMapping()
    {
        this.LocationSource.Setup(r => r.ToGeopositionLocationAsync(this.DefaultLocation)).Returns(Task.FromResult(this.DefaultLocation));
        var latitude = this.DefaultLocation.Latitude.ToString();
        var longitude = this.DefaultLocation.Longitude.ToString();

        this.WattTimeClient.Setup(w => w.GetBalancingAuthorityAsync(latitude!, longitude!)
        ).ReturnsAsync(() => this.DefaultBalancingAuthority);
    }

    private List<GridEmissionDataPoint> GenerateDataPoints(int numberOfDatapoints, float value = 10, DateTimeOffset startTime = default)
    {
        var defaultFrequency = 300;
        var dataPoints = new List<GridEmissionDataPoint>();
        var pointTime = startTime == default ? this.DefaultDataStartTime : startTime;
        for (int i = 0; i < numberOfDatapoints; i++)
        {
            var dataPoint = new GridEmissionDataPoint()
            {
                BalancingAuthorityAbbreviation = this.DefaultBalancingAuthority.Abbreviation,
                PointTime = pointTime,
                Value = value,
                Frequency = defaultFrequency
            };
            dataPoints.Add(dataPoint);
            pointTime += TimeSpan.FromSeconds(defaultFrequency);
        };

        return dataPoints;
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
