using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMapsFree.Model;

/// <summary>
/// Serializable object describing the electricityMap login response object.
/// </summary>
[Serializable]
public record LoginResult
{
    /// <summary>
    /// The Bearer Token used to authenticate future requests.
    /// </summary>
    [JsonPropertyName("Token")]
    public string Token { get; set; } = string.Empty;
}