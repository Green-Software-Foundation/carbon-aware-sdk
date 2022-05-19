
namespace CarbonAware.Model
{
    public readonly struct SCIBaseData
    {
        public Guid TaskIdentifier { get; }
        public double EnergyConsumption { get; }
        public double MarginalCarbonEmissions { get; }
        public double EmbodiedEmissions { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }

        public SCIBaseData(Guid taskIdentifier, double energyConsumption, double marginalCarbonEmissions, double embodiedEmissions, DateTime start, DateTime end)
        {
            TaskIdentifier = taskIdentifier;
            EnergyConsumption = energyConsumption;
            MarginalCarbonEmissions = marginalCarbonEmissions;
            EmbodiedEmissions = embodiedEmissions;
            StartTime = start;
            EndTime = end;
        }
    }
}
