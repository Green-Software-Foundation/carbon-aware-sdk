using CarbonAware.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.CLI.Model;
class EmissionsForecastDTO
{
    public DateTimeOffset GeneratedAt { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Location { get; set; } = string.Empty;
    public DateTimeOffset DataStartAt { get; set; }
    public DateTimeOffset DataEndAt { get; set; }
    public int WindowSize { get; set; }
    public IEnumerable<EmissionsDataDTO>? OptimalDataPoints { get; set; }
    public IEnumerable<EmissionsDataDTO>? ForecastData { get; set; }

    public static explicit operator EmissionsForecastDTO(EmissionsForecast emissionsForecast)
    {
        EmissionsForecastDTO forecast = new()
        {
            GeneratedAt = emissionsForecast.GeneratedAt,
            Location = emissionsForecast.Location.Name!,
            DataStartAt = emissionsForecast.DataStartAt,
            DataEndAt = emissionsForecast.DataEndAt,
            WindowSize = (int)emissionsForecast.WindowSize.TotalMinutes,
            OptimalDataPoints = emissionsForecast.OptimalDataPoints.Select(d => (EmissionsDataDTO)d),
            ForecastData = emissionsForecast.ForecastData.Select(d => (EmissionsDataDTO)d)!,
            RequestedAt = emissionsForecast.RequestedAt
        };
        return forecast;
    }
 }