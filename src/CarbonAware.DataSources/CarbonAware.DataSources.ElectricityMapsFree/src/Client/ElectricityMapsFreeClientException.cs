using CarbonAware.Interfaces;
using System;

namespace CarbonAware.DataSources.ElectricityMapsFree.Client;

internal class ElectricityMapsFreeClientException : Exception
{
    public ElectricityMapsFreeClientException(string message) : base(message)
    {
    }
}
