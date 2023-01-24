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

    [TestCase(8, 1, 1, 0, TestName = "GetCarbonIntensity calls GetRecentCarbonIntensityDataAsync method when date within 24 hours")]
    [TestCase(36, 1, 0, 1, TestName = "GetCarbonIntensity calls GetPastRangeDataAsync method when date outside of 24 hours")]
    [TestCase(30, 12, 0, 1, TestName = "GetCarbonIntensity calls GetPastRangeDataAsync method when start date outside of 24 hours but enddate within 24 hours")]
    [TestCase(8, 12, 0, 1, TestName = "GetCarbonIntensity calls GetRecentCarbonIntensityDataAsync method when start date within 24 hours but enddate outside of 24 hours")]
    public async Task GetCarbonIntensity_CallsExpectedClientEndpoint(int startTimeOffset, int endTimeOffset, int expectedHistoryCalls, int expectedPastRangeCalls)
    {
        var now = DateTimeOffset.UtcNow;
        var startDate = now.AddHours(-startTimeOffset);
        var endDate = startDate.AddHours(endTimeOffset);
        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        HistoryCarbonIntensityData emissionData = new();

        this._electricityMapsClient.Setup(c => c.GetRecentCarbonIntensityHistoryAsync(
            _defaultLatitude,
            _defaultLongitude)
        ).ReturnsAsync(() => emissionData);

        PastRangeData pastRange = new();
        this._electricityMapsClient.Setup(c => c.GetPastRangeDataAsync(
            _defaultLatitude,
            _defaultLongitude,
            startDate,
            endDate)
        ).ReturnsAsync(() => pastRange);

        await _dataSource.GetCarbonIntensityAsync(_defaultLocation, startDate, endDate);

        _electricityMapsClient.Verify(c => c.GetPastRangeDataAsync(_defaultLatitude, _defaultLongitude, startDate, endDate), Times.Exactly(expectedPastRangeCalls));

        _electricityMapsClient.Verify(c => c.GetRecentCarbonIntensityHistoryAsync(_defaultLatitude, _defaultLongitude), Times.Exactly(expectedHistoryCalls));
    }

    [Test]
    public async Task GetCarbonIntensity_DateRangeWithin24Hours_ReturnsResultsWhenRecordsFound()
    {
        var startDate = DateTimeOffset.UtcNow.AddHours(-10);
        var endDate = startDate.AddHours(1);
        var expectedCarbonIntensity = 100;

        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        HistoryCarbonIntensityData emissionData = new()
        {
            HistoryData = new List<CarbonIntensity>()
            {
                new CarbonIntensity()
                {
                    Value = expectedCarbonIntensity,
                },
                new CarbonIntensity()
                {
                    Value = expectedCarbonIntensity,
                }
            }
        };

        this._electricityMapsClient.Setup(c => c.GetRecentCarbonIntensityHistoryAsync(
            _defaultLatitude,
            _defaultLongitude)
        ).ReturnsAsync(() => emissionData);

        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(2));

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.That(first.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(first.Location, Is.EqualTo(_defaultLocation.Name));

        this._locationSource.Verify(l => l.ToGeopositionLocationAsync(_defaultLocation));
    }

    [Test]
    public async Task GetCarbonIntensity_DateRangeMore24Hours_ReturnsResultsWhenRecordsFound()
    {
        var startDate = _defaultDataStartTime;
        var endDate = startDate.AddHours(1);
        var expectedCarbonIntensity = 100;

        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        PastRangeData emissionData = new()
        {
            HistoryData = new List<CarbonIntensity>()
            {
                new CarbonIntensity()
                {
                    Value = expectedCarbonIntensity,
                },
                new CarbonIntensity()
                {
                    Value = expectedCarbonIntensity,
                }
            }
        };

        this._electricityMapsClient.Setup(c => c.GetPastRangeDataAsync(
            _defaultLatitude,
            _defaultLongitude,
            startDate,
            endDate)
        ).ReturnsAsync(() => emissionData);

        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(2));

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.That(first.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(first.Location, Is.EqualTo(_defaultLocation.Name));

        this._locationSource.Verify(l => l.ToGeopositionLocationAsync(_defaultLocation));
    }

    [Test]
    public async Task GetCarbonIntensity_PastRange_ReturnsEmptyListWhenNoRecordsFound()
    {
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = startDate.AddHours(1);

        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        this._electricityMapsClient.Setup(c => c.GetPastRangeDataAsync(
            _defaultLatitude,
            _defaultLongitude,
            startDate,

            endDate)
        ).ReturnsAsync(() => new PastRangeData());

        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public void GetCarbonIntensity_ThrowsWhenRegionNotFound()
    {
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = startDate.AddMinutes(1);

        this._locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate));
    }

    [Test]
    public async Task GetDurationBetweenHistoryDataPoints_ReturnsDefaultDuration_WhenOneDatapointReturned()
    {
        var startDate = DateTimeOffset.UtcNow.AddHours(-8);
        var endDate = startDate.AddHours(1);
        // Arrange
        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        HistoryCarbonIntensityData emissionData = new()
        {
            HistoryData = new List<CarbonIntensity>()
            {
                new CarbonIntensity()
            }
        };

        this._electricityMapsClient.Setup(c => c.GetRecentCarbonIntensityHistoryAsync(
            _defaultLatitude,
            _defaultLongitude)
        ).ReturnsAsync(() => emissionData);


        // Act & Assert
        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.That(result.Count(), Is.EqualTo(1));
        var first = result.First();
        Assert.IsNotNull(first);
        Assert.That(first.Duration, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public async Task GetDurationBetweenHistoryDataPoints_WhenMultipleDataPoints_ReturnsExpectedDuration()
    {
        var startDate = DateTimeOffset.UtcNow.AddHours(-8);
        var endDate = startDate.AddHours(1);
        var expectedDuration = TimeSpan.FromHours(1);
        // Arrange
        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        HistoryCarbonIntensityData emissionData = new()
        {
            HistoryData = new List<CarbonIntensity>()
            {
                new CarbonIntensity()
                {
                    DateTime = startDate,

                },
                new CarbonIntensity()
                {
                    DateTime= startDate + expectedDuration,
                }
            }
        };

        this._electricityMapsClient.Setup(c => c.GetRecentCarbonIntensityHistoryAsync(
            _defaultLatitude,
            _defaultLongitude)
        ).ReturnsAsync(() => emissionData);


        // Act & Assert
        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.That(result.Count(), Is.EqualTo(2));

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.That(first.Duration, Is.EqualTo(expectedDuration));

        var second = result.Skip(1)?.First();
        Assert.IsNotNull(second);
        Assert.That(second.Duration, Is.EqualTo(expectedDuration));
    }
}