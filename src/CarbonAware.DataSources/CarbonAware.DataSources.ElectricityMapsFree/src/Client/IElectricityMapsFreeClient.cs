using CarbonAware.DataSources.ElectricityMapsFree.Model;

namespace CarbonAware.DataSources.ElectricityMapsFree.Client;

/// <summary>
/// An interface for interacting with the Electricity Maps Free API, also called CO2 Signal.
/// </summary>
public interface IElectricityMapsFreeClient
{
    public const string NamedClient = "ElectricityMapsFreeClient";

    /// <summary>
    /// Async method to get the latest emission data for a given zone
    /// </summary>
    public Task<Forecast?> GetCurrentForecastAsync(string countryCodeAbbreviation);

    /// <summary>
    /// Async method to get the latest emission data for a given zone
    /// </summary>
    public Task<Forecast?> GetCurrentForecastAsync(Zone zone);

    // TODO: For GetCurrentForecastAsync by latitude and longitude
    public Task<Forecast?> GetCurrentForecastAsync(string latitude, string longitude);

    // TODO: For Commercial Version methods
    //public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(string countryCodeAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(Zone zone, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<Forecast>> GetForecastByDateAsync(string countryCodeAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<Forecast>> GetForecastByDateAsync(Zone zone, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<Stream> GetHistoricalDataAsync(string countryCodeAbbreviation);
    //public Task<Stream> GetHistoricalDataAsync(Zone zone);
}
