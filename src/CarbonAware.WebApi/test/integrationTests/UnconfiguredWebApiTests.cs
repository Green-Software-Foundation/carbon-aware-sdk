using CarbonAware.DataSources.Configuration;
using NUnit.Framework;
using System.Net;

namespace CarbonAware.WebApi.IntegrationTests;

/// <summary>
/// Tests that the Web API starts without configuration.
/// </summary>
[TestFixture(DataSourceType.None)]
public class UnconfiguredWebApiTests : IntegrationTestingBase
{
    private readonly string healthURI = "/health";
    private readonly string fakeURI = "/fake-endpoint";

    public UnconfiguredWebApiTests(DataSourceType dataSource) : base(dataSource) { }

    [Test]
    public async Task HealthCheck_ReturnsOK()
    {
        //Use client to get endpoint
        var result = await _client.GetAsync(healthURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task FakeEndPoint_ReturnsNotFound()
    {
        var result = await _client.GetAsync(fakeURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}