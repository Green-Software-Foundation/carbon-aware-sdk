using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

namespace GSF.CarbonIntensity.Tests;

[TestFixture]
public class EmissionsHandlerTests
{
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Mock<ILogger<EmissionsHandler>> Logger { get; set; }
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<EmissionsHandler>>();
    }

    private static Mock<ICarbonAwareAggregator> CreateCarbonAwareAggregatorWithAverageCI(double data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
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
    [TestCase("Sydney", "2022-03-07T01:00:00", "2022-03-07T03:30:00", TestName = "Library GetAverageCarbonIntensity Success")]
    public async Task GetAverageCarbonIntensity_SuccessfulCallReturnsOk(string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        double data = 0.7;
        var emissionsHandler = new EmissionsHandler(Logger.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);

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

}