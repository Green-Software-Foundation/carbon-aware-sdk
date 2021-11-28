using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace CarbonAware.Config
{
    public class ServiceManager
    {
        private const string PLUGINS_FOLDER = "plugins";

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

        public void Initialize()
        {
            List<ServiceRegistration> configuredServices = _configManager.GetServiceConfiguration();
            LoadPluginAssemblies();
            CreateServiceProvider(configuredServices);
            ConfigureServices(configuredServices);
        }

        public void ConfigureServices(List<ServiceRegistration> services)
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
            var registeredServiceName = registeredService.GetType().Name;
            var configSection = _configManager.GetConfigurationSection(registeredServiceName);
            registeredService.Configure(configSection);
        }

        public void CreateServiceProvider(List<ServiceRegistration> services)
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

        public void LoadPluginAssemblies()
        {
            // An aggregate catalog that combines multiple catalogs.
            var catalog = new AggregateCatalog();

            // Adds all the parts found in the same assembly as the Program class.
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));

            // Add all the parts found in the "plugins" folder
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory + PLUGINS_FOLDER));

            // Create the CompositionContainer with the parts in the catalog.
            _container = new CompositionContainer(catalog);
            _container.ComposeParts(this);

            // Plugins folder DLL's are now loaded!
        }

        

        public static void AddService(IServiceCollection serviceCollection, ServiceRegistration service)
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
}
