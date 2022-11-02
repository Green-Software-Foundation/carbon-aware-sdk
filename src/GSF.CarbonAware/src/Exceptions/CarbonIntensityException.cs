namespace GSF.CarbonAware.Exceptions;

public class CarbonIntensityException : Exception
{
    public CarbonIntensityException()
    {
    }

    public CarbonIntensityException(string message)
        : base(message)
    {
    }

    public CarbonIntensityException(string message, Exception inner)
        : base(message, inner)
    {
    }
}