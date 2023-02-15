using CarbonAware.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Configuration;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarbonAware.DataSources.ElectricityMaps.Tests;

[TestFixture]
public class ServiceCollectionExtensionTests
{
    const string ForecastDataSourceKey = $"DataSources:ForecastDataSource";
    const string EmissionsDataSourceKey = $"DataSources:EmissionsDataSource";
    const string ForecastDataSourceValue = $"ElectricityMapsTest";
    const string EmissionsDataSourceValue = $"ElectricityMapsTest";
    const string HeaderKey = $"DataSources:Configurations:ElectricityMapsTest:APITokenHeader";
    const string AuthHeader = "auth-token";
    const string TokenKey = $"DataSources:Configurations:ElectricityMapsTest:APIToken";
    const string DefaultTokenValue = "myDefaultToken123";
    const string UseProxyKey = $"DataSources:Configurations:ElectricityMapsTest:Proxy:UseProxy";
    const string ProxyUrl = $"DataSources:Configurations:ElectricityMapsTest:Proxy:Url";
    const string ProxyUsername = $"DataSources:Configurations:ElectricityMapsTest:Proxy:Username";
    const string ProxyPassword = $"DataSources:Configurations:ElectricityMapsTest:Proxy:Password";

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

    [TestCase(true, TestName = "ClientProxyTest, successful: denotes adding ElectricityMaps data sources using proxy.")]
    [TestCase(false, TestName = "ClientProxyTest, successful: denotes adding ElectricityMaps data sources without using proxy.")]
    public void ClientProxy_ConfigurationTest(bool withProxyUrl)
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { EmissionsDataSourceKey, EmissionsDataSourceValue },
            { HeaderKey, AuthHeader },
            { TokenKey, DefaultTokenValue },
            { UseProxyKey, withProxyUrl.ToString() }
        };

        if (withProxyUrl)
        {
            settings.Add(ProxyUrl, "http://fakeProxy");
            settings.Add(ProxyUsername, "proxyUsername");
            settings.Add(ProxyPassword, "proxyPassword");
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act
        var forecastDataSource = serviceCollection.AddElectricityMapsForecastDataSource(configuration.DataSources());
        var emissionsDataSource = serviceCollection.AddElectricityMapsEmissionsDataSource(configuration.DataSources());

        // Assert
        Assert.That(forecastDataSource, Is.Not.Null);
        Assert.That(emissionsDataSource, Is.Not.Null);
    }
}