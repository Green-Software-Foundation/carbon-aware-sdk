using CarbonAware.DataSources.Configuration;
using CarbonAware.WebApi.IntegrationTests;
using NUnit.Framework;
using System.Net;
using System.Text.Json;

namespace CarbonAware.WepApi.IntegrationTests;

/// <summary>
/// Tests that the Web API controller handles locations instances 
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
[TestFixture(DataSourceType.ElectricityMaps)]
public class LocationsControllerTests : IntegrationTestingBase
{
    private readonly string locationsURI = "/locations";

    public LocationsControllerTests(DataSourceType dataSource) : base(dataSource) { }


    [Test]
    public async Task GetLocations_ReturnsOk()
    {
        var result = await _client.GetAsync(locationsURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.Content, Is.Not.Null);
        using var stream = await result.Content.ReadAsStreamAsync();
        Assert.That(stream, Is.Not.Null);
        var data = await JsonSerializer.DeserializeAsync<IDictionary<string, dynamic>>(stream);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.ContainsKey("eastus"), Is.True);
        Assert.That(data!.ContainsKey("northeurope"), Is.True);
    }
}
