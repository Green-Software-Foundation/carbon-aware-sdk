using GSF.CarbonAware.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace function
{
    public class GetCarbonIntensity
    {
        private readonly IEmissionsHandler _handler;

        public GetCarbonIntensity(IEmissionsHandler handler)
        {
            this._handler = handler;
        }


        [FunctionName("GetAverageCarbonIntensity")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //Get the startDate, endDate, and location from the request query if the values are present in the query
            string startDate = req.Query["startdate"];
            string endDate = req.Query["enddate"];
            string location = req.Query["location"];

            //If the parameters are includes in the body of the request, read the values from the request body
            string requestBody = String.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            dynamic data = JsonSerializer.Deserialize<object>(requestBody);

            startDate ??= data?.startDate;
            endDate ??= data?.endDate;
            location ??= data?.location;


            try
            {
                var result = await _handler.GetAverageCarbonIntensityAsync(location, DateTimeOffset.Parse(startDate), DateTimeOffset.Parse(endDate));
                log.LogInformation($"For location {location} Starting at: {startDate} Ending at: {endDate} the Average Emissions Rating is: {result}.");

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                //Messages related to incorrect parameter values (ie dates outside of range) are returned in the data section
                //Otherwise send the returned error messages
                if (e.Data.Count>0)
                    return new BadRequestObjectResult(e.Data);
                else 
                    return new BadRequestObjectResult(e.Message);
            }

        }
    }
}
