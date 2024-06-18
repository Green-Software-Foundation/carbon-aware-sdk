using CarbonAware.DataSources.WattTime.Constants;
using CarbonAware.DataSources.WattTime.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CarbonAware.DataSources.WattTime.Client.Tests;

internal static class TestData
{
    public class TestDataConstants
    {
        public const string Region = "TEST_REGION";
        public const string RegionFullName = "Test Region Full Name";
        public const string Market = "mkt";
        public static DateTimeOffset GeneratedAt = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static DateTimeOffset PointTime = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static DateTime Date = new DateTime(2099, 1, 1, 0, 0, 0);
        public const float Value = 999.99f;
        public const string Version = "1.0";
        public const string SignalType = SignalTypes.co2_moer;
    }

    internal static string GetGridDataResponseJsonString()
    {
        return JsonSerializer.Serialize(_GetGridDataResponse());
    }
    private static GridEmissionsDataResponse _GetGridDataResponse()
    {
        var gridEmissionDataResponse = new GridEmissionsDataResponse()
        {
            Meta = _GetGridDataMetaResponse(),
            Data = _GetGridEmissionDataPoints()
        };
        return gridEmissionDataResponse;
    }

    private static GridEmissionsMetaData _GetGridDataMetaResponse()
    {
        var gridEmissionsMetaData = new GridEmissionsMetaData()
        {
            Region = TestDataConstants.Region,
            GeneratedAt = TestDataConstants.GeneratedAt,
            GeneratedAtPeriodSeconds = 30,
            Model = new GridEmissionsModelData()
            {
                Date = TestDataConstants.Date,
                Type = SignalTypes.co2_moer
            },
            DataPointPeriodSeconds = 30,
            SignalType = SignalTypes.co2_moer,
            Units = "co2_moer"
        };

        return gridEmissionsMetaData;
    }
    private static List<GridEmissionDataPoint> _GetGridEmissionDataPoints()
    {
        return new List<GridEmissionDataPoint>()
        {
            _GetGridEmissionDataPoint()
        };
    }

    private static GridEmissionDataPoint _GetGridEmissionDataPoint()
    {
        return new GridEmissionDataPoint()
        {
            Frequency = 300,
            Market = TestDataConstants.Market,
            PointTime = TestDataConstants.PointTime,
            Value = TestDataConstants.Value,
            Version = TestDataConstants.Version
        };
    }

    internal static string GetCurrentForecastJsonString()
    {
        return JsonSerializer.Serialize(_GetCurrentForecastEmissionsDataResponse());
    }

    private static ForecastEmissionsDataResponse _GetCurrentForecastEmissionsDataResponse()
    {
        return new ForecastEmissionsDataResponse()
        {
            Data = _GetGridEmissionDataPoints(),
            Meta = _GetGridDataMetaResponse()
        };
    }


    internal static string GetHistoricalForecastDataJsonString()
    {
        return JsonSerializer.Serialize(_GetHistoricalForecastEmissionsDataResponse());
    }

    private static HistoricalForecastEmissionsDataResponse _GetHistoricalForecastEmissionsDataResponse()
    {
        return new HistoricalForecastEmissionsDataResponse()
        {
            Meta = _GetGridDataMetaResponse(),
            Data = new List<HistoricalEmissionsData>
            {
                new HistoricalEmissionsData()
                {
                    Forecast = _GetGridEmissionDataPoints(),
                    GeneratedAt = TestDataConstants.GeneratedAt
                }
            }
        };
    }

    internal static string GetRegionJsonString()
    {
        return JsonSerializer.Serialize(_GetRegion());
    }

    private static RegionResponse _GetRegion()
    {
        return new RegionResponse()
        {
            Region = TestDataConstants.Region,
            RegionFullName = TestDataConstants.RegionFullName,
            SignalType = SignalTypes.co2_moer
        };
    }
}