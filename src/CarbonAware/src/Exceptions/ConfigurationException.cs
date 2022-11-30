namespace CarbonAware.Exceptions;

/// <summary>
/// Represents a CarbonAware project exception.
/// </summary>
public class ConfigurationException : CarbonAwareException
{
    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    public ConfigurationException() : base()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConfigurationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused the conversion exception.</param>
    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}