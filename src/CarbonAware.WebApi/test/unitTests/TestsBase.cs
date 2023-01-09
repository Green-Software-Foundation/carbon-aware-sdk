using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using CarbonAware.WebApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CarbonAware.WepApi.UnitTests;

/// <summary>
/// TestsBase for all WebAPI specific tests.
/// </summary>
public abstract class TestsBase
{
    protected Mock<ILogger<CarbonAwareController>> MockCarbonAwareLogger { get; }
    protected TestsBase()
    {
        this.MockCarbonAwareLogger = new Mock<ILogger<CarbonAwareController>>();
    }

    protected static Mock<IEmissionsHandler> CreateEmissionsHandler(List<EmissionsData> data)
    {
        var handler = new Mock<IEmissionsHandler>();

        handler.Setup(x => x.GetEmissionsDataAsync(It.IsAny<string[]>(), null, null))
            .Callback((string[] location, DateTimeOffset? start, DateTimeOffset? end) =>
            {
                Assert.NotNull(location);
                Assert.NotZero(location.Length);
            })
            .ReturnsAsync(data);
        return handler;
    }

    protected static Mock<IEmissionsHandler> CreateHandlerWithBestEmissionsData(List<EmissionsData> data)
    {
        var handler = new Mock<IEmissionsHandler>();
        handler.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<string[]>(), null, null))
            .Callback((string[] location, DateTimeOffset? start, DateTimeOffset? end) =>
            {
                Assert.NotNull(location);
                Assert.NotZero(location.Length);
            })
            .ReturnsAsync(data);
        return handler;
    }

    protected static Mock<IForecastHandler> CreateForecastHandler(List<EmissionsData> data)
    {
        var forecasts = new List<EmissionsForecast>()
        {
            new EmissionsForecast(){ EmissionsDataPoints = data }
        };
        var handler = new Mock<IForecastHandler>();
        handler.Setup(x => x.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()))
            .Callback((string[] locations, DateTimeOffset? dataStartAt, DateTimeOffset? dataEndAt, int? windowSize) =>
            {
                Assert.NotNull(locations);
                Assert.NotZero(locations.Length);
            })
            .ReturnsAsync(forecasts);
        return handler;
    }

    protected static Mock<IEmissionsHandler> CreateCarbonAwareHandlerWithAverageCI(double data)
    {
        var handler = new Mock<IEmissionsHandler>();
        handler.Setup(x => x.GetAverageCarbonIntensityAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((string location, DateTimeOffset start, DateTimeOffset end) =>
            {
                Assert.NotNull(location);
                Assert.NotZero(location.Length);
                Assert.NotNull(start);
                Assert.NotNull(end);
            })
            .ReturnsAsync(data);

        return handler;
    }
}
