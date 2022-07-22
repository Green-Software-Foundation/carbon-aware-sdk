using CarbonAware.Tools.electricityMapClient.Model;

namespace CarbonAware.Tools.electricityMapClient;

/// <summary>
/// An interface for interacting with the electricityMap API.
/// </summary>
public interface IelectricityMapClient
{
    public const string NamedClient = "electricityMapClient";

    /// <summary>
    /// Async method to get the latest emission data for a given zone
    /// </summary>
    public Task<Forecast?> GetCurrentForecastAsync(string countryCodeAbbreviation);

    /// <summary>
    /// Async method to get the latest emission data for a given zone
    /// </summary>
    public Task<Forecast?> GetCurrentForecastAsync(Zone zone);

    // TODO: For GetCurrentForecastAsync by latitude and longtitude
    //public Task<Forecast?> GetCurrentForecastAsync(float latitude, float longtitude);

    // TODO: For Commercial Version methods
    //public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(string countryCodeAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(Zone zone, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<Forecast>> GetForecastByDateAsync(string countryCodeAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<Forecast>> GetForecastByDateAsync(Zone zone, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<Stream> GetHistoricalDataAsync(string countryCodeAbbreviation);
    //public Task<Stream> GetHistoricalDataAsync(Zone zone);
}
