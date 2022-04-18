using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Interfaces;

namespace CarbonAware.Tests;

public class ServiceManagerTests
{
    IConfigManager? _goodConfigManager;
    IConfigManager? _badTypesConfigManager;
    IConfigManager? _badServiceConfigManager;

    [SetUp]
    public void Setup()
    {
        _goodConfigManager = new ConfigManager("files/carbon-aware/test-carbon-aware-clean.json");
        _badTypesConfigManager = new ConfigManager("files/carbon-aware/test-carbon-aware-bad-types.json");
        _badServiceConfigManager = new ConfigManager("files/carbon-aware/test-carbon-aware-bad-service-config.json");
    }

    [Test]
    public void CreatesWithConfigManager()
    {
        var serviceManager = new ServiceManager(_goodConfigManager);
        Assert.Pass();
    }

    [Test]
    public void CreatesWithoutPluginDirectory()
    {
        var pluginDirPath = AppDomain.CurrentDomain.BaseDirectory + ServiceManager.PLUGINS_FOLDER;
        if (Directory.Exists(pluginDirPath))
        {
            Directory.Delete(pluginDirPath);
        }

        var serviceManager = new ServiceManager(_goodConfigManager);
        Assert.Pass();
    }

    [Test]
    public void CreatesWithPluginDirectory()
    {
        var pluginDirPath = AppDomain.CurrentDomain.BaseDirectory + ServiceManager.PLUGINS_FOLDER;
        if (!Directory.Exists(pluginDirPath))
        {
            Directory.CreateDirectory(pluginDirPath);
        }

        var serviceManager = new ServiceManager(_goodConfigManager);
        Directory.Delete(pluginDirPath);

        Assert.Pass();
    }

    [Test]
    public void CreatesAndRetrievesServices()
    {
        var serviceManager = new ServiceManager(_goodConfigManager);

        var plugin = serviceManager.ServiceProvider.GetService<ICarbonAwarePlugin>();
        Assert.IsNotNull(plugin);

        var data = serviceManager.ServiceProvider.GetService<ICarbonAwareStaticDataService>();
        Assert.IsNotNull(data);

        Assert.Pass();
    }

    [Test]
    public void HandlesBadTypesInConfig()
    {
        try
        {
            var serviceManager = new ServiceManager(_badTypesConfigManager);
        }
        catch (ArgumentException)
        {
            Assert.Pass();
        }
    }


    [Test]
    public void HandlesBadServiceConfig()
    {
        try
        {
            var serviceManager = new ServiceManager(_badServiceConfigManager);
        }
        catch (ArgumentException)
        {
            Assert.Pass();
        }
    }
}