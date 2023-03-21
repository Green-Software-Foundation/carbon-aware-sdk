using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.WattTime;

internal class WattTimeClientException : CarbonAwareException
{
    public WattTimeClientException(string message) : base(message)
    {
    }
}
