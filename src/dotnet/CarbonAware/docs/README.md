# CarbonAware Plugin Architecture

## What is it   

CarbonAware Plugin allows developers to use different CarbonAware Service Providers either from an in-house development or form an existing already provided. (i.e [WattTime](https://www.wattime.org)) to be incorporated as part of a highler level application (i.e Web Service), therefore there is no need to understand the details of how to talk to that provider.


## How to use it

To consume a Plugin, the application requires to inject into the Registration Service Collection **ICarbonAware** interface with the corresponding Plugin implementation concrete class. (***Note: Subject to change to make it easier to register plugins***). The following example illustrates how to consume the Plugin.

### Registration

**Program.cs**
```csharp

using CarbonAware;
using CarbonAware.Plugin.MyPlugin;

serviceCollection.AddSingleton<ICarbonAware, CarbonAwareMyPlugin>();
....
```

### Usage

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
}
```

## Create a new Plugin

###  Interface - ICarbonAware

