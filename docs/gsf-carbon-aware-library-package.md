# GSF CarbonAware .NET Library Package

## Dependency Injection

In order to get access to the [handlers](./architecture/c%23-client-library.md#handlers) in the library, using a common practice with C# is through `Microsoft.Extensions.DependencyInjection` extensions. This way the whole life cycle of the handler instance is managed by the container’s framework, and it would help to isolate the concrete implementation from the user facing interface. For instance, a consumer would be able to call extensions as:
```c#
// Using DI Services (Emissions) to register GSF SDK library
services.AddEmissionsServices(configuration);
```
```c#
// An application Consumer construct should inject a GSF handler like the following example
class ConsumerClass(IEmissionsHandler handler, ILogger<ConsumerClass> logger)
{
    ....
    this._handler = handler;
    this._logger = logger;
    ....
}
```

And the usage of a method for IEmissionsHandler

```c#
async Task<double> GetRating()
{
    ...
    return await this._handler.GetAverageCarbonIntensity(…);
}
```
Another functionality of the application could just do Forecast data. So, it would be a matter of following the same pattern:

```c#
// Using DI Services (Forecast) to register GSF SDK library
services.AddForecastServices(configuration);
```

```c#
class ForecastApp(IForecastHandler handler)
{
    ...
    this._handler = handler;
}
```
And the usage of a method for IForecastHandler:

```c#
async Task<EmissionsData> GetOptimal(…)
{
    ...
    return await this._handler.GetCurrentAsync()...).OptimalDataPoints.First();
}
```

This way it would fit within the same stack as the rest of the SDK is implemented. Also, it would be easier to integrate later when the current consumers (CLI/WebApi) should be moved to use the library.
