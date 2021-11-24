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
        private const string CONFIG_SECTION_SERVICE_REGISTRATIONS = "service-registrations";
        private const string CONFIG_SERVICES_ARRAY = "services";
        private const string PLUGINS_FOLDER = "plugins";

        private ServiceProvider _serviceProvider;
        private CompositionContainer _container;

        private string _configPath;

        public ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public ServiceManager(string configPath)
        {
            _configPath = configPath;

            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(_configPath);


            var config = builder.Build();
            var section = config.GetSection(CONFIG_SECTION_SERVICE_REGISTRATIONS);
            var services = section.GetSection(CONFIG_SERVICES_ARRAY).Get<List<ServiceRegistration>>();

            ValidateServiceSyntax(services);
            LoadPluginAssemblies();
            RegisterServices(services, config);
        }

        private void RegisterServices(List<ServiceRegistration> services, IConfigurationRoot config)
        {
            var serviceCollection = new ServiceCollection()
                            .AddLogging();

            // Register Services
            foreach (var service in services)
            {
                AddService(serviceCollection, service);
            }

            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Configure Services
            foreach (var service in services)
            {
                var serviceType = Type.GetType(service.service, true);
                var registeredService = _serviceProvider.GetService(serviceType) as IConfigurable;
                var registeredServiceName = registeredService.GetType().Name;
                var configSection = config.GetSection(registeredServiceName);
                registeredService.Configure(configSection);
            }
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

        public void ValidateServiceSyntax(List<ServiceRegistration> services)
        {
            if (services == null) throw new ArgumentException($"Configuration file '{_configPath}' is invalid.  Could not find services.");
            foreach (var service in services)
            {
                if (service.service is null || service.implementation is null)
                {
                    throw new ArgumentException($"Service configuration is invalid.  Service: '{service.service}', Implementation: '{service.implementation}'");
                }
            }
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
