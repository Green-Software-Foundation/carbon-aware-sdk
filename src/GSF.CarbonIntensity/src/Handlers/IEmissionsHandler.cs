using CarbonAware.Aggregators.CarbonAware;

namespace GSF.CarbonIntensity.Handlers
{
    public interface IEmissionsHandler
    {
        /// <summary>
        /// Retrieves the measured carbon intensity data for the given location between the time boundaries and calculates the average carbon intensity during that period. 
        /// </summary>
        /// <remarks> This function is useful for reporting the measured carbon intensity for a specific time period in a specific location. </remarks>
        /// <param name="parameters">The request object <see cref="CarbonAwareParameters"/> with `Start`, `End` and `SingleLocation` fields set.</param>
        /// <returns>The average carbon intensity value.</returns>
        Task<double> GetAverageCarbonIntensity(CarbonAwareParameters parameters);
    }
}