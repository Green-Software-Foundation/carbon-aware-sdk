using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using CarbonAware.Interfaces;

namespace CarbonAware.Config;

public class ServiceManager
{
    public const string PLUGINS_FOLDER = "plugins";

    private ServiceProvider _serviceProvider;
    private CompositionContainer _container;
    private IConfigManager _configManager;

    public ServiceProvider ServiceProvider
    {
        get { return _serviceProvider; }
    }

    public ServiceManager(IConfigManager configManager)
    {
        _configManager = configManager;
        Initialize();
    }

    private void Initialize()
    {
        List<ServiceRegistration> configuredServices = _configManager.GetServiceConfiguration();
        LoadPluginAssemblies();
        CreateServiceProvider(configuredServices);
        ConfigureServices(configuredServices);
    }

    private void ConfigureServices(List<ServiceRegistration> services)
    {
        // Configure Services
        foreach (var service in services)
        {
            ConfigureService(service);
        }
    }

    private void ConfigureService(ServiceRegistration service)
    {
        var serviceType = Type.GetType(service.service, true);
        var registeredService = _serviceProvider.GetService(serviceType) as IConfigurable;
        LoadServiceConfiguration(registeredService);
    }

    private void LoadServiceConfiguration(IConfigurable registeredService)
    {
        var registeredServiceName = registeredService.GetType().Name;
        var configSection = _configManager.GetConfigurationSection(registeredServiceName);
        registeredService.Configure(configSection);
    }

    private void CreateServiceProvider(List<ServiceRegistration> services)
    {
        var serviceCollection = new ServiceCollection()
                        .AddLogging();
        // Register Services
        foreach (var service in services)
        {
            AddService(serviceCollection, service);
        }
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private void LoadPluginAssemblies()
    {
        // An aggregate catalog that combines multiple catalogs.
        var catalog = new AggregateCatalog();

        // Adds all the parts found in the same assembly as the Program class.
        //catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
        var pluginsFolder = AppDomain.CurrentDomain.BaseDirectory + PLUGINS_FOLDER;
        Console.WriteLine("plugins folder>" + pluginsFolder);
        // If there is no plugins folder, simply return
        if (!Directory.Exists(pluginsFolder)){
            Console.WriteLine("directory doesn't exist");
            return;
        } 

        // Add all the parts found in the "plugins" folder
        catalog.Catalogs.Add(new DirectoryCatalog(pluginsFolder));

        // Create the CompositionContainer with the parts in the catalog.
        _container = new CompositionContainer(catalog);
        _container.ComposeParts(this);

        // Plugins folder DLL's are now loaded!
    }

    private static void AddService(IServiceCollection serviceCollection, ServiceRegistration service)
    {
        try
        {
            var serviceType = Type.GetType(service.service, true);
            var serviceImplementation = Type.GetType(service.implementation);

            serviceCollection.AddSingleton(serviceType, serviceImplementation);
        }
        catch
        {
            throw new ArgumentException($"Error configuring service '${service.service}' with type $'{service.implementation}'");
        }
    }
}
