namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.Plugins;
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

    protected static Mock<ICarbonAware> CreatePluginWithData(List<EmissionsData> data)
    {
        var plugin = new Mock<ICarbonAware>();
        plugin.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).ReturnsAsync(data);
        return plugin;
    }

    protected static Mock<ICarbonAware> CreatePluginWithException()
    {
        var plugin = new Mock<ICarbonAware>();
        plugin.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).Throws<Exception>();
        return plugin;
    }
}
