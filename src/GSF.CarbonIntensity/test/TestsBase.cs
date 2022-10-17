using CarbonAware.Aggregators.CarbonAware;
using Microsoft.Extensions.Logging;
using Moq;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;
using GSF.CarbonIntensity.Handlers;

namespace GSF.CarbonIntensity.Tests;

/// <summary>
/// TestsBase for all GSF.CarbonIntensity specific tests.
/// </summary>
public abstract class TestsBase
{
    protected Mock<ILogger<EmissionsHandler>> MockCarbonAwareLogger { get; }
    protected TestsBase()
    {
        this.MockCarbonAwareLogger = new Mock<ILogger<EmissionsHandler>>();
    }

    protected static Mock<ICarbonAwareAggregator> CreateCarbonAwareAggregatorWithAverageCI(double data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
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
