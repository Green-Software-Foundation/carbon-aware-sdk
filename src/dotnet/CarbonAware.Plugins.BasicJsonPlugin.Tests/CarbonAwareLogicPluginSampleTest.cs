using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace CarbonAware.Plugins.BasicJsonPlugin.Tests;

public class MockLocations
{
    public static string Sydney = "Sydney";
    public static string Melbourne = "Melbourne";
    public static string Auckland = "Auckland";
    public static string DoesNotExist = "DOES NOT EXIST";
}

public class MockRatings
{
    public static double Sydney = 0.9;
    public static double Melbourne = 0.7;
    public static double Auckland = 0.1;
}

public class MockCarbonAwareStaticDataService : ICarbonAwareStaticDataService
{
    private readonly List<EmissionsData> _data = new List<EmissionsData>()
        {
            // Sydney with very high emissions
            new EmissionsData()
            {
                Location = MockLocations.Sydney,
                Rating = MockRatings.Sydney,
                Time = DateTime.Now
            },
            // Melbourne with high emissions
            new EmissionsData()
            {
                Location = MockLocations.Melbourne,
                Rating = MockRatings.Melbourne,
                Time = DateTime.Now
            },
            // Auckland with very low emissions
            new EmissionsData()
            {
                Location = MockLocations.Auckland,
                Rating = MockRatings.Auckland,
                Time = DateTime.Now
            },
            // Auckland with very low emissions
            new EmissionsData()
            {
                Location = MockLocations.Sydney,
                Rating = MockRatings.Sydney,
                Time = DateTime.Now + TimeSpan.FromMinutes(1)
            }
        };

    public void Configure(IConfigurationSection configuration)
    {

    }

    public List<EmissionsData> GetData()
    {
        return _data;
    }

}

public class Tests
{
    private ICarbonAwarePlugin _plugin;
    private CarbonAwareCore _core;

    [SetUp]
    public void Setup()
    {
        Console.WriteLine("Test Setup");
        var dataService = new MockCarbonAwareStaticDataService();
        dataService.Configure(null);
        _plugin = new CarbonAwareBasicDataPlugin(dataService);
        _core = new CarbonAwareCore(_plugin);
    }

    [TearDown]
    public void TearDown()
    {
        Console.WriteLine("Test TearDown");
    }

    [Test]
    public void AssertMetadataPropertiesAreSet()
    {
        Assert.NotNull(_plugin.Author);
        Assert.NotNull(_plugin.Description);
        Assert.NotNull(_plugin.Name);
        Assert.NotNull(_plugin.URL);
        Assert.NotNull(_plugin.Version);
    }

    [Test]
    public void TestEmissionsDataForLocationByTime()
    {
        var ed = _plugin.GetEmissionsDataForLocationByTime(MockLocations.Sydney, DateTime.Now);
        Assert.AreEqual(MockRatings.Sydney, ed[0].Rating);

        // Should get nothing if there is no location
        ed = _plugin.GetEmissionsDataForLocationByTime(MockLocations.DoesNotExist, DateTime.Now);
        Assert.AreEqual(0, ed.Count);

        // Should get nothing if there is no data prior
        ed = _plugin.GetEmissionsDataForLocationByTime(MockLocations.Sydney, DateTime.Now - TimeSpan.FromDays(100));
        Assert.AreEqual(0, ed.Count);

        // Core running on same plugin should have same result
        ed = _core.GetEmissionsDataForLocationByTime(MockLocations.Sydney, DateTime.Now);
        Assert.AreEqual(MockRatings.Sydney, ed[0].Rating);

        // Core running on same plugin should have same result
        ed = _core.GetEmissionsDataForLocationByTime(MockLocations.Sydney, DateTime.Now, DateTime.Now + TimeSpan.FromHours(1));
        Assert.AreEqual(MockRatings.Sydney, ed[0].Rating);
    }

    [Test]
    public void TestEmissionsDataForLocationsByTime()
    {
        var locations = new List<string>() { MockLocations.Sydney, MockLocations.Auckland };

        var emissionDataList = _plugin.GetEmissionsDataForLocationsByTime(locations, DateTime.Now);
        Assert.AreEqual(2, emissionDataList.Count);
        Assert.AreEqual(MockRatings.Sydney, emissionDataList[0].Rating);
        Assert.AreEqual(MockRatings.Auckland, emissionDataList[1].Rating);

        // Core should have the same result with the same plugin
        emissionDataList = _core.GetEmissionsDataForLocationsByTime(locations, DateTime.Now);
        Assert.AreEqual(2, emissionDataList.Count);
        Assert.AreEqual(MockRatings.Sydney, emissionDataList[0].Rating);
        Assert.AreEqual(MockRatings.Auckland, emissionDataList[1].Rating);
    }

    [Test]
    public void TestEmissionsDataForBestLocationByTime()
    {
        var locations1 = new List<string>() { MockLocations.Sydney, MockLocations.Melbourne };
        var locations2 = new List<string>() { MockLocations.Sydney, MockLocations.Auckland, MockLocations.Melbourne };

        // Plugin should find the lowest emissions location 
        var ed = _plugin.GetBestEmissionsDataForLocationsByTime(locations1, DateTime.Now);
        Assert.AreEqual(MockLocations.Melbourne, ed[0].Location);
        ed = _plugin.GetBestEmissionsDataForLocationsByTime(locations2, DateTime.Now);
        Assert.AreEqual(MockLocations.Auckland, ed[0].Location);

        // Core should have the same result with the same plugin 
        ed = _core.GetBestEmissionsDataForLocationsByTime(locations1, DateTime.Now);
        Assert.AreEqual(MockLocations.Melbourne, ed[0].Location);
        ed = _core.GetBestEmissionsDataForLocationsByTime(locations2, DateTime.Now);
        Assert.AreEqual(MockLocations.Auckland, ed[0].Location);
    }
}
