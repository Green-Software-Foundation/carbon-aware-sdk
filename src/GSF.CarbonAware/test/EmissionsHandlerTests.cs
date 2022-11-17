using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Tools.WattTimeClient;
using GSF.CarbonAware.Exceptions;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;
using CarbonAware.Aggregators.Emissions;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class EmissionsHandlerTests
{

    private Mock<ILogger<EmissionsHandler>>? Logger { get; set; }

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<EmissionsHandler>>();
    }

    private static Mock<IEmissionsAggregator> CreateCarbonAwareAggregatorWithAverageCI(double data)
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

    /// <summary>
    /// Tests that successfull call to the aggregator with any data returned results in expected format.
    /// </summary>
    [TestCase("Sydney", "2022-03-07T01:00:00", "2022-03-07T03:30:00", TestName = "GetAverageCarbonIntensity calls aggregator successfully")]
    public async Task GetAverageCarbonIntensity_SuccessfulCallReturnsOk(string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        double data = 0.7;
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);

        var parametersDTO = new CarbonAwareParametersBaseDTO
        {
            SingleLocation = location,
            Start = start,
            End = end
        };

        // Act
        double? carbonIntensityOutput = await emissionsHandler.GetAverageCarbonIntensityAsync(location, start,end);

        // Assert
        var expectedContent = data;
        var actualContent = (carbonIntensityOutput == null) ? 0 : carbonIntensityOutput;
        Assert.AreEqual(expectedContent, actualContent);
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

}