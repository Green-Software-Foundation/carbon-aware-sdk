using Microsoft.Extensions.Configuration;

namespace CarbonAware.WebApi.Configuration;


internal class CarbonExporterConfiguration
{
    public const string Key = "CarbonExporter";

    public int PeriodInHours { get; set; } = 24;

    public void AssertValid()
    {
        if(PeriodInHours <= 0)
        {
            throw new ArgumentException($"The value of CarbonExporter.PeriodInHours must be greater than 0.");
        }
    }
}