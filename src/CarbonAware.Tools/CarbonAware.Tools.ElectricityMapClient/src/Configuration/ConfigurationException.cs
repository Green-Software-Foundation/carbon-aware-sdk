namespace CarbonAware.Tools.ElectricityMapClient.Configuration;

/// <summary>
/// An exception class thrown when the electricityMap client is misconfigured.
/// </summary>
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
    }
}
