using CarbonAware.Exceptions;
using CarbonAware.DataSources.ElectricityMapsFree.Constants;
using CarbonAware.DataSources.ElectricityMapsFree.Model;

namespace CarbonAware.DataSources.ElectricityMapsFree.Configuration;

public class ElectricityMapsFreeClientConfiguration
{
    public const string Key = "ElectricityMapsFreeClient";

    public string? Token { get; set; }

    public string BaseUrl { get; set; } = "https://api.co2signal.com/v1/";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.Token))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.Token)} is required for electricityMaps free.");
        }

        if (!Uri.IsWellFormedUriString(this.BaseUrl, UriKind.Absolute))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.BaseUrl)} is not a valid absolute url.");
        }
    }
}