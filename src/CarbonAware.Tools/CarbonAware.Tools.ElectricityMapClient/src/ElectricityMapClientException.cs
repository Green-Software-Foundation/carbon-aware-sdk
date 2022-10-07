using CarbonAware.Interfaces;
using System;

namespace CarbonAware.Tools.ElectricityMapClient;

public class ElectricityMapClientException : Exception
{
    public ElectricityMapClientException(string message) : base(message)
    {
    }
}
