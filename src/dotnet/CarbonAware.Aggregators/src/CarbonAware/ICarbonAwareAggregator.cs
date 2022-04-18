using CarbonAware.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Aggregators.CarbonAware
{
    public interface ICarbonAwareAggregator : IAggregator
    {
        /// <summary>
        /// Returns emissions data records.
        /// </summary>
        /// <param name="props">IDictionary with properties required by concrete classes</param>
        /// <returns>An IEnumerable instance with EmissionsData instances.</returns>
        Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props);
    }
}