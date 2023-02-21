using CarbonAware.LocationSources.Configuration;
using CarbonAware.LocationSources.Exceptions;
using CarbonAware.LocationSources.Model;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Reflection;
using System.Text.Json;

namespace CarbonAware.LocationSources.Test;

[TestFixture]
public class LocationSourceTest
{

    private string _goodFile { get; set; }
    private string _badFile { get; set; }
    private string _dupFile { get; set; }
    private string _assemblyDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


    [OneTimeSetUp]
    protected async Task CreateTestLocationFiles()
    {
        var goodData = new Dictionary<string, NamedGeoposition>
        {
            { Constants.EastUsRegion.Name, Constants.EastUsRegion },
            { Constants.WestUsRegion.Name, Constants.WestUsRegion },
            { Constants.NorthCentralRegion.Name, Constants.NorthCentralRegion },
        };
        _goodFile = await GenerateTestLocationFile(goodData);
        var badData = new Dictionary<string, NamedGeoposition>
        {
            { Constants.NorthCentralRegion.Name, Constants.NorthCentralRegion },
            { Constants.FakeRegion.Name, Constants.FakeRegion },
        };
        _badFile = await GenerateTestLocationFile(badData, "bad");

        var dupData = new Dictionary<string, NamedGeoposition>
        {
            { Constants.EastUsRegion.Name, Constants.EastUsRegion },
            { Constants.WestUsRegion.Name, Constants.WestUsRegion }
        };
        _dupFile = await GenerateTestLocationFile(dupData);
    }

    [Test]
    public void GeopositionLocation_InvalidRegionName_ThrowsException()
    {
        var configuration = new LocationDataSourcesConfiguration();
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location invalidLocation = new Location()
        {
            Name = "invalid location"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(invalidLocation);
        });
    }

    [Test]
    public void GeopositionLocation_InvalidNamedLocation_ThrowsException()
    {
        
        var logger = Mock.Of<ILogger<LocationSource>>();
        var configuration = new LocationDataSourcesConfiguration();
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        configuration.LocationSourceFiles.Add(new LocationSourceFile
        {
            DataFileLocation = _badFile
        });
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var locationSource = new LocationSource(logger, options.Object);

        Assert.ThrowsAsync<LocationConversionException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(Constants.FakeLocation);
        });
    }

    [Test]
    public async Task GeopositionLocation_ValidLocation_With_MultiConfiguration()
    {
        var configuration = new LocationDataSourcesConfiguration();
        configuration.LocationSourceFiles.Add(new LocationSourceFile
            {
                Prefix = "prefix1",
                Delimiter = "-",
                DataFileLocation = _goodFile
            });
        configuration.LocationSourceFiles.Add(new LocationSourceFile
            {
                Prefix = "prefix2",
                Delimiter = "_",
                DataFileLocation = _goodFile
            });
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            Name = "prefix1-test-eastus"
        };

        var eastResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);

        inputLocation = new Location {
            Name = "prefix2_test-westus"
        };

        var westResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);
    }

    [TestCase("prefix-test-eastus", TestName = "ValidLocation_CaseInsensitive: exact match case")]
    [TestCase("PrEfIx-test-eastus", TestName = "ValidLocation_CaseInsensitive: prefix case insensitive")]
    [TestCase("prefix-teST-EAstus", TestName = "ValidLocation_CaseInsensitive: location name case insensitive")]
    [TestCase("PREFIX-teST-EAstus", TestName = "ValidLocation_CaseInsensitive: both case insensitive")]
    public async Task GeopositionLocation_ValidLocation_IgnoresCase(string locationInputName)
    {
        var configuration = new LocationDataSourcesConfiguration();
        configuration.LocationSourceFiles.Add(new LocationSourceFile
            {
                Prefix = "prefix",
                Delimiter = "-",
                DataFileLocation = _goodFile
            });
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            Name = locationInputName
        };

        var result = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, result);
    }

    [Test]
    public async Task GeopositionLocation_ValidLocation_No_Configuration_DiscoverFiles()
    {
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => new LocationDataSourcesConfiguration());
        var mockLogger = new Mock<ILogger<LocationSource>>();
        var locationSource = new LocationSource(mockLogger.Object, options.Object);

        Location inputLocation = new Location {
            Name = "test-eastus"
        };

        var eastResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);
        VerifyLoggerCall(mockLogger, "files discovered", LogLevel.Information);

        inputLocation = new Location {
            Name = "test-westus"
        };

        var westResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);
    }

   [Test]
    public void GeopositionLocation_InvalidLocation_With_Configuration()
    {
        var configuration = new LocationDataSourcesConfiguration();
        configuration.LocationSourceFiles.Add(new LocationSourceFile
        {
            Prefix = "prefix",
            Delimiter = "-",
            DataFileLocation = _goodFile
        });
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            Name = "test-eastus"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(inputLocation);
        });

        inputLocation = new Location {
            Name = "test-westus"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(inputLocation);
        });
    }

    [Test]
    public async Task GeopositionLocation_ValidLocation_DupLocationKey()
    {
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => new LocationDataSourcesConfiguration());
        var mockLogger = new Mock<ILogger<LocationSource>>();
        var locationSource = new LocationSource(mockLogger.Object, options.Object);

        Location inputLocation = new Location {
            Name = $"{Constants.EastUsRegion.Name}_1"
        };
        var result = await locationSource.ToGeopositionLocationAsync(inputLocation);
        Assert.That(result?.Name, Is.EqualTo(Constants.EastUsRegion.Name));
        inputLocation = new Location {
            Name = $"{Constants.WestUsRegion.Name}_1"
        };
        result = await locationSource.ToGeopositionLocationAsync(inputLocation);
        Assert.That(result?.Name, Is.EqualTo(Constants.WestUsRegion.Name));

        inputLocation = new Location {
            Name = $"{Constants.WestUsRegion.Name}_2"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(inputLocation);
        });
    }

    [OneTimeTearDown]
    protected void RemoveTestLocationFiles()
    {
        var fileList = new List<string> 
        {
            _goodFile, _badFile, _dupFile
        };

        fileList.ForEach(file =>
        {
            var path = Path.Combine(_assemblyDirectory, LocationSourceFile.BaseDirectory, file);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        });
    }

    private static void AssertLocationsEqual(Location expected, Location actual)
    {
        Assert.AreEqual(expected.Latitude, actual.Latitude);
        Assert.AreEqual(expected.Longitude, actual.Longitude);
    }

    private void VerifyLoggerCall(Mock<ILogger<LocationSource>> logger, String message, LogLevel level)
    {
        logger.Verify(m => m.Log(level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(message)),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private async Task<string> GenerateTestLocationFile(Dictionary<string, NamedGeoposition> data, string fileExt = "json")
    {
        var fileName = Path.ChangeExtension(Path.GetRandomFileName(), $".{fileExt}");
        var filePath = Path.Combine(_assemblyDirectory, LocationSourceFile.BaseDirectory, fileName);
        using FileStream createStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(createStream, data);
        await createStream.DisposeAsync();
        return fileName;
    }
}
