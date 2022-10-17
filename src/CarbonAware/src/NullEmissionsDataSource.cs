using CarbonAware.Interfaces;
using CarbonAware.Model;

namespace CarbonAware;

public class NullEmissionsDataSource : IEmissionsDataSource
{
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        return Task.FromResult(Enumerable.Empty<EmissionsData>());
    }

    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        return Task.FromResult(Enumerable.Empty<EmissionsData>());
    }
}