using GSF.CarbonAware.Configuration;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class ServiceCollectionExtensionsTest
{
    [Test]
    public void AddEmissionsServices_ReturnsServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

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
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        // Act
        IServiceCollection provider = ServiceCollectionExtensions.AddEmissionsServices(services, configuration);

        // Assert
        Assert.NotNull(provider);
    }

    [Test]
    public void AddForecastServices_ReturnsServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

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
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        IServiceCollection provider = ServiceCollectionExtensions.AddForecastServices(services, configuration);

        // Assert
        Assert.NotNull(provider);
    }    
}