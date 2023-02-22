using CarbonAware.DataSources.ElectricityMapsFree.Model;

namespace CarbonAware.DataSources.ElectricityMapsFree.Client;

/// <summary>
/// An interface for interacting with the Electricity Maps Free API, also called CO2 Signal.
/// </summary>
public interface IElectricityMapsFreeClient
{
    public const string NamedClient = "ElectricityMapsFreeClient";

    /// <summary>
    /// Async method to get the latest emission data for a given country code
    /// </summary>
    public Task<GridEmissionDataPoint> GetCurrentEmissionsAsync(string countryCodeAbbreviation);

    /// <summary>
    /// Async method to get the latest emission data for a given zone
    /// </summary>
    public Task<GridEmissionDataPoint> GetCurrentEmissionsAsync(Zone zone);

    /// <summary>
    /// Async method to get the latest emission data for a given latitude and longitude
    /// </summary>
    public Task<GridEmissionDataPoint> GetCurrentEmissionsAsync(string latitude, string longitude);

}
