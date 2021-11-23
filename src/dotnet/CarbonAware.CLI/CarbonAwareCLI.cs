using CarbonAware;
using CarbonAwareCLI.Config;
using CarbonAwareCLI.Options;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace CarbonAwareCLI
{
    public class CarbonAwareCLI
    {
        private const string CONFIG_SECTION_SERVICE_REGISTRATIONS = "service-registrations";
        private const string CONFIG_SERVICES_ARRAY = "services";
        private const string PLUGINS_FOLDER = "plugins";

        private CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
        private CarbonAwareCore _carbonAwareCore;
        private ServiceProvider _serviceProvider;
        private CompositionContainer _container;

        /// <summary>
        /// Indicates if the command line arguments have been parsed successfully 
        /// </summary>
        public bool Parsed { get; private set; } = false;

        public CarbonAwareCLI(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<CLIOptions>(args);

            try
            {
                // Parse command line parameters
                parseResult.WithParsed(ValidateCommandLineArguments);
                parseResult.WithNotParsed(ThrowOnParseError);

                // Configure the services
                ConfigureServices();

                var plugin = _serviceProvider.GetService<ICarbonAwarePlugin>();

                // Create the new core using the plugin
                _carbonAwareCore = new CarbonAwareCore(plugin);

                Parsed = true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Configures the service provider 
        /// </summary>
        private void ConfigureServices()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(_state.ConfigPath);

            var config = builder.Build();
            var section = config.GetSection(CONFIG_SECTION_SERVICE_REGISTRATIONS);
            var services = section.GetSection(CONFIG_SERVICES_ARRAY).Get<List<ServiceRegistration>>();
                
            ValidateServiceSyntax(services);

            LoadPluginAssemblies();

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


        private void LoadPluginAssemblies()
        {
            // An aggregate catalog that combines multiple catalogs.
            var catalog = new AggregateCatalog();
            // Adds all the parts found in the same assembly as the Program class.
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            
            // Add all the parts found in the "plugins" folder
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory + PLUGINS_FOLDER));

            // Create the CompositionContainer with the parts in the catalog.
            _container = new CompositionContainer(catalog);
            _container.ComposeParts(this);

            // Plugins folder DLL's are now loaded!
        }

        private void ValidateServiceSyntax(List<ServiceRegistration> services)
        {
            if (services == null) throw new ArgumentException($"Configuration file '{_state.ConfigPath}' is invalid.  Could not find services.");
            foreach (var service in services)
            {
                if (service.service is null || service.implementation is null)
                {
                    throw new ArgumentException($"Service configuration is invalid.  Service: '{service.service}', Implementation: '{service.implementation}'");
                }
            }
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

        /// <summary>
        /// Handles missing messages.  Currently reports the message tag as an argument exception.
        /// This method needs updating to add detailed "Missing parameter" messages
        /// </summary>
        /// <param name="errors"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ThrowOnParseError(IEnumerable<Error> errors)
        {
            var enumerator = errors.GetEnumerator();

            if (enumerator.MoveNext())
            {
                throw new ArgumentException(enumerator.Current.Tag.ToString());
            }

            // TODO: add error message builder such as
            //var builder = SentenceBuilder.Create();
            //var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
            //var excList = errorMessages.Select(msg => new ArgumentException(msg)).ToList();
            //if (excList.Any())
            //    throw new AggregateException(excList);
        }

        public List<EmissionsData> GetEmissions()
        {
            List<EmissionsData> foundEmissions = new List<EmissionsData>();

            if (_state.Lowest)
            {
                foundEmissions = _carbonAwareCore.GetBestEmissionsDataForLocationsByTime(_state.Locations, _state.Time, _state.ToTime);
            }
            else
            {
                foundEmissions = _carbonAwareCore.GetEmissionsDataForLocationsByTime(_state.Locations, _state.Time, _state.ToTime);
            }

            return foundEmissions;
        }

        public void OutputEmissionsData(List<EmissionsData> emissions)
        {
            Console.WriteLine($"{JsonConvert.SerializeObject(emissions, Formatting.Indented)}");
        }

        private void ValidateCommandLineArguments(CLIOptions o)
        {
            // -v --verbose 
            ParseVerbose(o);

            // -t --time --toTime
            ParseTime(o);

            // --lowest
            ParseLowest(o);

            // -c --config
            ParseConfigPath(o);

            // -l --locations
            ParseLocations(o);
        }

        #region Parse Options 

        private void ParseLocations(CLIOptions o)
        {
            _state.Locations.AddRange(o.Location);
        }

        private void ParseLowest(CLIOptions o)
        {
            _state.Lowest = o.Lowest;
        }

        private void ParseVerbose(CLIOptions o)
        {
            if (o.Verbose)
            {
                _state.Verbose = true;
            }
        }

        private void ParseConfigPath(CLIOptions o)
        {
            if (o.ConfigPath is not null)
            {
                if (!File.Exists(o.ConfigPath))
                {
                    throw new ArgumentException($"File '{o.ConfigPath}' could not be found.");
                }
                _state.ConfigPath = o.ConfigPath;
            }
        }

        private void ParseTime(CLIOptions o)
        {
            // Validate time arguments
            if (o.Time is null)
            {
                _state.TimeOption = TimeOptionStates.Time;
                _state.Time = DateTime.Now;
            }
            else if (o.Time is not null)
            {
                _state.TimeOption = TimeOptionStates.Time;
                try
                {
                    _state.Time = DateTime.Parse(o.Time);
                }
                catch
                {
                    throw new ArgumentException(
                        $"Date and time needs to be in the format 'xxxxx'.  Date and time provided was '{o.Time}'.");
                }
            }

            if (o.ToTime is not null)
            {
                _state.TimeOption = TimeOptionStates.TimeWindow;

                try
                {
                    _state.ToTime = DateTime.Parse(o.ToTime);
                }
                catch
                {
                    throw new ArgumentException(
                        $"Date and time needs to be in the format 'xxxxx'.  Date and time provided was '{o.ToTime}'.");
                }
            }
        }

        #endregion
    }
}
