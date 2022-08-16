
namespace CarbonAware.Tools.ElectricityMapClient.Configuration;

public class ElectricityMapClientConfiguration
{
    public const string Key = "ElectricityMapClient";

    public string? token { get; set; }

    public string BaseUrl { get; set; } = "https://api.co2signal.com/v1/latest";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.token))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.token)} is required for electricityMap.");
        }

        if (!Uri.IsWellFormedUriString(this.BaseUrl, UriKind.Absolute))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.BaseUrl)} is not a valid absolute url.");
        }
    }
}
