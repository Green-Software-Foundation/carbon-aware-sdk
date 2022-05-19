namespace CarbonAware.Tools.WattTimeClient.Configuration;

/// <summary>
/// An exception class thrown when the WattTime client is misconfigured.
/// </summary>
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
    }
}
