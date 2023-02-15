using CarbonAware.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Configuration;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarbonAware.DataSources.ElectricityMaps.Tests;

[TestFixture]
public class ServiceCollectionExtensionTests
{
    private readonly string ForecastDataSourceKey = $"DataSources:ForecastDataSource";
    private readonly string EmissionsDataSourceKey = $"DataSources:EmissionsDataSource";
    private readonly string ForecastDataSourceValue = $"ElectricityMapsTest";
    private readonly string EmissionsDataSourceValue = $"ElectricityMapsTest";
    private readonly string HeaderKey = $"DataSources:Configurations:ElectricityMapsTest:APITokenHeader";
    private readonly string AuthHeader = "auth-token";
    private readonly string TokenKey = $"DataSources:Configurations:ElectricityMapsTest:APIToken";
    private readonly string DefaultTokenValue = "myDefaultToken123";
    private readonly string UseProxyKey = $"DataSources:Configurations:ElectricityMapsTest:Proxy:UseProxy";

    [Test]
    public void ClientProxyTest_With_Missing_ProxyURL_ThrowsException()
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { EmissionsDataSourceKey, EmissionsDataSourceValue },
            { HeaderKey, AuthHeader },
            { TokenKey, DefaultTokenValue },
            { UseProxyKey, "true" },
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => serviceCollection.AddElectricityMapsForecastDataSource(configuration.DataSources()));
        Assert.Throws<ConfigurationException>(() => serviceCollection.AddElectricityMapsEmissionsDataSource(configuration.DataSources()));
    }

    [Test]
    public void ClientProxyTest_With_No_ProxyURL_AddsServices()
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { EmissionsDataSourceKey, EmissionsDataSourceValue },
            { HeaderKey, AuthHeader },
            { TokenKey, DefaultTokenValue },
            { UseProxyKey, "false" },
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act
        var forecastService = serviceCollection.AddElectricityMapsForecastDataSource(configuration.DataSources());
        var emissionsService = serviceCollection.AddElectricityMapsEmissionsDataSource(configuration.DataSources());

        // Assert
        Assert.NotNull(forecastService);
        Assert.NotNull(emissionsService);
    }

    [Test]
    public void ClientProxyTest_With_ProxyURL_AddsServices()
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { EmissionsDataSourceKey, EmissionsDataSourceValue },
            { HeaderKey, AuthHeader },
            { TokenKey, DefaultTokenValue },
            { UseProxyKey, "true" },
            { "DataSources:Configurations:ElectricityMapsTest:Proxy:url", "http://10.10.10.1"},
            { "DataSources:Configurations:ElectricityMapsTest:Proxy:username", "proxyUsername"},
            { "DataSources:Configurations:ElectricityMapsTest:Proxy:password", "proxyPassword"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act
        var forecastService = serviceCollection.AddElectricityMapsForecastDataSource(configuration.DataSources());
        var emissionsService = serviceCollection.AddElectricityMapsEmissionsDataSource(configuration.DataSources());

        // Assert
        Assert.NotNull(forecastService);
        Assert.NotNull(emissionsService);
    }
}