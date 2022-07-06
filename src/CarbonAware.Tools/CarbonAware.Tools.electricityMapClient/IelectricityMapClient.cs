using CarbonAware.Tools.electricityMapClient.Model;

namespace CarbonAware.Tools.electricityMapClient;

/// <summary>
/// An interface for interacting with the WattTime API.
/// </summary>
public interface IelectricityMapClient
{
    public const string NamedClient = "electricityMapClient";
    
    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given balancing authority.
    /// </summary>
    /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<Forecast?> GetCurrentForecastAsync(string countryCodeAbbreviation);

    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given balancing authority.
    /// </summary>
    /// <param name="balancingAuthority">Balancing authority</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<Forecast?> GetCurrentForecastAsync(Zone zone);

    // TODO: For Commercial methods not implemented yet
    //public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(string countryCodeAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(Zone zone, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<Forecast>> GetForecastByDateAsync(string countryCodeAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<IEnumerable<Forecast>> GetForecastByDateAsync(Zone zone, DateTimeOffset startTime, DateTimeOffset endTime);
    //public Task<Stream> GetHistoricalDataAsync(string countryCodeAbbreviation);
    //public Task<Stream> GetHistoricalDataAsync(Zone zone);


}
