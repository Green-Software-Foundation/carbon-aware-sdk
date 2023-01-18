namespace GSF.CarbonAware.Exceptions;

public class CarbonAwareException : Exception
{
    public CarbonAwareException()
    {
    }

    public CarbonAwareException(string message)
        : base(message)
    {
    }

    public CarbonAwareException(string message, Exception inner)
        : base(message, inner)
    {
    }
}