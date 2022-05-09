using CarbonAware.Tools.WattTimeClient.Model;

namespace CarbonAware.Tools.WattTimeClient;

/// <summary>
/// An interface for interacting with the WattTime API.
/// </summary>
public interface IWattTimeClient
{
    /// <summary>
    /// Async method to get observed emission data for a given balancing authority and time period.
    /// </summary>
    /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
    /// <param name="startTime">Start time of the time period</param>
    /// <param name="endTime">End time of the time period</param>
    /// <returns>An <see cref="Task{IEnumerable}{GridEmissionDataPoint}"/> which contains all emissions data points in a period.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(string balancingAuthorityAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);

    /// <summary>
    /// Async method to get observed emission data for a given balancing authority and time period.
    /// </summary>
    /// <param name="balancingAuthority">Balancing authority</param>
    /// <param name="startTime">Start time of the time period</param>
    /// <param name="endTime">End time of the time period</param>
    /// <returns>An <see cref="Task{IEnumerable}{GridEmissionDataPoint}"/> which contains all emissions data points in a period.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(BalancingAuthority balancingAuthority, DateTimeOffset startTime, DateTimeOffset endTime);

    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given balancing authority.
    /// </summary>
    /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<Forecast?> GetCurrentForecastAsync(string balancingAuthorityAbbreviation);

    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given balancing authority.
    /// </summary>
    /// <param name="balancingAuthority">Balancing authority</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<Forecast?> GetCurrentForecastAsync(BalancingAuthority balancingAuthority);

    /// <summary>
    /// Async method to get all generated forecasts in the given period and balancing authority.
    /// </summary>
    /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
    /// <param name="startTime">Start time of the time period</param>
    /// <param name="endTime">End time of the time period</param>
    /// <returns>An <see cref="Task{IEnumerable}{Forecast}"/> which contains all forecast sets generated in the given period.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<IEnumerable<Forecast>> GetForecastByDateAsync(string balancingAuthorityAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);

    /// <summary>
    /// Async method to get all generated forecasts in the given period and balancing authority.
    /// </summary>
    /// <param name="balancingAuthority">Balancing authority</param>
    /// <param name="startTime">Start time of the time period</param>
    /// <param name="endTime">End time of the time period</param>
    /// <returns>An <see cref="Task{IEnumerable}{Forecast}"/> which contains all forecast sets generated in the given period.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<IEnumerable<Forecast>> GetForecastByDateAsync(BalancingAuthority balancingAuthority, DateTimeOffset startTime, DateTimeOffset endTime);

    /// <summary>
    /// Async method to get the balancing authority for a given location.
    /// </summary>
    /// <param name="latitude">Latitude of the location</param>
    /// <param name="longitude">Longitude of the location</param>
    /// <returns>An <see cref="Task{BalancingAuthority}"/> which contains the balancing authority details.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<BalancingAuthority> GetBalancingAuthorityAsync(string latitude, string longitude);

    /// <summary>
    /// Async method to get the balancing authority abbreviation for a given location.
    /// </summary>
    /// <param name="latitude">Latitude of the location</param>
    /// <param name="longitude">Longitude of the location</param>
    /// <returns>A string which contains the balancing authority details.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<string?> GetBalancingAuthorityAbbreviationAsync(string latitude, string longitude);

    /// <summary>
    /// Async method to get binary data (representing a zip file) of the historical emissions data for the given balancing authority.
    /// </summary>
    /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
    /// <returns>An <see cref="Task{Stream}"/> which contains the binary data stream of the .zip file.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<Stream> GetHistoricalDataAsync(string balancingAuthorityAbbreviation);

    /// <summary>
    /// Async method to get binary data (representing a zip file) of the historical emissions data for the given balancing authority.
    /// </summary>
    /// <param name="balancingAuthority">Balancing authority</param>
    /// <returns>An <see cref="Task{Stream}"/> which contains the data Stream of the .zip file.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    public Task<Stream> GetHistoricalDataAsync(BalancingAuthority balancingAuthority);
}
