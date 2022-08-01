using System.Text.Json.Serialization;

namespace CarbonAware.Tools.WattTimeClient.Model;

/// <summary>
/// Serializable object describing the WattTime login response object.
/// </summary>
[Serializable]
public record LoginResult
{
    /// <summary>
    /// The Bearer token used to authenticate future requests.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}