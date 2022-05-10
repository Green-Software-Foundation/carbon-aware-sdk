# CarbonAware Plugin Architecture

## What is it   

CarbonAware Plugin allows developers to use different CarbonAware Service Providers either from an in-house development or form an existing already provided. (i.e [WattTime](https://www.wattime.org), [ElectricityMapping](https://static.electricitymap.org)) to be incorporated as part of a highler level application (i.e Web Service), with the benefit of avoiding all the details to understand how to interact with a specific provider.


## How to use a Plugin

To consume a Plugin, the application requires to inject into the Registration Service Collection **ICarbonAware** interface with the corresponding Plugin implementation concrete class. (***Note: Subject to change to make it easier to register plugins***). The following example illustrates how to consume the Plugin using a Service Collection Extension.

### Registration

**Program.cs**
```csharp

using CarbonAware.Plugins.MyPlugin.Configuration;
..
serviceCollection.TryAddSingleton<ICarbonAware, CarbonAwareMyPlugin>();
....
```

### Invoke Plugin method

Any class that has a reference to a Plugin, can invoke `GetEmissions(..)` method with the list of properties that is interesting to pull over. See `CarbonAwareConstants.cs` for the list of the current properties supported.

**MyModule.cs**
```csharp

public class MyClass {

    private readonly ICarbonAware _plugin;
    ...
    public MyClass(ICarbonAware plugin)
    {
        ...
        _plugin = plugin;
        ...
    }

    public async Task MyMethod()
    {
        ...
        var props = new Dictionary<string, object>();
        params.Add(CarbonAwareConstants.LOCATIONS, new List<string> {
            "eastus"
        });
        var results = await _plugin.GetEmissionsDataAsync(props);
        foreach (var r in results)
        {
            Console.WriteLine($"EmissionsData Location: {r.Location}");
            Console.WriteLine($"EmissionsData Time: {r.Time.ToString()}");
            Console.WriteLine($"EmissionsData Rating: {r.Rating}");
        }
        ...
    }
}
```

## Create a new Plugin

To incorporate a new Plugin, create a new dotnet project with the following steps.

```sh
cd src/dotnet
dotnet new classlib -o CarbonAware.Plugin.MyPlugin
dotnet sln add CarbonAware.Plugin.MyPlugin/CarbonAware.Plugin.MyPlugin.csprj
dotnet add CarbonAware.Plugin.MyPlugin/CarbonAware.Plugin.MyPlugin.csprj reference CarbonAware/CarbonAware.csproj
```
###  Implement Interface - ICarbonAware

Given the fact all the registration of the implementation are using DI, the following steps adds the package to the new project.

```sh
cd CarbonAware.Plugin.MyPlugin
dotnet add package Microsoft.Extensions.DependencyInjection
```

Now create the implementation of `ICarbonAware`

```csharp
...
using CarbonAware;
using CarbonAware.Model;

namespace CarbonAware.Plugin.MyPlugin;

public class MyPlugin : ICarbonAware
{

    private readonly ILogger<MyPuglin> _logger;
    public MyPuglin(ILogger<MyPlugin> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        // code specific.
    }
}
```

Create Service Collection Extension under `Configuration`

```sh
mkdir CarbonAware.Plugin.MyPlugin/Configuration
```

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.Plugins.MyPlugin.Configuration;

public static class ServiceCollectionExtensions 
{
    public static void AddCarbonAwareServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ICarbonAware, MyPlugin>();
    }
}
```

Now build the project or the solution

```sh
cd ..
dotnet build
```

### Add Unit Tests

Implement unit tests for the new Plugin to have coverage of the functionality. Below are the steps on how to add one

```sh
cd src/dotnet
dotnet new nunit -o CarbonAware.Plugin.MyPlugin.Tests
dotnet sln CarbonAwareSDK.sln add CarbonAware.Plugin.MyPlugin.Tests/CarbonAware.Plugin.MyPlugin.Tests.csproj
dotnet add CarbonAware.Plugin.MyPlugin.Tests/CarbonAware.Plugin.MyPlugin.csproj  reference CarbonAware.Plugin.MyPlugin/CarbonAware.Plugin.MyPlugin.csproj
```

Add `Moq` if needed

```sh
cd CarbonAware.Plugin.MyPlugin.Tests
dotnet add package Moq
```

After these steps, the skeleton of the unit test is ready, then build and test the project.

```sh
dotnet build
dotnet test
...
 CarbonAware.Plugins.MyPlugin -> /<workspace>/src/dotnet/CarbonAware.Plugins.MyPlugin/bin/Debug/net6.0/CarbonAware.Plugins.MyPlugin.dll
 ...
 Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14, Duration: 413 ms
```

### Configuring LogLevels

The default LogLevel settings for the application are found in the corresponding `appsettings.json`, which may contain the following section -- see here for additional details on [Logging in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) and on [Logging Providers in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging-providers)

```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
```

To permanently change the LogLevel, just update the `appsettings.json` for the app.
To override a LogLevel at runtime, an environment variable can set the LogLevel value. 
For example to set the Logging:LogLevel:Default LogLevel to Debug: `export Logging__LogLevel__Default="Debug"` 

Example using the CLI:

```sh
cd src/dotnet/CarbonAware.CLI
export Logging__LogLevel__Default="Debug"
dotnet run -l westus
```

Example using the WebApp:

```sh
cd src/dotnet/CarbonAware.WebApi
export Logging__LogLevel__Default="Debug"
dotnet run
```

Or, to change the LogLevel for just one run of the app:

```sh
cd src/dotnet/CarbonAware.WebApi
Logging__LogLevel__Default="Debug" dotnet run
```
