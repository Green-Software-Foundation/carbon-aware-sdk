using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources.Exceptions;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarbonAware.DataSources.ElectricityMaps.Tests;

[TestFixture]
public class ElectricityMapsDataSourceTests
{
    private Mock<ILogger<ElectricityMapsDataSource>> _logger { get; set; }

    private Mock<IElectricityMapsClient> _electricityMapsClient { get; set; }

    private ElectricityMapsDataSource _dataSource { get; set; }

    private Mock<ILocationSource> _locationSource { get; set; }
    private static Location _defaultLocation = new Location() { Name = "eastus", Latitude = 34.123m, Longitude = 123.456m };
    private static string _defaultLatitude => _defaultLocation.Latitude.ToString() ?? "";
    private static string _defaultLongitude => _defaultLocation.Longitude.ToString() ?? "";
    private static DateTimeOffset _defaultDataStartTime = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<ElectricityMapsDataSource>>();
        _electricityMapsClient = new Mock<IElectricityMapsClient>();
        _locationSource = new Mock<ILocationSource>();
        _dataSource = new ElectricityMapsDataSource(_logger.Object, _electricityMapsClient.Object, _locationSource.Object);
    }

    [Test]
    public async Task GetCurrentCarbonIntensityForecastAsync_ReturnsResultsWhenRecordsFound()
    {
        // Arrange
        var startDate = _defaultDataStartTime;
        var endDate = startDate.AddMinutes(1);
        var updatedAt = new DateTimeOffset(2022, 4, 18, 12, 30, 00, TimeSpan.FromHours(-6));
        var expectedDuration = TimeSpan.FromMinutes(5);
        var expectedCarbonIntensity = 10;

        var forecast = new ForecastedCarbonIntensityData()
        {
            UpdatedAt = updatedAt,
            ForecastData = new List<Forecast>()
            {
                new Forecast()
                {
                    CarbonIntensity = expectedCarbonIntensity,
                    DateTime = startDate
                },
                new Forecast()
                {
                    CarbonIntensity = expectedCarbonIntensity,
                    DateTime = startDate + expectedDuration
                },
            }
        };

        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));
        _electricityMapsClient.Setup(client => client.GetForecastedCarbonIntensityAsync(_defaultLatitude, _defaultLongitude)
            ).ReturnsAsync(() => forecast);

        // Act
        var result = await _dataSource.GetCurrentCarbonIntensityForecastAsync(_defaultLocation);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.GeneratedAt, Is.EqualTo(updatedAt));
        Assert.That(result.Location, Is.EqualTo(_defaultLocation));

        var firstDataPoint = result.ForecastData.First();
        var lastDataPoint = result.ForecastData.Last();
        Assert.IsNotNull(firstDataPoint);
        Assert.That(firstDataPoint.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(firstDataPoint.Location, Is.EqualTo(_defaultLocation.Name));
        Assert.That(firstDataPoint.Time, Is.EqualTo(startDate));
        Assert.That(firstDataPoint.Duration, Is.EqualTo(expectedDuration));

        Assert.IsNotNull(lastDataPoint);
        Assert.That(lastDataPoint.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(lastDataPoint.Location, Is.EqualTo(_defaultLocation.Name));
        Assert.That(lastDataPoint.Time, Is.EqualTo(startDate + expectedDuration));
        Assert.That(lastDataPoint.Duration, Is.EqualTo(expectedDuration));

        _locationSource.Verify(r => r.ToGeopositionLocationAsync(_defaultLocation), Times.Once);
    }

    [Test]
    public void GetCurrentCarbonIntensityForecastAsync_ThrowsWhenRegionNotFound()
    {
        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await _dataSource.GetCurrentCarbonIntensityForecastAsync(_defaultLocation));
    }
}