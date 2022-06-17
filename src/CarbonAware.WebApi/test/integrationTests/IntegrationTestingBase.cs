using CarbonAware.DataSources.Configuration;
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
public abstract class IntegrationTestingBase
{
    internal DataSourceType _dataSource;
    internal string? _dataSourceEnv;
    internal WebApplicationFactory<Program> _factory;
    protected HttpClient _client;
    protected IDataSourceMocker _dataSourceMocker;
    private readonly MediaTypeHeaderValue _mediaType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IntegrationTestingBase(DataSourceType dataSource)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _dataSource = dataSource;
        _dataSourceEnv = null;
        _factory = new WebApplicationFactory<Program>();
    }


    public string ConstructDateQueryURI(string Url, string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Use HTTP Query builder
        var builder = new UriBuilder();

        //Add all query parameters
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["locations"] = location;
        query["time"] = $"{start:O}";
        query["toTime"] = $"{end:O}";

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
        _dataSourceEnv = Environment.GetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource");

        //Switch between different data sources as needed
        //Each datasource should have an accompanying DataSourceMocker that will perform setup activities
        switch (_dataSource)
        {
            case DataSourceType.JSON:
                {
                    Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "JSON");
                    _dataSourceMocker = new JsonDataSourceMocker();
                    break;
                }
            case DataSourceType.WattTime:
                {
                    Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "WattTime");
                    _dataSourceMocker = new WattTimeDataSourceMocker();
                    break;
                }
            case DataSourceType.None:
                {
                    throw new NotSupportedException($"DataSourceType {_dataSource.ToString()} not supported");
                }
        }

        //Setup the WebAppFactory with custom settings as required by the datasource
        //For instance, overriding specific clients with new URLs.
        _factory = _dataSourceMocker.OverrideWebAppFactory(_factory);
        _client = _factory.CreateClient();
    }

    [SetUp]
    public void SetupTests()
    {
        _dataSourceMocker.Initialize();
    }

    [TearDown]
    public void ResetTests()
    {
        _dataSourceMocker.Reset();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        _dataSourceMocker.Dispose();
        Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", _dataSourceEnv);
    }
}