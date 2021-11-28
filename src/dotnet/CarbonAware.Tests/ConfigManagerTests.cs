namespace CarbonAware.Tests;

public class ConfigManagerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void LoadsCleanFile()
    {
        var c = new ConfigManager("files/carbon-aware/test-carbon-aware-clean.json");
        c.GetServiceConfiguration();
        Assert.Pass();
    }

    [Test]
    public void HandlesInvalidConfig()
    {
        try
        {
            var c = new ConfigManager("files/carbon-aware/test-carbon-aware-bad.json");
            c.GetServiceConfiguration();
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            Assert.Pass();
        }
    }

    [Test]
    public void HandlesMalformedJson()
    {
        try
        {
            var c = new ConfigManager("files/test-malformed-json.json");
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            Assert.Pass();
        }
    }

    [Test]
    public void GetsAllServices()
    {
        var c = new ConfigManager("files/carbon-aware/test-carbon-aware-clean.json");
        var services = c.GetServiceConfiguration();
        
        Assert.AreEqual(2, services.Count);

        Assert.AreEqual("Data Service", services[0].name);

        Assert.AreEqual("Logic Service", services[1].name);
    }
}
