using Microsoft.AspNetCore.Http;

namespace CarbonAware;

/// <summary>
/// Carbon Aware Variables bindings
/// </summary>
public class CarbonAwareVariablesConfiguration
{
    /// <summary>
    /// The Key containing the configuration values.
    /// </summary>
    public const string Key = "CarbonAwareVars";

    /// <summary>
    /// Gets or sets the route prefix to use for all web api routes.
    /// </summary>
    public PathString WebApiRoutePrefix { get; set; }

    /// <summary>
    /// Gets or sets the forecast data source to use.
    /// </summary>
    public string ForecastDataSource { get; set; }

    /// <summary>
    /// Gets or sets the emissions data source to use.
    /// </summary>
    public string EmissionsDataSource { get; set; }

    #nullable enable
    /// <summary>
    /// Gets or sets proxy information for making calls to the internet.
    /// </summary>
    public WebProxyConfiguration? Proxy { get; set; }
    #nullable disable

    public string TelemetryProvider { get; set; }

    public Boolean VerboseApi {get; set;}

}
