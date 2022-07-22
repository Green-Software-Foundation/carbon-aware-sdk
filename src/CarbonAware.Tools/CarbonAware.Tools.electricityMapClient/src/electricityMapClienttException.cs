using CarbonAware.Interfaces;
using System;

namespace CarbonAware.Tools.electricityMapClient;

public class electricityMapClientException : Exception
{
    public electricityMapClientException(string message) : base(message)
    {
    }
}
