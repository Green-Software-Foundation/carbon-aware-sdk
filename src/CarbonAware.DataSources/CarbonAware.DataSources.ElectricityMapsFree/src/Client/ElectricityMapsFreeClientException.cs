using CarbonAware.Interfaces;
using System;

namespace CarbonAware.DataSources.ElectricityMapsFree.Client;

public class ElectricityMapsFreeClientException : Exception
{
    public ElectricityMapsFreeClientException(string message) : base(message)
    {
    }
}
