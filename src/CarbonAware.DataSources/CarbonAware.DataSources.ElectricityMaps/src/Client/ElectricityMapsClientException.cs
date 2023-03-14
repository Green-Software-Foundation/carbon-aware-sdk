using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.ElectricityMaps.Client;

internal class ElectricityMapsClientException : CarbonAwareException
{
    public ElectricityMapsClientException(string message) : base(message)
    {
    }
}