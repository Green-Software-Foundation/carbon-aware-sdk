namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.Plugin;
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

    protected static Mock<IPlugin> CreatePluginWithData(List<EmissionsData> data)
    {
        var plugin = new Mock<IPlugin>();
        plugin.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).ReturnsAsync(data);
        return plugin;
    }

    protected static Mock<IPlugin> CreatePluginWithException()
    {
        var plugin = new Mock<IPlugin>();
        plugin.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).Throws<Exception>();
        return plugin;
    }
}
