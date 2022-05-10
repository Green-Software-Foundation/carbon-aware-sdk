namespace CarbonAware.Tests;
using CarbonAware;
using Microsoft.Extensions.Logging;
using Moq;

public class CarbonAwareCoreTests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void GetsDataAsExpected()
    {
        // Create a carbon aware core with a mock plugin
        // and data service
        var staticDataService = new MockDataService();
        
        // Assume the data service is set
        Assert.NotNull(staticDataService.EmissionsFile.Date);
        
        Assert.Greater(staticDataService.EmissionsFile.Emissions.Count, 0);

        var plugin = new MockLogicPlugin(staticDataService);
        var logger = Mock.Of<ILogger<CarbonAwareCore>>();
        var carbonAware = new CarbonAwareCore(logger, plugin);

        // This is NOT a logic test, the carbon aware core
        // is a pass through that adds logging and consistency
        // This should return what the mock service returns
        // which is the entire list from the data source
        // with zero filtering or logic
        var list1 = carbonAware.GetBestEmissionsDataForLocationsByTime(new List<string> { "westus" }, DateTime.Now);
        var directList = staticDataService.GetData();
        Assert.AreEqual(list1, directList);

        // ... regardless of parameters... 
        var list2 = carbonAware.GetBestEmissionsDataForLocationsByTime(new List<string> { "eastus" }, DateTime.Now);
        Assert.AreEqual(list2, directList);

    }
}