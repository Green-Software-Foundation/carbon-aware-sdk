using Microsoft.Extensions.Configuration;

namespace CarbonAware.Configuration;
public class DataSourcesConfiguration
{
    public const string Key = "DataSources";

    #nullable enable
    public string? EmissionsDataSource { get; set; }
    public string? ForecastDataSource { get; set; }
    public IConfigurationSection? ConfigurationSection { get; set; }
    #nullable disable

    /// <summary>
    /// Gets the "Type" field for configuration object associated with EmissionsDataSource setting. 
    /// </summary>
    /// <returns>Returns the string value of the associated "Type" field.</returns>
    public string EmissionsConfigurationType()
    {
        return GetConfigurationType(EmissionsDataSource);
    }

    /// <summary>
    /// Gets the "Type" field for configuration object associated with ForecastDataSource setting. 
    /// </summary>
    /// <returns>Returns the string value of the associated "Type" field.</returns>
    public string ForecastConfigurationType()
    {
        return GetConfigurationType(ForecastDataSource);
    }

    /// <summary>
    /// Gets the entire configuration object associated with EmissionsDataSource setting. 
    /// </summary>
    /// <returns>Returns the <see cref="IConfigurationSection"> of the associated EmissionsDataSource.</returns>
    public IConfigurationSection EmissionsConfigurationSection()
    {
        return GetConfigurationSection(EmissionsDataSource);
    }

    /// <summary>
    /// Gets the entire configuration object associated with ForecastDataSource setting. 
    /// </summary>
    /// <returns>Returns the <see cref="IConfigurationSection"> of the associated ForecastDataSource.</returns>
    public IConfigurationSection ForecastConfigurationSection()
    {
        return GetConfigurationSection(ForecastDataSource);
    }

    /// <summary>
    /// Asserts that specified data sources have an associated configuration.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if any data source does not have a configuration section.</exception>
    public void AssertValid()
    {
        if (!string.IsNullOrEmpty(EmissionsDataSource) && !ConfigurationSectionContainsKey(EmissionsDataSource))
        {
            throw new ArgumentException($"Emissions data source value '{EmissionsDataSource}' was not found in 'Configurations'");
        }

        if (!string.IsNullOrEmpty(ForecastDataSource) && !ConfigurationSectionContainsKey(ForecastDataSource))
        {
            throw new ArgumentException($"Forecast data source value '{ForecastDataSource}' was not found in 'Configurations'");
        }
    }

    private string GetConfigurationType(string dataSourceName)
    {
        return ConfigurationSection.GetValue<string>($"{dataSourceName}:Type");   
    }

    private IConfigurationSection GetConfigurationSection(string dataSourceName)
    {
        return ConfigurationSection.GetSection(dataSourceName);
    }

    private bool ConfigurationSectionContainsKey(string key)
    {
        foreach (var subsection in ConfigurationSection.GetChildren())
        {
            if (subsection.Key == key)
            {
                return true;
            }
        }
        return false;
    }
}
