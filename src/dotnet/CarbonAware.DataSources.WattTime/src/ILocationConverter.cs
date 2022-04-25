using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient.Model;

namespace CarbonAware.DataSources.WattTime;

/// <summary>
/// Represents a WattTime location converter.
/// </summary>
public interface ILocationConverter
{
    /// <summary>
    /// Converts a location to a balancing authority for WattTime.
    /// </summary>
    /// <param name="location">The location to convert.</param>
    /// <returns>The balancing authority to use.</returns>
    /// <exception cref="LocationConversionException">Thrown when the given location can't be converted to a balancing authority.</exception>
    public Task<BalancingAuthority> ConvertLocationToBalancingAuthorityAsync(Location location);
}
