namespace CarbonAware;

public class WebProxyConfiguration
{

    /// <summary>
    /// Gets or sets a value that determines whether or not a proxy will be used.
    /// </summary>
    public bool UseProxy { get; set; }

    /// <summary>
    /// Gets or sets the proxy url
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Sets the proxy username
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Sets the proxy password
    /// </summary>
    public string Password { get; set; }
}
