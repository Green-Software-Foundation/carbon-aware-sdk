using CarbonAware.Interfaces;
using System;

namespace CarbonAware.Tools.WattTimeClient;

public class WattTimeClientException : Exception
{
    public WattTimeClientException(string message) : base(message)
    {
    }
}
