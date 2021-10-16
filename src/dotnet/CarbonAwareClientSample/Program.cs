using CarbonAware;
using CarbonAwareLogicPluginSample;
using System;

namespace CarbonAwareClientSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var ca = new CarbonAwareCore(new CarbonAwareLogicSamplePlugin());

            ca.GetEmissionsDataForLocationByTime(null, DateTime.Now);

        }
    }
}
