using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GSF.CarbonIntensity.Tests;

[TestFixture]
public class ForecastHandlerTests
{
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Mock<ILogger<ForecastHandler>> Logger { get; set; }
    private Mock<ICarbonAwareAggregator> Aggregator { get; set; }
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<ForecastHandler>>();
        Aggregator = new Mock<ICarbonAwareAggregator>();
    }

    [Test]
    public async Task GetCurrent_Succeed_Call_MockAggregator()
    {
        var data = new List<CarbonAware.Model.EmissionsForecast> {
            new CarbonAware.Model.EmissionsForecast {
                DataEndAt = DateTimeOffset.Parse("2022-03-01T18:30:00Z"),
                DataStartAt = DateTimeOffset.Parse("2022-03-01T15:30:00Z"),
                ForecastData = Array.Empty<CarbonAware.Model.EmissionsData>(),
                OptimalDataPoints = Array.Empty<CarbonAware.Model.EmissionsData>(),
            }
        };

        Aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ReturnsAsync(data);

        var handler = new ForecastHandler(Logger.Object, Aggregator.Object);
        var result = await handler.GetCurrent(It.IsAny<string>(),It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>());
        Assert.That(result, Is.Not.Null);
    }
}
