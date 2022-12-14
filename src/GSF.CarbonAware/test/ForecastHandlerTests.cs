using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Forecast;
using GSF.CarbonAware.Exceptions;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class ForecastHandlerTests
{
    private Mock<ILogger<ForecastHandler>>? Logger { get; set; }

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<ForecastHandler>>();
    }

    [TestCase("Sydney", "eastus", "2022-03-07T01:00:00", "2022-03-07T03:30:00", 5, TestName = "GetCurrentForecastAsync calls aggregator: all fields")]
    [TestCase("Sydney", null, null, null, null, TestName = "GetCurrentForecastAsync calls aggregator: required fields only")]
    public async Task GetCurrentForecastAsync_Succeed_Call_MockAggregator_WithOutputData(string location1, string? location2, DateTimeOffset? start, DateTimeOffset? end, int? duration)
    {
        var data = new List<global::CarbonAware.Model.EmissionsForecast> {
            new global::CarbonAware.Model.EmissionsForecast {
                RequestedAt = DateTimeOffset.Now,
                GeneratedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(1),
                ForecastData = Array.Empty<global::CarbonAware.Model.EmissionsData>(),
                OptimalDataPoints = Array.Empty<global::CarbonAware.Model.EmissionsData>(),
            }
        };

        var aggregator = CreateCurrentForecastAggregator(data);
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        var locations = location2 != null ? new string[] { location1, location2 } : new string[] { location1 };
        var result = await handler.GetCurrentForecastAsync(locations, start, end, duration);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task GetCurrentForecastAsync_Succeed_Call_MockAggregator_WithoutOutputData()
    {
        var aggregator = CreateCurrentForecastAggregator(Array.Empty<global::CarbonAware.Model.EmissionsForecast>().ToList());
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        var result = await handler.GetCurrentForecastAsync(new string[] { "eastus" }, DateTimeOffset.Now, DateTimeOffset.Now + TimeSpan.FromHours(1), 30);
        Assert.That(result, Is.Empty);
    }

    [TestCase("Sydney","2022-03-07T01:00:00", "2022-03-07T03:30:00", "2022-03-07T03:30:00", 5, TestName = "GetForecastByDate calls aggregator: all fields")]
    [TestCase("Sydney", null, null, "2022-03-07T01:00:00", null, TestName = "GetForecastByDate calls aggregator: required fields only")]
    public async Task GetForecastByDateAsync_Succeed_Call_MockAggregator_WithOutputData(string location, DateTimeOffset? start, DateTimeOffset? end, DateTimeOffset requestedAt, int? duration)
    {
        var data = new global::CarbonAware.Model.EmissionsForecast {
            RequestedAt = requestedAt,
            GeneratedAt = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1),
            ForecastData = Array.Empty<global::CarbonAware.Model.EmissionsData>(),
            OptimalDataPoints = Array.Empty<global::CarbonAware.Model.EmissionsData>(),
        };

        var aggregator = CreateForecastByDateAggregator(data);
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        var result = await handler.GetForecastByDateAsync(location, start, end, requestedAt, duration);
        Assert.That(result.RequestedAt, Is.EqualTo(requestedAt));
    }

    [Test]
    public void ForecastHandler_WrapsCarbonAwareExceptionsCorrectly()
    {
        var aggregator = SetupMockAggregatorThatThrows();
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        Assert.ThrowsAsync<CarbonAwareException>(async () => await handler.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
        Assert.ThrowsAsync<CarbonAwareException>(async () => await handler.GetForecastByDateAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
    }

    [Test]
    public void ForecastHandler_WrapsSystemExceptionsCorrectly()
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ThrowsAsync(new ArgumentException());

        aggregator
            .Setup(x => x.GetForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ThrowsAsync(new NotImplementedException());
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        Assert.ThrowsAsync<ArgumentException>(async () => await handler.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
        Assert.ThrowsAsync<NotImplementedException>(async () => await handler.GetForecastByDateAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
    }

    private static Mock<IForecastAggregator> CreateCurrentForecastAggregator(IEnumerable<global::CarbonAware.Model.EmissionsForecast> data)
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);

        return aggregator;
    }

    private static Mock<IForecastAggregator> CreateForecastByDateAggregator(global::CarbonAware.Model.EmissionsForecast data)
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Requested);
                parameters.Validate();
            })
            .ReturnsAsync(data);

        return aggregator;
    }

    private static Mock<IForecastAggregator> SetupMockAggregatorThatThrows()
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ThrowsAsync(new CarbonAware.Exceptions.CarbonAwareException(""));

        aggregator
            .Setup(x => x.GetForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ThrowsAsync(new CarbonAware.Exceptions.CarbonAwareException(""));

        return aggregator;
    }
}
