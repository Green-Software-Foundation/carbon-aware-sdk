using CarbonAware.Exceptions;

namespace CarbonAware.Tools.WattTimeClient;

public class WattTimeClientException : CarbonAwareException
{
    public WattTimeClientException(string message) : base(message)
    {
    }
}
