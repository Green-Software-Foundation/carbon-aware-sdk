using Microsoft.Extensions.Configuration;

namespace CarbonAware.Configuration;

internal class EmissionsDataCacheConfiguration
{
    public const string Key = "EmissionsDataCache";

    public bool Enabled { get; set; }

    public int ExpirationMin { get; set; }

    public void AssertValid()
    {
        if(ExpirationMin <= 0)
        {
            throw new ArgumentException($"Expiration period for data cache value must be greater than 0.");
        }
    }
}