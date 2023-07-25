using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.WattTime.Model;

/// <summary>
/// Serializable object describing the WattTime login response object.
/// </summary>
[Serializable]
internal record LoginResult
{
    /// <summary>
    /// The Bearer token used to authenticate future requests.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}