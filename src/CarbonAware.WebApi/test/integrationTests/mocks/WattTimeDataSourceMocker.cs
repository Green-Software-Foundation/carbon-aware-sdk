using CarbonAware.DataSources.Configuration;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace CarbonAware.WebApi.IntegrationTests;
public class WattTimeDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;
    private readonly object _dataSource = DataSourceType.WattTime;

    internal WattTimeDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Initialize();
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        var data = new List<GridEmissionDataPoint>();
        DateTimeOffset pointTime = start;
        TimeSpan duration = TimeSpan.FromSeconds(300);
        GridEmissionDataPoint newDataPoint = WattTimeServerMocks.GetDefaultEmissionsDataPoint();
        newDataPoint.PointTime = pointTime;
        newDataPoint.BalancingAuthorityAbbreviation = location;
        newDataPoint.Frequency = (int)duration.TotalSeconds;

        data.Add(newDataPoint);
        pointTime = newDataPoint.PointTime + duration;

        while (pointTime < end)
        {
            newDataPoint = WattTimeServerMocks.GetDefaultEmissionsDataPoint();
            newDataPoint.PointTime = pointTime;
            newDataPoint.BalancingAuthorityAbbreviation = location;
            newDataPoint.Frequency = (int)duration.TotalSeconds;

            data.Add(newDataPoint);
            pointTime = newDataPoint.PointTime + duration;
        }

        WattTimeServerMocks.SetupDataMock(_server, data);
    }

    public WebApplicationFactory<Program> OverrideWebAppFactory(WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<WattTimeClientConfiguration>(configOpt =>
                {
                    configOpt.BaseUrl = _server.Url!;
                });
            });
        });
    }

    public void Initialize()
    {
        WattTimeServerMocks.WattTimeServerSetupMocks(_server);
    }

    public void Reset()
    {
        _server.Reset();
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}