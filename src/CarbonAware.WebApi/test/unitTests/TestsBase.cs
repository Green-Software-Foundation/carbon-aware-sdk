using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Emissions;
using CarbonAware.Aggregators.Forecast;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

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

    protected static Mock<IEmissionsAggregator> CreateEmissionsAggregator(List<EmissionsData> data)
    {
        var aggregator = new Mock<IEmissionsAggregator>();
        aggregator.Setup(x => x.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);
        return aggregator;
    }

    protected static Mock<IEmissionsAggregator> CreateAggregatorWithBestEmissionsData(List<EmissionsData> data)
    {
        var aggregator = new Mock<IEmissionsAggregator>();
        aggregator.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);
        return aggregator;
    }

    protected static Mock<IForecastAggregator> CreateForecastAggregator(List<EmissionsData> data)
    {
        var forecasts = new List<EmissionsForecast>()
        {
            new EmissionsForecast(){ ForecastData = data }
        };
        var aggregator = new Mock<IForecastAggregator>();
        aggregator.Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(forecasts);
        return aggregator;
    }

    protected static Mock<IEmissionsAggregator> CreateCarbonAwareAggregatorWithAverageCI(double data)
    {
        var aggregator = new Mock<IEmissionsAggregator>();
        aggregator.Setup(x => x.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
                parameters.Validate();
            })
            .ReturnsAsync(data);

        return aggregator;
    }
}
