using System;
using System.Collections.Generic;
namespace CarbonAware
{
    /// <summary>
    /// Interface for a static data services that simply gives a list 
    /// of emissions data for client side processing.
    /// </summary>
    public interface ICarbonAwareStaticDataService
    {
        /// <summary>
        /// Retrieve all data as a List for client side processing.
        /// </summary>
        /// <returns>A List&lt;EmissionsData&gt; of all data available.</returns>
        List<EmissionsData> GetData();

        /// <summary>
        /// Loads the data from the location provided
        /// </summary>
        /// <param name="location">Location of the data.  This may vary based on the configured data service, for example, this may be a URL, remote storage location, or relative local file path</param>
        void LoadData(string location);
    }
}
