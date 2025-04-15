﻿using CarbonAware.DataSources.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.DataSources.Json.Mocks;
using CarbonAware.DataSources.ElectricityMaps.Mocks;
using CarbonAware.DataSources.WattTime.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Web;

namespace CarbonAware.WebApi.IntegrationTests;

/// <summary>
/// A base class that does all the common setup for the Integration Testing
/// Overrides WebAPI factory by switching out different configurations via _datasource
/// </summary>
internal abstract class IntegrationTestingBase
{
    internal DataSourceType _dataSource;
    internal string? _emissionsDataSourceEnv;
    internal string? _forecastDataSourceEnv;
    internal string? _enableCarbonExporterEnv;
    internal WebApplicationFactory<Program> _factory;
    protected HttpClient _client;
    internal IDataSourceMocker _dataSourceMocker;
    private readonly MediaTypeHeaderValue _mediaType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IntegrationTestingBase(DataSourceType dataSource)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _dataSource = dataSource;
        _emissionsDataSourceEnv = null;
        _forecastDataSourceEnv = null;
        _enableCarbonExporterEnv = null;
        _factory = new WebApplicationFactory<Program>();
    }


    public string ConstructUriWithQueryString(string Url, IDictionary<string, string> queryStringParams)
    {
        // Use HTTP Query builder
        var builder = new UriBuilder();

        //Add all query parameters
        var query = HttpUtility.ParseQueryString(builder.Query);

        foreach (var kvp in queryStringParams)
        {
            query[kvp.Key] = kvp.Value;
        }

        //Generate final query string
        builder.Query = query.ToString();
        builder.Path = Url;

        //These values are blank as they are set by the SDK
        builder.Scheme = "";
        builder.Port = -1;
        builder.Host = "";

        return builder.ToString();
    }

    public async Task<HttpResponseMessage?> PostJSONBodyToURI(object body, string URI)
    {
        var jsonBody = JsonSerializer.Serialize(body);
        StringContent _content = new StringContent(jsonBody);

        _content.Headers.ContentType = _mediaType;

        return await _client.PostAsync(URI, _content);
    }

    [OneTimeSetUp]
    public void Setup()
    {
        _emissionsDataSourceEnv = Environment.GetEnvironmentVariable("DataSources__EmissionsDataSource");
        _forecastDataSourceEnv = Environment.GetEnvironmentVariable("DataSources__ForecastDataSource");
        _enableCarbonExporterEnv = Environment.GetEnvironmentVariable("CarbonAwareVars__EnableCarbonExporter");
        Environment.SetEnvironmentVariable("CarbonAwareVars__EnableCarbonExporter", "true");
        //Switch between different data sources as needed
        //Each datasource should have an accompanying DataSourceMocker that will perform setup activities
        switch (_dataSource)
        {
            case DataSourceType.JSON:
                {
                    Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", "Json");
                    Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", "");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__Json__Type", "Json");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__Json__DataFileLocation", "test-data-azure-emissions.json");
                    Environment.SetEnvironmentVariable("CarbonAwareVars__VerboseApi", "true");
                    _dataSourceMocker = new JsonDataSourceMocker();
                    break;
                }
            case DataSourceType.WattTime:
                {
                    Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", "WattTime");
                    Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", "WattTime");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__WattTime__Username", "username");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__WattTime__Password", "password");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__WattTime__Type", "WattTime");
                    _dataSourceMocker = new WattTimeDataSourceMocker();
                    break;
                }
            case DataSourceType.ElectricityMaps:
                {
                    Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", "ElectricityMaps");
                    Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", "ElectricityMaps");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMaps__Type", "ElectricityMaps");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMaps__APITokenHeader", "token");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMaps__APIToken", "test");

                    _dataSourceMocker = new ElectricityMapsDataSourceMocker();
                    break;
                }
            case DataSourceType.None:
                {
                    Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", "");
                    Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", "");
                    break;
                }
        }

        // After initializing and configuring the data source, we can now create the client from our factory
        _client = _factory.CreateClient();
    }

    [SetUp]
    public void SetupTests()
    {
        if (_dataSource == DataSourceType.JSON)
        {
            // To force WebApplication to consume new JSON datasorce it needs to be restarted.
            // This is a direct result of JSON caching in the CatbonAware code 
            _factory.Dispose();
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        _dataSourceMocker?.Initialize();
    }

    [TearDown]
    public void ResetTests()
    {
        _dataSourceMocker?.Reset();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        _dataSourceMocker?.Dispose();
        Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", _emissionsDataSourceEnv);
        Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", _forecastDataSourceEnv);
        Environment.SetEnvironmentVariable("CarbonAwareVars__EnableCarbonMetrics", _enableCarbonExporterEnv);
    }
}