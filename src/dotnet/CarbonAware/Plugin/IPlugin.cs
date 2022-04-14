using System.Collections;

namespace CarbonAware.Plugin;

public interface IPlugin
{
    string Name { get; }
    string Description { get; }
    string Author { get; }
    string Version { get; }

    /// <summary>
    /// Returns emissions data records.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes</param>
    /// <returns>An IEnumerable instance with EmissionsData instances.</returns>
    Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props);
}