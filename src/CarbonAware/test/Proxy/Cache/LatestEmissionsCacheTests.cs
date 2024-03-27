using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Moq;

namespace CarbonAware.Proxies.Cache;

class LatestEmissionsCacheTests
{

    Mock<IEmissionsDataSource>? _mock;

    IEmissionsDataSource? _dataSource;

    IDictionary<string, DateTimeOffset>? _dateTimes;

    IDictionary<string, EmissionsData>? _emissions;

    IDictionary<string, Location>? _locations;

    [Test]
    public void GetCarbonIntensityAsync_ForSingleLocation_UseCachedDataForSameQuery()
    {
        var config = new EmissionsDataCacheConfiguration();
        config.Enabled = true;
        config.ExpirationMin = 10;

        var dataSource = LatestEmissionsCache<IEmissionsDataSource>.CreateProxy(_dataSource!, config);
        var resultFromDataSource = dataSource!.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;
        var resultFromCache = dataSource!.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;

        // The results are same
        Assert.AreEqual(resultFromDataSource, resultFromCache);
        // The access to the data source occurs just once
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Once);
    }

    [Test]
    public void GetCarbonIntensityAsync_ForSingleLocation_UsePartOfCachedData()
    {
        var config = new EmissionsDataCacheConfiguration();
        config.Enabled = true;
        config.ExpirationMin = 10;

        var dataSource = LatestEmissionsCache<IEmissionsDataSource>.CreateProxy(_dataSource!, config);
        var resultFromDataSource = dataSource!.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["thirdDay"]).Result;
        var resultFromCache = dataSource!.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;

        // The result for the second query is a part of the first one
        Assert.AreEqual(resultFromDataSource.Count(), 2);
        Assert.AreEqual(resultFromCache.Count(), 1);
        // The second query does not access to tha data source
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Never);
    }

    [Test]
    public void GetCarbonIntensityAsync_ForSingleLocation_AccessToDataSourceIfCachedDataIsNotMached()
    {
        var config = new EmissionsDataCacheConfiguration();
        config.Enabled = true;
        config.ExpirationMin = 10;

        var dataSource = LatestEmissionsCache<IEmissionsDataSource>.CreateProxy(_dataSource!, config);
        var resultOfFirst = dataSource!.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;
        var resultOfSecond = dataSource!.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["secondDay"], _dateTimes["thirdDay"]).Result;

        Assert.Contains(_emissions!["eastus-firstDay"], resultOfFirst.ToList());
        Assert.Contains(_emissions["eastus-secondDay"], resultOfSecond.ToList());
        // Both queries access to tha data source
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Once);
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(_locations!["eastus"], _dateTimes!["secondDay"], _dateTimes["thirdDay"]), Moq.Times.Once);
    }

    [Test]
    public void GetCarbonIntensityAsync_ForSingleLocation_AccessToDataSourceIfLocationNameIsEmpty()
    {
        var config = new EmissionsDataCacheConfiguration();
        config.Enabled = true;
        config.ExpirationMin = 10;

        var dataSource = LatestEmissionsCache<IEmissionsDataSource>.CreateProxy(_dataSource!, config);
        var resultOfFirst = dataSource!.GetCarbonIntensityAsync(_locations!["coordinate"], _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;
        var resultOfSecond = dataSource!.GetCarbonIntensityAsync(_locations!["coordinate"], _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;

        Assert.AreEqual(resultOfFirst, resultOfSecond);
        // Both queries access to tha data source
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(_locations!["coordinate"], _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Exactly(2));
    }

    [Test]
    public void GetCarbonIntensityAsync_ForMultiLocation_UseCachedDataForSameQuery()
    {
        var config = new EmissionsDataCacheConfiguration();
        config.Enabled = true;
        config.ExpirationMin = 10;

        var dataSource = LatestEmissionsCache<IEmissionsDataSource>.CreateProxy(_dataSource!, config);
        var resultFromDataSource = dataSource!.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"], _locations["westus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;
        var resultFromCache = dataSource!.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"], _locations["westus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;

        // The results are same
        Assert.AreEqual(resultFromDataSource, resultFromCache);
        // The access to the data source occurs just once
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"], _locations["westus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Once);
    }

    [Test]
    public void GetCarbonIntensityAsync_ForMultiLocation_UsePartOfCachedData()
    {
        var config = new EmissionsDataCacheConfiguration();
        config.Enabled = true;
        config.ExpirationMin = 10;

        var dataSource = LatestEmissionsCache<IEmissionsDataSource>.CreateProxy(_dataSource!, config);
        var resultFromDataSource = dataSource!.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"], _locations["westus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;
        var resultFromCache = dataSource!.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;

        // The result for the second query is a part of the first one
        Assert.AreEqual(resultFromDataSource.Count(), 2);
        Assert.AreEqual(resultFromCache.Count(), 1);
        // The access to the data source occurs just once
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Never);
    }

    [Test]
    public void GetCarbonIntensityAsync_ForMultiLocation_UsePartOfCachedData2()
    {
        var config = new EmissionsDataCacheConfiguration();
        config.Enabled = true;
        config.ExpirationMin = 10;

        var dataSource = LatestEmissionsCache<IEmissionsDataSource>.CreateProxy(_dataSource!, config);
        var resultForFirst = dataSource!.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;
        var resultForSecond = dataSource!.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"], _locations["westus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]).Result;

        Assert.AreEqual(resultForFirst.Count(), 1);
        Assert.AreEqual(resultForSecond.Count(), 2);
        // the first query
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(new List<Location>{_locations!["eastus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Once);
        // the second query does not contain "eastus" because the data for "eastus" have been already cached
        _mock!.Verify(ds => ds.GetCarbonIntensityAsync(new List<Location>{_locations!["westus"]}, _dateTimes!["firstDay"], _dateTimes["secondDay"]), Moq.Times.Once);
    }

    [SetUp]
    public void Setup()
    {
        _mock!.Invocations.Clear();
    }

    [OneTimeSetUp]
    public void SetupDataAndMock()
    {
        // Crate test data
        _dateTimes = new Dictionary<string, DateTimeOffset>
        {
            {"firstDay", new DateTimeOffset(2021, 11, 16, 0, 0, 0, TimeSpan.Zero)},
            {"secondDay", new DateTimeOffset(2021, 11, 17, 0, 0, 0, TimeSpan.Zero)},
            {"thirdDay", new DateTimeOffset(2021, 11, 18, 0, 0, 0, TimeSpan.Zero)}
        };

        var location1 = new Location();
        location1.Name = "eastus";
        var location2 = new Location();
        location2.Name = "westus";
        var location3 = new Location();
        location3.Latitude = 0;
        location3.Longitude = 0;
        _locations = new Dictionary<string, Location>
        {
            {"eastus", location1},
            {"westus", location2},
            {"coordinate", location3}
        };

        var emissions1 = new EmissionsData();
        emissions1.Location = "eastus";
        emissions1.Time = new DateTimeOffset(2021, 11, 16, 0, 55, 0, TimeSpan.Zero);
        var emissions2 = new EmissionsData();
        emissions2.Location = "eastus";
        emissions2.Time = new DateTimeOffset(2021, 11, 17, 0, 55, 0, TimeSpan.Zero);
        var emissions3 = new EmissionsData();
        emissions3.Location = "westus";
        emissions3.Time = new DateTimeOffset(2021, 11, 16, 0, 55, 0, TimeSpan.Zero);
        var emissions4 = new EmissionsData();
        emissions4.Location = "";
        emissions4.Time = new DateTimeOffset(2021, 11, 16, 0, 55, 0, TimeSpan.Zero);
        _emissions = new Dictionary<string, EmissionsData> 
        {
            {"eastus-firstDay", emissions1}, 
            {"eastus-secondDay", emissions2},
            {"westus-firstDay", emissions3},
            {"coordinate-firstDay", emissions4}
        };

        // setup a mock
        _mock = new Mock<IEmissionsDataSource>();

        // setup for GetCarbonIntensityAsync(Location, DateTimeOffset, DateTimeOffset)
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(_locations["eastus"], _dateTimes["firstDay"], _dateTimes["secondDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["eastus-firstDay"]}));
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(_locations["eastus"], _dateTimes["secondDay"], _dateTimes["thirdDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["eastus-secondDay"]}));
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(_locations["eastus"], _dateTimes["firstDay"], _dateTimes["thirdDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["eastus-firstDay"], _emissions["eastus-secondDay"]}));
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(_locations["westus"], _dateTimes["firstDay"], _dateTimes["secondDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["westus-firstDay"]}));
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(_locations["coordinate"], _dateTimes["firstDay"], _dateTimes["secondDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["coordinate-firstDay"]}));

        // setup for GetCarbonIntensityAsync(IEnumerable<Location>, DateTimeOffset, DateTimeOffset)
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(new List<Location>{_locations["eastus"], _locations["westus"]}, _dateTimes["firstDay"], _dateTimes["secondDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["eastus-firstDay"], _emissions["westus-firstDay"]}));
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(new List<Location>{_locations["eastus"]}, _dateTimes["firstDay"], _dateTimes["secondDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["eastus-firstDay"]}));
        _mock.Setup(ds => 
            ds.GetCarbonIntensityAsync(new List<Location>{_locations["westus"]}, _dateTimes["firstDay"], _dateTimes["secondDay"]))
            .Returns(Task.FromResult((IEnumerable<EmissionsData>)new List<EmissionsData>{_emissions["westus-firstDay"]}));

        _dataSource = _mock.Object;
    }

}
