using CarbonAware.WebApi.Configuration;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;

namespace CarbonAware.WebApi.Metrics;

internal class CarbonMetrics : IDisposable
{
    private readonly ILogger<CarbonMetrics> _logger;

    private readonly IOptionsMonitor<CarbonExporterConfiguration> _configurationMonitor;
    private CarbonExporterConfiguration _configuration => this._configurationMonitor.CurrentValue;
    internal const string MeterName = "Carbon.Aware.Metric";
    internal const string ActivitySourceName = "Carbon.Aware.Metric";
    internal const string GaugeName = "carbon.aware.intensity";

    public ActivitySource ActivitySource { get; }

    private readonly IEmissionsHandler _emissionsHandler;
    private readonly ILocationHandler _locationHandler;

    private readonly Meter _meter;

    private readonly ConcurrentBag<string> _locations = new ConcurrentBag<string>();
    private readonly IDictionary<string, ObservableGauge<double>> _gauges = new ConcurrentDictionary<string, ObservableGauge<double>>();

    public CarbonMetrics(IMeterFactory meterFactory, IOptionsMonitor<CarbonExporterConfiguration> monitor, ILogger<CarbonMetrics> logger, IEmissionsHandler emissionsHandler, ILocationHandler locationHandler)
    {
        _emissionsHandler = emissionsHandler ?? throw new ArgumentNullException(nameof(emissionsHandler));
        _locationHandler = locationHandler ?? throw new ArgumentNullException(nameof(locationHandler));
        _configurationMonitor = monitor;
        _logger = logger;
        _configuration.AssertValid();

        string? version = typeof(CarbonMetrics).Assembly.GetName().Version?.ToString();
        ActivitySource = new ActivitySource(ActivitySourceName, version);
        _meter = meterFactory.Create(MeterName, version);
        InitLocations();
    }

    private void InitLocations()
    {
        // initialize locations and guages
        _locations.Clear();
        _gauges.Clear();

        // load locations
        Task<IDictionary<string, Location>> locationsTask = _locationHandler.GetLocationsAsync();
        try
        {
            locationsTask.Result.Keys.ToList().ForEach(d => _locations.Add(d));
            // create guages for each locaton
            foreach(var loc in _locations){
                _gauges[loc] = _meter.CreateObservableGauge(CarbonMetrics.GaugeName, () => GetIntensity(loc));
            }
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex.Message);
            _locations.Clear();
            _gauges.Clear();
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
        ActivitySource.Dispose();
    }

    private Measurement<double> GetIntensity(string location){
        try
        {
            var end = DateTimeOffset.UtcNow;
            var start = end.AddHours(-1*_configuration.PeriodInHours);
            var intensity = _emissionsHandler.GetEmissionsDataAsync(location, start, end)
                .Result
                .MaxBy(d => d.Time)!
                .Rating;
            var measurement = new Measurement<double>(intensity, new TagList(){{"location", location}});
            return measurement;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return new Measurement<double>(0, new TagList(){{"location", location}});
        }
    }
}