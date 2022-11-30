using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Tools.WattTimeClient;
using EmissionsData = CarbonAware.Model.EmissionsData;
using GSF.CarbonAware.Exceptions;
using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;
using CarbonAware.Aggregators.Emissions;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class EmissionsHandlerTests
{
    private Mock<ILogger<EmissionsHandler>>? Logger { get; set; }

    private readonly EmissionsData[] EmptyTestData = Array.Empty<EmissionsData>();

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<EmissionsHandler>>();
    }

    /// <summary>
    /// GetEmissionsData: Tests empty or null location arrays throw ArgumentException.
    /// </summary>
    [TestCase(new object?[] { null, null }, TestName = "GetEmissions, array of nulls, throws: simulates 'location=&location=' empty value input")]
    [TestCase(new object?[] { null, }, TestName = "GetEmissions, array of nulls, throws: simulates 'location=' empty value input")]
    [TestCase(new object?[] { }, TestName = "GetEmissions, empty array, throws: simulates no 'location' input")]
    public void GetEmissionsData_NoLocations_ThrowsException(params string[] locations)
    {
        // Arrange
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsAggregator(EmptyTestData).Object);

        //Act/Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await emissionsHandler.GetEmissionsDataAsync(locations));
    }

    /// <summary>
    /// GetEmissionsData: Tests that successful emissions call to aggregator with varied location input returns expected data.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetEmissions, successful: simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetEmissions, successful: simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney", "Melbourne" }, TestName = "GetEmissions, successful: simulates 'location=Sydney&location=Melbourne'")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetEmissions, successful: simulates 'location=Sydney'")]
    public async Task GetEmissionsData_VariedLocationsInput_SuccessfulCall(params string[] locations)
    {
        // Arrange
        var data = new EmissionsData[]
        {
            new EmissionsData()
            {
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsAggregator(data).Object);

        // Act
        var result = await emissionsHandler.GetEmissionsDataAsync(locations);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.First().Location, Is.EqualTo("Sydney"));
    }

    /// <summary>
    /// GetBestEmissionsData: Tests empty or null location arrays throw ArgumentException.
    /// </summary>
    [TestCase(new object?[] { null, null }, TestName = "GetBestEmissions, array of nulls, throws: simulates 'location=&location=' empty value input")]
    [TestCase(new object?[] { null, }, TestName = "GetBestEmissions, array of nulls, throws: simulates 'location=' empty value input")]
    [TestCase(new object?[] { }, TestName = "GetBestEmissions, empty array, throws: simulates no 'location' query string")]
    public void GetBestEmissionsData_NoLocations_ThrowsException(params string[] locations)
    {
        // Arrange
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsAggregatorWithBestEmissionsData(EmptyTestData).Object);

        //Act/Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await emissionsHandler.GetBestEmissionsDataAsync(locations));
    }

    /// <summary>
    /// GetBestEmissionsData: Tests that successful emissions call to aggregator with varied location input returns expected data.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetBestEmissions, successful: simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetBestEmissions, successful: simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney", "Melbourne" }, TestName = "GetBestEmissions, successful: simulates 'location=Sydney&location=Melbourne'")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetBestEmissions, successful: simulates 'location=Sydney'")]
    public async Task GetBestEmissionsData_VariedLocationsInput_SuccessfulCall(params string[] locations)
    {
        // Arrange
        var data = new EmissionsData[]
        {
            new EmissionsData()
            {
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var emissionsHandler = new EmissionsHandler(Logger!.Object,CreateEmissionsAggregatorWithBestEmissionsData(data).Object);

        // Act
        var result = await emissionsHandler.GetBestEmissionsDataAsync(locations);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.First().Location, Is.EqualTo("Sydney"));
    }

    /// <summary>
    /// GetAverageCarbonIntensity: Single time range, successfull call to the aggregator with any data returned results in expected format.
    /// </summary>
    [TestCase("Sydney", "2022-03-07T01:00:00", "2022-03-07T03:30:00", TestName = "GetAverageCarbonIntensity calls aggregator successfully")]
    public async Task GetAverageCarbonIntensity_SuccessfulCall(string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        double data = 0.7;
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsAggregatorWithAverageCI(data).Object);

        // Act
        var value = await emissionsHandler.GetAverageCarbonIntensityAsync(location, start, end);

        // Assert
        var expectedContent = data;
        Assert.That(value, Is.EqualTo(expectedContent));
    }

    /// <summary>
    /// Tests that when an error is thrown, it is caught and wrapped in the custom exception.
    /// </summary>
    [Test]
    public void GetAverageCarbonIntensity_ErrorThrowsCustomException()
    {
        // Arrange
        var aggregator = new Mock<IEmissionsAggregator>();
        aggregator.Setup(x => x.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>())).ThrowsAsync(new WattTimeClientException(""));
        var emissionsHandler = new EmissionsHandler(Logger!.Object, aggregator.Object);

        // Act/Assert
        Assert.ThrowsAsync<CarbonAwareException>(async () => await emissionsHandler.GetAverageCarbonIntensityAsync("location", DateTimeOffset.Now, DateTimeOffset.Now));
    }

    private static Mock<IEmissionsAggregator> CreateEmissionsAggregatorWithAverageCI(double data)
    {
        var aggregator = new Mock<IEmissionsAggregator>();
        aggregator.Setup(x => x.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
                parameters.Validate();
            })
            .ReturnsAsync(data);

        return aggregator;
    }

    private static Mock<IEmissionsAggregator> CreateEmissionsAggregator(EmissionsData[] data)
    {
        var aggregator = new Mock<IEmissionsAggregator>();
        aggregator.Setup(x => x.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);
        return aggregator;
    }

    private static Mock<IEmissionsAggregator> CreateEmissionsAggregatorWithBestEmissionsData(EmissionsData[] data)
    {
        var aggregator = new Mock<IEmissionsAggregator>();
        aggregator.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);
        return aggregator;
    }
}