using CarbonAware.Exceptions;
using CarbonAware.Model;

namespace CarbonAware.Tests;

public class EmissionsForecastTests
{
    [Test]
    public void GetDurationBetweenForecastDataPoints_ThrowsWhenZeroDatapointsReturned()
    {
        // Arrange
        var forecast = new EmissionsForecast()
        {
            GeneratedAt = DateTimeOffset.Now,
            ForecastData = new List<EmissionsData>()
        };

        // Act & Assert
        Assert.Throws<CarbonAwareException>(() => forecast.GetDurationBetweenForecastDataPoints());
    }

    [Test]
    public void GetDurationBetweenForecastDataPoints_ThrowsWhenOneDatapointReturned()
    {
        // Arrange
        var forecast = new EmissionsForecast()
        {
            GeneratedAt = DateTimeOffset.Now,
            ForecastData = new List<EmissionsData>()
            {
                new EmissionsData()
            }
        };

        // Act & Assert
        Assert.Throws<CarbonAwareException>(() => forecast.GetDurationBetweenForecastDataPoints());
    }

    [Test]
    public void GetDurationBetweenForecastDataPoints_WhenMultipleDataPoints_ReturnsExpectedDuration()
    {
        // Arrange
        var expectedDuration = TimeSpan.FromMinutes(5);
        var startTime = DateTimeOffset.Parse("2022-07-01T12:00:00Z");
        var dataPoints = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Time = startTime
            },
            new EmissionsData()
            {
                Time = startTime + expectedDuration
            },
        };
        
        var forecast = new EmissionsForecast()
        {
            GeneratedAt = DateTimeOffset.Now,
            ForecastData = dataPoints
        };

        // Act
        var result = forecast.GetDurationBetweenForecastDataPoints();
        
        // Assert
        Assert.AreEqual(expectedDuration, result);
    }
}
