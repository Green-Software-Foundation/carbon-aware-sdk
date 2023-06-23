using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Configuration;
internal class DataSourcesConfiguration
{
    public const string Key = "DataSources";

    #nullable enable
    public string? EmissionsDataSource { get; set; }
    public string? ForecastDataSource { get; set; }
    public IConfigurationSection? Section { get; set; }
    #nullable disable

    /// <summary>
    /// Gets the "Type" field for configuration object associated with a setting by data source interface type.
    /// </summary>
    /// <returns>Returns the string value of the associated "Type" field.</returns>
    public string ConfigurationType<T>() where T : IDataSource
    {
        if (typeof(T) == typeof(IEmissionsDataSource))
        {
            return EmissionsConfigurationType();
        }
        else if (typeof(T) == typeof(IForecastDataSource))
        {
            return ForecastConfigurationType();
        }
        else
        {
            throw new ArgumentException($"Data source interface type '{typeof(T)}' is not supported.");
        }
    }

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
    /// Gets the entire configuration object associated with a setting by data source interface type. 
    /// </summary>
    /// <returns>Returns the <see cref="IConfigurationSection"> of the associated data source interface.</returns>
    public IConfigurationSection ConfigurationSection<T>() where T : IDataSource
    {
        if (typeof(T) == typeof(IEmissionsDataSource))
        {
            return EmissionsConfigurationSection();
        }
        else if (typeof(T) == typeof(IForecastDataSource))
        {
            return ForecastConfigurationSection();
        }
        else
        {
            throw new ArgumentException($"Data source interface type '{typeof(T)}' is not supported.");
        }
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
        return Section.GetValue<string>($"{dataSourceName}:Type");   
    }

    private IConfigurationSection GetConfigurationSection(string dataSourceName)
    {
        return Section.GetSection(dataSourceName);
    }

    private bool ConfigurationSectionContainsKey(string key)
    {
        foreach (var subsection in Section.GetChildren())
        {
            if (subsection.Key == key)
            {
                return true;
            }
        }
        return false;
    }
}
