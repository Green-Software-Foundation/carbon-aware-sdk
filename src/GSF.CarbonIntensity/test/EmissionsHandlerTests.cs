using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Handlers;
using GSF.CarbonIntensity.Models;
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
        var emissionsHandler = new EmissionsHandler(this.MockCarbonAwareLogger.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);

        var parametersDTO = new CarbonAwareParametersBaseDTO
        {
            SingleLocation = location,
            Start = start,
            End = end
        };

        // Act
        var carbonIntensityOutput = await emissionsHandler.GetAverageCarbonIntensity(parametersDTO);

        // Assert
        var expectedContent = new CarbonIntensityResult { Location = location, StartTime = start, EndTime = end, CarbonIntensity = data }.ToString();
        var actualContent = (carbonIntensityOutput == null) ? string.Empty : carbonIntensityOutput.ToString();
        Assert.AreEqual(expectedContent, actualContent);
    }

}