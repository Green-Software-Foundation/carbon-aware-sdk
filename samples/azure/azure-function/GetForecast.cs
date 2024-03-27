using GSF.CarbonAware.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;


namespace CarbonAwareFunctions
{
    public class GetForecast
    {
        private readonly IForecastHandler _handler;
        private readonly ILogger<GetForecast> _log;

        public GetForecast(IForecastHandler handler, ILogger<GetForecast> log)
        {
            this._handler = handler;
            this._log = log;
        }

        [Function("GetCurrentForecast")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            //Get the startDate, endDate, location, and duration from the request query if the values are present in the query
            string startDate = req.Query["startdate"];
            string endDate = req.Query["enddate"];
            string location = req.Query["location"];
            string duration = req.Query["duration"];

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
            duration ??= data?.duration;

            try
            {
                var result = await _handler.GetCurrentForecastAsync(new string[] { location }, DateTimeOffset.Parse(startDate), DateTimeOffset.Parse(endDate), int.Parse(duration));

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                //Messages related to incorrect parameter values (ie dates outside of range) are returned in the data section
                //Otherwise send the returned error messages
                if (e.Data.Count > 0)
                    return new BadRequestObjectResult(e.Data);
                else
                    return new BadRequestObjectResult(e.Message);
            }

        }
    }
}
