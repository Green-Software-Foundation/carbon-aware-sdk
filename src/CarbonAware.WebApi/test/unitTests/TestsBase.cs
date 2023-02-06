using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using CarbonAware.WebApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using CarbonAware.Aggregators.CarbonAware;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;
using CarbonAware.Aggregators.Emissions;
using CarbonAware.Aggregators.Forecast;

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
        var aggregator = new Mock<IEmissionsAggregator>();

        handler.Setup(x => x.GetEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
            .Callback((string[] location, DateTimeOffset? start, DateTimeOffset? end) =>
            {
                aggregator.Setup(x => x.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
                    .Callback((CarbonAwareParameters parameters) =>
                    {
                        parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                        parameters.Validate();
                    }).ReturnsAsync(It.IsAny<List<CarbonAware.Model.EmissionsData>>());
            })
            .ReturnsAsync(data);
        return handler;
    }

    protected static Mock<IEmissionsHandler> CreateHandlerWithBestEmissionsData(List<EmissionsData> data)
    {
        var handler = new Mock<IEmissionsHandler>();
        var aggregator = new Mock<IEmissionsAggregator>();

        handler.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
            .Callback((string[] location, DateTimeOffset? start, DateTimeOffset? end) =>
            {
                aggregator.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
                    .Callback((CarbonAwareParameters parameters) =>
                    {
                        parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                        parameters.Validate();
                    }).ReturnsAsync(It.IsAny<List<CarbonAware.Model.EmissionsData>>());
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
        var aggregator = new Mock<IForecastAggregator>();

        handler.Setup(x => x.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()))
            .Callback((string[] locations, DateTimeOffset? dataStartAt, DateTimeOffset? dataEndAt, int? windowSize) =>
            {
                aggregator.Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
                    .Callback((CarbonAwareParameters parameters) =>
                    {
                        parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                        parameters.Validate();
                    }).ReturnsAsync(It.IsAny<List<CarbonAware.Model.EmissionsForecast>>());
            })
            .ReturnsAsync(forecasts);
        return handler;
    }

    protected static Mock<IEmissionsHandler> CreateCarbonAwareHandlerWithAverageCI(double data)
    {
        var handler = new Mock<IEmissionsHandler>();
        var aggregator = new Mock<IEmissionsAggregator>();

        handler.Setup(x => x.GetAverageCarbonIntensityAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((string location, DateTimeOffset start, DateTimeOffset end) =>
            {
                aggregator.Setup(x => x.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>()))
                    .Callback((CarbonAwareParameters parameters) =>
                    {
                        parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
                        parameters.Validate();
                    }).ReturnsAsync(It.IsAny<double>());
            })
            .ReturnsAsync(data);

        return handler;
    }

    protected static Mock<ILocationHandler> CreateLocations(bool withcontent = true)
    {
        var locationSource = new Mock<ILocationHandler>();
        var data = new Dictionary<string, Location>();
        if (withcontent)
        {
            data.Add("eastus", new Location {
                Name = "eastus",
                Latitude = 1,
                Longitude = 1
            });
        }
        locationSource.Setup(x => x.GetLocationsAsync())
            .ReturnsAsync(data);
        return locationSource;
    }
}
