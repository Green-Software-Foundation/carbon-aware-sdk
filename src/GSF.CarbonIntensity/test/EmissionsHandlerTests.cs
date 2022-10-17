using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Handlers;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace GSF.CarbonIntensity.Tests;

[TestFixture]
public class EmissionsHandlerTests : TestsBase
{
    /// <summary>
    /// Tests that successfull call to the aggregator with any data returned results in expected format.
    /// </summary>
    [TestCase("Sydney", "2022-03-07T01:00:00", "2022-03-07T03:30:00", TestName = "Library GetAverageCarbonIntensity Success")]
    public async Task GetAverageCarbonIntensity_SuccessfulCallReturnsOk(string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        double data = 0.7;
        var emissionsHandler = new EmissionsHandler(MockEmissionsHandlerLogger.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);

        var parametersDTO = new CarbonAwareParametersBaseDTO
        {
            SingleLocation = location,
            Start = start,
            End = end
        };

        // Act
        double? carbonIntensityOutput = await emissionsHandler.GetAverageCarbonIntensity(parametersDTO);

        // Assert
        var expectedContent = data;
        var actualContent = (carbonIntensityOutput == null) ? 0 : carbonIntensityOutput;
        Assert.AreEqual(expectedContent, actualContent);
    }

}