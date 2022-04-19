namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.WebApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;

/// <summary>
/// TestsBase for all WebAPI specific tests.
/// </summary>
public abstract class TestsBase
{
    protected TestsBase()
    {
        this.MockLogger = new Mock<ILogger<CarbonAwareController>>();
    }

    protected Mock<ILogger<CarbonAwareController>> MockLogger { get; }

    protected static Mock<ICarbonAwareAggregator> CreateAggregatorWithData(List<EmissionsData> data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).ReturnsAsync(data);
        return aggregator;
    }

    protected static Mock<ICarbonAwareAggregator> CreateAggregatorWithException()
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).Throws<Exception>();
        return aggregator;
    }
}
