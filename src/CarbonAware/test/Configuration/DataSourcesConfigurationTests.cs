using CarbonAware.Configuration;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Tests.Configuration;

class DataSourcesConfigurationTests
{
    [TestCase("WattTime", "", TestName = "AssertValid: Emissions valid, Forecast empty")]
    [TestCase("WattTime", null, TestName = "AssertValid: Emissions valid, Forecast null")]
    [TestCase("WattTime", "WattTime", TestName = "AssertValid: Emissions valid, Forecast valid")]
    [TestCase("", "WattTime", TestName = "AssertValid: Forecast valid, Emissions empty")]
    [TestCase(null, "WattTime", TestName = "AssertValid: Forecast valid, Emissions null")]
    [TestCase(null, null, TestName = "AssertValid: Forecast null, Emissions null")]

    public void AssertValid_WithAtleastOneDataSourceConfigured_DoesNotThrowException(string? emissionsDataSource, string? forecastDataSource)
    {
        //Arrange
        var inMemorySettings = new Dictionary<string, string>() {{
            "Configurations:WattTime", "" 
        }};

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        DataSourcesConfiguration dataSourceConfig = new()
        {
            EmissionsDataSource = emissionsDataSource,
            ForecastDataSource = forecastDataSource,
            Section = configuration.GetSection("Configurations")
        };
        
        Assert.DoesNotThrow(() => dataSourceConfig.AssertValid());
    }

    [TestCase("WattTime","badkey","Emissions data source", TestName = "AssertValid: EmissionsDataSourceNotInConfigurationSection") ]
    [TestCase("badkey", "WattTime", "Forecast data source", TestName = "AssertValid: ForecastDataSourceNotInConfigurationSection")]

    public void AssertValid_DataSourceNotInConfigurationSection_ThrowsException(string forecastDataSource, string emissionsDataSource, string errorMessage)
    {
        //Arrange
        var inMemorySettings = new Dictionary<string, string>() {{
            "Configurations:WattTime", ""
        }};

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        DataSourcesConfiguration dataSourceConfig = new()
        {
            EmissionsDataSource = emissionsDataSource,
            ForecastDataSource = forecastDataSource,
            Section = configuration.GetSection("Configurations")
        };
        var ex = Assert.Throws<ArgumentException>(() => dataSourceConfig.AssertValid());
        Assert.That(ex!.Message, Contains.Substring(errorMessage));
    }

    [TestCase("WattTime", "Json", TestName = "Assert Configuration Type is forecast=WattTime and emissions=Json")]
    [TestCase("WattTime", "ElectricityMaps", TestName = "Assert Configuration Type is forecast=WattTime and emissions=ElectricityMaps")]
    [TestCase("Json", null, TestName = "Assert Configuration Type is forecast=Json and emissions=null")]
    public void ConfigurationTypesAndSections_ReturnCorrectType(string forecastDataSource, string emissionsDataSource)
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>()
        {
            { "Configurations:WattTime:Type", "WattTime" },
            { "Configurations:Json:Type", "Json" },
            { "Configurations:ElectricityMaps:Type", "ElectricityMaps" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var dataSourceConfig = new DataSourcesConfiguration()
        {
            EmissionsDataSource = emissionsDataSource,
            ForecastDataSource = forecastDataSource,
            Section = configuration.GetSection("Configurations")
        };
        
        // Act
        var emissionsType = dataSourceConfig.EmissionsConfigurationType();
        var forecastType = dataSourceConfig.ForecastConfigurationType();
        var emissionsSection = dataSourceConfig.EmissionsConfigurationSection();
        var forecastSection = dataSourceConfig.ForecastConfigurationSection();

        // Assert
        Assert.That(emissionsDataSource, Is.EqualTo(emissionsType));
        Assert.That(forecastDataSource, Is.EqualTo(forecastType));
        Assert.That(emissionsSection.Value, Is.EqualTo(configuration.GetSection("Configurations").Value));
        Assert.That(forecastSection.Value, Is.EqualTo(configuration.GetSection("Configurations").Value));
    }
}
