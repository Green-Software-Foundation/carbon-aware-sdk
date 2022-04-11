
namespace CarbonAware.Tools.WattTimeClient.Configuration;

/// <summary>
/// A configuration class for holding WattTime client config values.
/// </summary>
public class WattTimeClientConfiguration
{
    public const string Key = "WattTimeClient";

    /// <summary>
    /// Gets or sets the username to use when connecting to WattTime.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password to use when connecting to WattTime
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the base url to use when connecting to WattTime
    /// </summary>
    public string BaseUrl { get; set; } = "https://api2.watttime.org/v2/";

    /// <summary>
    /// Validate that this object is properly configured.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.Username))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.Username)} is required for WattTime.");
        }

        if (string.IsNullOrWhiteSpace(this.Password))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.Password)} is required for WattTime.");
        }

        if (!Uri.IsWellFormedUriString(this.BaseUrl, UriKind.Absolute))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.BaseUrl)} is not a valid absolute url.");
        }
    }
}
