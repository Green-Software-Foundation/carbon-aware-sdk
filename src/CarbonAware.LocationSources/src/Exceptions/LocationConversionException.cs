using CarbonAware.Exceptions;
using System;

namespace CarbonAware.LocationSources.Exceptions;

/// <summary>
/// Represents a conversion exception.
/// </summary>
public class LocationConversionException : CarbonAwareException
{
    /// <summary>
    /// Creates a new instance of the <see cref="LocationConversionException"/> class.
    /// </summary>
    public LocationConversionException() : base()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LocationConversionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public LocationConversionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LocationConversionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused the conversion exception.</param>
    public LocationConversionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
