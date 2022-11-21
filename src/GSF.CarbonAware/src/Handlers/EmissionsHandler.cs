using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Emissions;
using CarbonAware.Exceptions;
using GSF.CarbonAware.Models;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonAware.Handlers;

internal sealed class EmissionsHandler : IEmissionsHandler
{
    private readonly ILogger<EmissionsHandler> _logger;
    private readonly IEmissionsAggregator _aggregator;

    /// <summary>
    /// Creates a new instance of the <see cref="EmissionsHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger for the handler</param>
    /// <param name="aggregator">An <see cref="IEmissionsAggregator"> aggregator.</param>
    public EmissionsHandler(ILogger<EmissionsHandler> logger, IEmissionsAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        return await GetEmissionsDataAsync(new string[] { location }, start, end);
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var parameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations
        };
        try
        {
            var emissionsData = await _aggregator.GetEmissionsDataAsync(parameters);
            var result = emissionsData.Select(e => (EmissionsData)e);
            return result;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var parameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations
        };
        try
        {
            var emissionsData = await _aggregator.GetBestEmissionsDataAsync(parameters);
            var result = emissionsData.Select(e => (EmissionsData)e);
            return result;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<CarbonIntensity> GetAverageCarbonIntensityAsync(string location, TimeRange requestedTimeRange)
    {
        return await GetAverageCarbonIntensityAsync(location, new TimeRange[] { requestedTimeRange });
    }
    /// <inheritdoc />
    public async Task<CarbonIntensity> GetAverageCarbonIntensityAsync(string location, TimeRange[] requestedTimeRanges)
    {
        var data = new List<CarbonIntensityData>();
        foreach (var timeRange in requestedTimeRanges)
        {
            var start = timeRange.StartTime;
            var end = timeRange.EndTime;
            var parameters = new CarbonAwareParametersBaseDTO
            {
                Start = start,
                End = end,
                SingleLocation = location
            };
            try
            {
                var value = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
                data.Add(new CarbonIntensityData(start, end, value));

                _logger.LogDebug("calculated average carbon intensity is {value} for time range {start} - {end}", value, start, end);
            }
            catch (CarbonAwareException ex)
            {
                throw new Exceptions.CarbonAwareException(ex.Message, ex);
            }
        }
        return new CarbonIntensity()
        {
            Location = location,
            CarbonIntensityDataPoints = data
        };
    }
}
