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
    /// Gets or sets the the carbon intensity data source to use.
    /// </summary>
    public string CarbonIntensityDataSource { get; set; }

    #nullable enable
    public WebProxyConfiguration? Proxy { get; set; }
    #nullable disable

}
