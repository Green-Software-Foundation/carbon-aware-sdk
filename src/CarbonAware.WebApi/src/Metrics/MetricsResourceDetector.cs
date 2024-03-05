using CarbonAware.WebApi.Metrics;
using OpenTelemetry.Resources;

internal class MetricsResourceDetector : IResourceDetector
{
    private readonly CarbonMetrics _carbonMetrics;

    public MetricsResourceDetector(CarbonMetrics carbonMetrics)
    {
        _carbonMetrics = carbonMetrics;
    }

    public Resource Detect()
    {
        return ResourceBuilder.CreateEmpty().Build();
    }
}