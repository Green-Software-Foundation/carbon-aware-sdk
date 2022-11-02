namespace GSF.CarbonAware.Handlers
{
    public interface IEmissionsHandler
    {
        /// <summary>
        /// Retrieves the measured carbon intensity data for the given location between the time boundaries and calculates the average carbon intensity during that period. 
        /// </summary>
        /// <remarks> This function is useful for reporting the measured carbon intensity for a specific time period in a specific location. </remarks>
        /// <param name="location">The location name where workflow is run (ex: eastus)</param>
        /// <param name="start">The time at which the workflow we are measuring carbon intensity for started (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">The time at which the workflow we are measuring carbon intensity for ended (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>The average carbon intensity value.</returns>
        Task<double> GetAverageCarbonIntensityAsync(string location, DateTimeOffset start, DateTimeOffset end);
    }
}
