using CarbonAware.DataSources.ElectricityMapsFree;
using CarbonAware.DataSources.ElectricityMapsFree.Client;
using CarbonAware.DataSources.ElectricityMapsFree.Model;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources.Exceptions;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace CarbonAware.DataSources.ElectricityMapsFree.Tests;

[TestFixture]
public class ElectricityMapsFreeDataSourceTests
{
    private Mock<ILogger<ElectricityMapsFreeDataSource>> _logger { get; set; }
    
    private Mock<IElectricityMapsFreeClient> _ElectricityMapsFreeClient { get; set; }
    
    private Mock<ILocationSource> _locationSource { get; set; }
    private ElectricityMapsFreeDataSource _dataSource { get; set; }

    private static Location _defaultLocation = new Location() { Name = "eastus", Latitude = 34.123m, Longitude = 123.456m };
    
    private static string _defaultLatitude => Convert.ToString(_defaultLocation.Latitude, CultureInfo.InvariantCulture) ?? "";
    private static string _defaultLongitude => Convert.ToString(_defaultLocation.Longitude, CultureInfo.InvariantCulture) ?? "";

    public ElectricityMapsFreeDataSourceTests()
    {
        _logger = new Mock<ILogger<ElectricityMapsFreeDataSource>>();
        _ElectricityMapsFreeClient = new Mock<IElectricityMapsFreeClient>();
        _locationSource = new Mock<ILocationSource>();
        _dataSource = new ElectricityMapsFreeDataSource(_logger.Object, _ElectricityMapsFreeClient.Object, _locationSource.Object);
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsResultsWhenRecordsFound()
    {
        var startDate = DateTimeOffset.UtcNow.AddHours(-10);
        var endDate = startDate.AddHours(1);
        var expectedCarbonIntensity = 100;

        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        GridEmissionDataPoint emissionData = new()
        {
            Data = new Data()
            {
                CarbonIntensity = expectedCarbonIntensity,
            }
        };

        this._ElectricityMapsFreeClient.Setup(c => c.GetCurrentEmissionsAsync(
            _defaultLatitude,
            _defaultLongitude)
        ).ReturnsAsync(emissionData);

        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(1));

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.That(first.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(first.Location, Is.EqualTo(_defaultLocation.Name));

        this._locationSource.Verify(l => l.ToGeopositionLocationAsync(_defaultLocation));
    }

    [Test]
    public async Task GetCarbonIntensity_UseRegionNameWhenCoordinatesNotAvailable()
    {
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = startDate.AddMinutes(1);
        var expectedCarbonIntensity = 100;

        this._locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Throws<LocationConversionException>();

        GridEmissionDataPoint emissionData = new()
        {
            Data = new Data()
            {
                CarbonIntensity = expectedCarbonIntensity,
            }
        };

        this._ElectricityMapsFreeClient.Setup(c => c.GetCurrentEmissionsAsync(_defaultLocation.Name ?? "")).ReturnsAsync(emissionData);

        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(1));

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.That(first.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(first.Location, Is.EqualTo(_defaultLocation.Name));

        this._locationSource.Verify(l => l.ToGeopositionLocationAsync(_defaultLocation));
        this._ElectricityMapsFreeClient.Verify(l => l.GetCurrentEmissionsAsync(_defaultLocation.Name ?? ""));
    }

    [Test]
    public void GetCarbonIntensity_ThrowsWhenClientDoesNotKnowRegionName()
    {
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = startDate.AddMinutes(1);

        Location nonExistingLocation = new Location() { Name = "WrongLocationName", Latitude = 34.123m, Longitude = 123.456m };
        this._locationSource.Setup(l => l.ToGeopositionLocationAsync(nonExistingLocation)).Throws<LocationConversionException>();

        this._ElectricityMapsFreeClient.Setup(c => c.GetCurrentEmissionsAsync(nonExistingLocation.Name)).Throws(new ElectricityMapsFreeClientHttpException("Error", new System.Net.Http.HttpResponseMessage()));

        Assert.ThrowsAsync<ElectricityMapsFreeClientHttpException>(async () => await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { nonExistingLocation }, startDate, endDate));
    }
}