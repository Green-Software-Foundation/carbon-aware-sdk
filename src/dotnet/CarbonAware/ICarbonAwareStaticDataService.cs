namespace CarbonAware;

/// <summary>
/// Interface for a static data services that simply gives a list 
/// of emissions data for client side processing.
/// </summary>
public interface ICarbonAwareStaticDataService : IConfigurable
{
    /// <summary>
    /// Retrieve all data as a List for client side processing.
    /// </summary>
    /// <returns>A List&lt;EmissionsData&gt; of all data available.</returns>
    public List<EmissionsData> GetData();

}
