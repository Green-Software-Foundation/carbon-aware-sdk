using GSF.CarbonAware.Configuration;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using CarbonAware.Exceptions;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class ServiceCollectionExtensionsTest
{
    [Test]
    public void AddEmissionsServices_ReturnsServices()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "LocationDataSourcesConfiguration:LocationSourceFiles:DataFileLocation", "azure-regions.json" },
            { "LocationDataSourcesConfiguration:LocationSourceFiles:Prefix", "az" },
            { "LocationDataSourcesConfiguration:LocationSourceFiles:Delimiter", "-" },
            { "DataSources:EmissionsDataSource", "Json" },
            { "DataSources:ForecastDataSource", "" },
            { "DataSources:Configurations:Json:Type", "JSON" },
            { "DataSources:Configurations:Json:DataFileLocation", "test-data-azure-emissions.json" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var provider = ServiceCollectionExtensions.AddEmissionsServices(services, configuration);

        // Assert
        Assert.That(provider, Is.Not.Null);
    }

    [Test]
    public void AddForecastServices_ReturnsServices()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "LocationDataSourcesConfiguration:LocationSourceFiles:DataFileLocation", "azure-regions.json" },
            { "LocationDataSourcesConfiguration:LocationSourceFiles:Prefix", "az" },
            { "LocationDataSourcesConfiguration:LocationSourceFiles:Delimiter", "-" },
            { "DataSources:EmissionsDataSource", "" },
            { "DataSources:ForecastDataSource", "WattTime" },
            { "DataSources:Configurations:WattTime:Type", "WattTime" },
            { "DataSources:Configurations:WattTime:Username", "username" },
            { "DataSources:Configurations:WattTime:Password", "password123" },
            { "DataSources:Configurations:WattTime:BaseURL", "https://api2.watttime.org/v2/" },
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var provider = ServiceCollectionExtensions.AddForecastServices(services, configuration);

        // Assert
        Assert.That(provider, Is.Not.Null);
    }

    [Test]
    public void AllServices_ThrowExceptions_WithMissingConfigValues()
    {
        // Arrange
        var services = new ServiceCollection();

                var inMemorySettings = new Dictionary<string, string>
        {
            { "DataSources:EmissionsDataSource", "" },
            { "DataSources:ForecastDataSource", "" },
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => ServiceCollectionExtensions.AddForecastServices(services, configuration));
        Assert.Throws<ConfigurationException>(() => ServiceCollectionExtensions.AddEmissionsServices(services, configuration));
    }
}