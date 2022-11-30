using CarbonAware.Interfaces;

namespace CarbonAware;

public class NullEmissionsDataSource : IEmissionsDataSource
{
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        throw new ArgumentException("EmissionsDataSource is not configured");
    }

    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        throw new ArgumentException("EmissionsDataSource is not configured");
    }
}