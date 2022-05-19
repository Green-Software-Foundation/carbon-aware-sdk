namespace CarbonAware.WebApi.Models;

[Serializable]
public record CarbonAwareWebApiError
{
    public string Message { get; set; } = string.Empty;
}