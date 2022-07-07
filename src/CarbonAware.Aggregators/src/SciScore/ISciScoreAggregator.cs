using CarbonAware.Model;

namespace CarbonAware.Aggregators.SciScore;

public interface ISciScoreAggregator : IAggregator
{
    /// <summary>
    /// Calculates the average carbon intensity for a given location and time interval.
    /// </summary>
    /// <param name="location">Location object representing the location for the carbon intensity data.</param>
    /// <param name="timeInterval">ISO8601 time interval representing the interval to calculate carbon intensity over.</param>
    /// <returns>An average value over the specified time interval or BadRequest Exception if the location or timeinterval is not specified
    /// <exception cref="ArgumentException">Can be thrown if an invalid location or timeInterval is provided.</exception>
    Task<double> CalculateAverageCarbonIntensityAsync(Location location, string timeInterval);
}