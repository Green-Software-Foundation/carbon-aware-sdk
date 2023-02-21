using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.ElectricityMaps.Client;

public class ElectricityMapsClientException : CarbonAwareException
{
    public ElectricityMapsClientException(string message) : base(message)
    {
    }
}