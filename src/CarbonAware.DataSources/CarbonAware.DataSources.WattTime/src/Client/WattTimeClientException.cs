using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.WattTime;

public class WattTimeClientException : CarbonAwareException
{
    public WattTimeClientException(string message) : base(message)
    {
    }
}
