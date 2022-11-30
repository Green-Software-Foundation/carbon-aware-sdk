namespace CarbonAware.Exceptions;

/// <summary>
/// Represents a CarbonAware project exception.
/// </summary>
public class CarbonAwareException : Exception
{
    /// <summary>
    /// Creates a new instance of the <see cref="CarbonAwareException"/> class.
    /// </summary>
    public CarbonAwareException() : base()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CarbonAwareException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CarbonAwareException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CarbonAwareException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused the conversion exception.</param>
    public CarbonAwareException(string message, Exception innerException) : base(message, innerException)
    {
    }
}