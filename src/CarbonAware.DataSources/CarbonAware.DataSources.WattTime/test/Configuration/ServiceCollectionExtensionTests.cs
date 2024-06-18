using CarbonAware.Configuration;
using CarbonAware.DataSources.WattTime.Client;
using CarbonAware.DataSources.WattTime.Configuration;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;

namespace CarbonAware.DataSources.WattTime.Tests;

[TestFixture]
class ServiceCollectionExtensionTests
{
    private readonly string ForecastDataSourceKey = $"DataSources:ForecastDataSource";
    private readonly string EmissionsDataSourceKey = $"DataSources:EmissionsDataSource";
    private readonly string ForecastDataSourceValue = $"WattTimeTest";
    private readonly string EmissionsDataSourceValue = $"WattTimeTest";
    private readonly string UsernameKey = $"DataSources:Configurations:WattTimeTest:Username";
    private readonly string Username = "devuser";
    private readonly string PasswordKey = $"DataSources:Configurations:WattTimeTest:Password";
    private readonly string Password = "12345";
    private readonly string ProxyUrl = $"DataSources:Configurations:WattTimeTest:Proxy:Url";
    private readonly string ProxyUsername = $"DataSources:Configurations:WattTimeTest:Proxy:Username";
    private readonly string ProxyPassword = $"DataSources:Configurations:WattTimeTest:Proxy:Password";
    private readonly string UseProxyKey = $"DataSources:Configurations:WattTimeTest:Proxy:UseProxy";

    

    [Test]
    public void ClientProxyTest_With_Missing_ProxyURL_ThrowsException()
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { EmissionsDataSourceKey, EmissionsDataSourceValue },
            { UsernameKey, Username },
            { PasswordKey, Password },
            { UseProxyKey, "true" },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => serviceCollection.AddWattTimeForecastDataSource(configuration.DataSources()));
        Assert.Throws<ConfigurationException>(() => serviceCollection.AddWattTimeEmissionsDataSource(configuration.DataSources()));
    }

    [Test]
    public void ClientProxyTest_With_Invalid_ProxyURL_ThrowsException()
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { EmissionsDataSourceKey, EmissionsDataSourceValue },
            { UsernameKey, Username },
            { PasswordKey, Password },
            { ProxyUrl, "http://fakeproxy:8080" },
            { UseProxyKey, "true" },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWattTimeForecastDataSource(configuration.DataSources());

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IWattTimeClient>();

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetRegionAsync("lat", "long"));
    }

    [TestCase(true, TestName = "ClientProxyTest, successful: denotes adding WattTime data sources using proxy.")]
    [TestCase(false, TestName = "ClientProxyTest, successful: denotes adding WattTime data sources without using proxy.")]
    public void ClientProxyTest_AddsDataSource(bool withProxyUrl)
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { EmissionsDataSourceKey, EmissionsDataSourceValue },
            { UsernameKey, Username },
            { PasswordKey, Password },
            { UseProxyKey, withProxyUrl.ToString() }
        };

        if (withProxyUrl)
        {
            settings.Add(ProxyUrl, "http://10.10.10.1");
            settings.Add(ProxyUsername, "proxyUsername");
            settings.Add(ProxyPassword, "proxyPassword");
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act
        var forecastDataSource = serviceCollection.AddWattTimeEmissionsDataSource(configuration.DataSources());
        var emissionsDataSource = serviceCollection.AddWattTimeForecastDataSource(configuration.DataSources());

        // Assert
        Assert.That(forecastDataSource, Is.Not.Null);
        Assert.That(emissionsDataSource, Is.Not.Null);
    }
}