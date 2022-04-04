# CarbonAware Plugin Architecture

## What is it   

CarbonAware Plugin allows developers to use different CarbonAware Service Providers either from an in-house development or form an existing already provided. (i.e [WattTime](https://www.wattime.org)) to be incorporated as part of a highler level application (i.e Web Service), with the benefit to avoid all the details to understand the details of how to interact with a specific provider.


## How to use a Plugin

To consume a Plugin, the application requires to inject into the Registration Service Collection **ICarbonAware** interface with the corresponding Plugin implementation concrete class. (***Note: Subject to change to make it easier to register plugins***). The following example illustrates how to consume the Plugin.

### Registration

**Program.cs**
```csharp

using CarbonAware;
using CarbonAware.Plugin.MyPlugin;

serviceCollection.AddSingleton<ICarbonAware, CarbonAwareMyPlugin>();
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

    public void MyMethod()
    {
        ...
        var props = new Dictionary<string, object>();
        params.Add(CarbonAwareConstants.LOCATIONS, new List<string> {
            "eastus"
        });
        var results = _plugin.GetEmissions(props);
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
###  Interface - ICarbonAware

