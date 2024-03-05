using Microsoft.Extensions.Configuration;

namespace CarbonAware.Configuration;

internal class EmissionsDataCacheConfiguration
{
    public const string Key = "EmissionsDataCache";

    public bool Enabled { get; set; } = false;

    public int ExpirationMin { get; set; } = 0;

    public void AssertValid()
    {
        if(Enabled & ExpirationMin <= 0)
        {
            throw new ArgumentException($"Expiration period for data cache value must be greater than 0.");
        }
    }
}