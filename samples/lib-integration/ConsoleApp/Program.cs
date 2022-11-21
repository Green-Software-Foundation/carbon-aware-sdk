using GSF.CarbonAware.Configuration;
using GSF.CarbonAware.Models;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

Console.WriteLine("Hello, ConsoleApp Emissions Sample!");

var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", optional: true)
        .Build();

var serviceCollection = new ServiceCollection();
var serviceProvider = serviceCollection.AddLogging()
    .AddEmissionsServices(configuration)
    .BuildServiceProvider();
var handlerEmissions = serviceProvider.GetRequiredService<IEmissionsHandler>();

const string startDate = "2022-03-01T15:30:00Z";
const string endDate = "2022-03-01T18:30:00Z";
const string location = "eastus";

var parsedStart = DateTimeOffset.Parse(startDate);
var parsedEnd = DateTimeOffset.Parse(endDate);

// EmissionsData
var emissionsDataResult = await handlerEmissions.GetEmissionsDataAsync(location, parsedStart, parsedEnd);
var rating = emissionsDataResult.First().Rating;
Console.WriteLine($"For location {location} Starting at: {startDate} Ending at: {endDate} the Emissions Rating is: {rating}.");

// BestEmissionsData
var bestEmissionsDataResult = await handlerEmissions.GetBestEmissionsDataAsync(new string[]{location}, parsedStart, parsedEnd);
var bestRating = bestEmissionsDataResult.First().Rating;
Console.WriteLine($"For location {location} Starting at: {startDate} Ending at: {endDate} the best Emissions Rating is: {bestRating}.");

// Average Carbon Intensity
var averageCarbonIntensityResult = await handlerEmissions.GetAverageCarbonIntensityAsync(location, new TimeRange(parsedStart, parsedEnd));
var average = averageCarbonIntensityResult.CarbonIntensityDataPoints.First().Value;
Console.WriteLine($"For location {location} Starting at: {startDate} Ending at: {endDate} the Average Emissions Rating is: {average}.");

try
{
    // Example call for Forecast on a try/catch block since JSON data source doesn't support it.
    // This is to verify that it builds and in case there is data source that supports Forecast, perform the call.
    serviceCollection = new ServiceCollection();
    serviceProvider = serviceCollection.AddLogging()
        .AddForecastServices(configuration)
        .BuildServiceProvider();

    var handlerForecast = serviceProvider.GetRequiredService<IForecastHandler>();
    var forecasts = await handlerForecast.GetCurrentForecastAsync(new string[] { location });
    foreach (var forecast in forecasts)
    {
        Console.WriteLine($"Forecast GeneratedAt: {forecast.GeneratedAt} ");
        Console.WriteLine($"Forecast RequestedAt: {forecast.RequestedAt} ");
        Console.WriteLine("EmissionsDataPoints");
        Array.ForEach(forecast.EmissionsDataPoints.ToArray(), Console.WriteLine);
        Console.WriteLine("OptimalDataPoints");
        Array.ForEach(forecast.OptimalDataPoints.ToArray(), Console.WriteLine);
    }
}
catch (ArgumentException ex)
{
    // Ignore since it is not supported.
    Console.WriteLine($"Got exception {ex.Message}. Ignoring it");
}
