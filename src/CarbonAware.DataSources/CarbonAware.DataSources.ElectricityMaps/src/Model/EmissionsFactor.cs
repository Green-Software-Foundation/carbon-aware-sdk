using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMaps.Model;

/// <summary>
/// Type of EmissionFactor to use for calculating carbon intensity as described in the Electricity Maps documentation - https://static.electricitymaps.com/api/docs/index.html#emission-factors
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmissionsFactor
{
    Lifecycle,
    Direct
}
