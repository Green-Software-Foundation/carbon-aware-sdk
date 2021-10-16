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

            var ed = ca.GetEmissionsDataForLocationByTime(null, DateTime.Now);

            Console.WriteLine($"Ratings for location is: {ed.Rating}");
        }
    }
}
