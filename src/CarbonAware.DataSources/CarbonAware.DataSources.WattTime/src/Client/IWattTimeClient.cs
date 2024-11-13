using CarbonAware.DataSources.WattTime.Model;

namespace CarbonAware.DataSources.WattTime.Client;

/// <summary>
/// An interface for interacting with the WattTime API.
/// </summary>
internal interface IWattTimeClient
{
    public const string NamedClient = "WattTimeClient";
    public const string NamedAuthenticationClient = "WattTimeAuthenticationClient";

    /// <summary>
    /// Async method to get observed emission data for a given region and time period.
    /// </summary>
    /// <param name="regionAbbreviation">Region abbreviation</param>
    /// <param name="startTime">Start time of the time period</param>
    /// <param name="endTime">End time of the time period</param>
    /// <returns>An <see cref="Task{IEnumerable}{GridEmissionDataPoint}"/> which contains all emissions data points in a period.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<GridEmissionsDataResponse> GetDataAsync(string regionAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime);

    /// <summary>
    /// Async method to get observed emission data for a given region and time period.
    /// </summary>
    /// <param name="region">Region</param>
    /// <param name="startTime">Start time of the time period</param>
    /// <param name="endTime">End time of the time period</param>
    /// <returns>An <see cref="Task{IEnumerable}{GridEmissionDataPoint}"/> which contains all emissions data points in a period.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<GridEmissionsDataResponse> GetDataAsync(RegionResponse region, DateTimeOffset startTime, DateTimeOffset endTime);

    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given region.
    /// </summary>
    /// <param name="regionAbbreviation">region abbreviation</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<ForecastEmissionsDataResponse> GetCurrentForecastAsync(string regionAbbreviation);

    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given region.
    /// </summary>
    /// <param name="region">region</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<ForecastEmissionsDataResponse> GetCurrentForecastAsync(RegionResponse region);

    /// <summary>
    /// Async method to get generated forecast at requested time and region.
    /// </summary>
    /// <param name="region">region abbreviation</param>
    /// <param name="requestedAt">The historical time used to fetch the most recent forecast generated as of that time.</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points or null if no Forecast generated at the requested time.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<HistoricalForecastEmissionsDataResponse?> GetForecastOnDateAsync(string region, DateTimeOffset requestedAt);

    /// <summary>
    /// Async method to get generated forecast at requested time and region.
    /// </summary>
    /// <param name="region">region</param>
    /// <param name="requestedAt">The historical time used to fetch the most recent forecast generated as of that time.</param>
    /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points or null if no Forecast generated at the requested time.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<HistoricalForecastEmissionsDataResponse?> GetForecastOnDateAsync(RegionResponse region, DateTimeOffset requestedAt);

    /// <summary>
    /// Async method to get the region for a given location.
    /// </summary>
    /// <param name="latitude">Latitude of the location</param>
    /// <param name="longitude">Longitude of the location</param>
    /// <returns>An <see cref="Task{region}"/> which contains the region details.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<RegionResponse> GetRegionAsync(string latitude, string longitude);

    /// <summary>
    /// Async method to get the region abbreviation for a given location.
    /// </summary>
    /// <param name="latitude">Latitude of the location</param>
    /// <param name="longitude">Longitude of the location</param>
    /// <returns>A string which contains the region details.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<string?> GetRegionAbbreviationAsync(string latitude, string longitude);

    /// <summary>
    /// Async method to get binary data (representing a zip file) of the historical emissions data for the given region.
    /// </summary>
    /// <param name="regionAbbreviation">region abbreviation</param>
    /// <returns>An <see cref="Task{Stream}"/> which contains the binary data stream of the .zip file.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<Stream> GetHistoricalDataAsync(string regionAbbreviation);

    /// <summary>
    /// Async method to get binary data (representing a zip file) of the historical emissions data for the given region.
    /// </summary>
    /// <param name="region">region</param>
    /// <returns>An <see cref="Task{Stream}"/> which contains the data Stream of the .zip file.</returns>
    /// <exception cref="WattTimeClientException">Can be thrown when errors occur connecting to WattTime client.  See the WattTimeClientException class for documentation of expected status codes.</exception>
    Task<Stream> GetHistoricalDataAsync(RegionResponse region);
}
