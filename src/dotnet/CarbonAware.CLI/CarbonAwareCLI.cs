using CarbonAware;
using CarbonAware.Plugins.BasicJsonPlugin;
using CarbonAwareCLI.Config;
using CarbonAwareCLI.Options;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CarbonAwareCLI
{

    public class CarbonAwareCLI
    {
        private CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
        private CarbonAwareCore _carbonAwareCore;
        private ServiceProvider _serviceProvider;

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
                Console.WriteLine("Error: Invalid arguments.");
                Console.WriteLine(e.Message);
            }
        }

        private const string CONFIG_SECTION_SERVICE_REGISTRATIONS = "service-registrations";
        private const string CONFIG_SERVICES_ARRAY = "services";

        private void ConfigureServices()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile(_state.ConfigPath);

            var config = builder.Build();
            var section = config.GetSection(CONFIG_SECTION_SERVICE_REGISTRATIONS);
            var services = section.GetSection(CONFIG_SERVICES_ARRAY).Get<List<ServiceRegistration>>();

            var serviceCollection = new ServiceCollection()
               .AddLogging();

            foreach (var service in services)
            {
                var serviceType = Type.GetType(service.service, true);
                var serviceImplementation = Type.GetType(service.implementation);

                serviceCollection.AddSingleton(serviceType, serviceImplementation);
            }

            _serviceProvider = serviceCollection.BuildServiceProvider();

            foreach (var service in services)
            {
                var serviceType = Type.GetType(service.service, true);
                var registeredService = _serviceProvider.GetService(serviceType) as IConfigurable;
                var registeredServiceName = registeredService.GetType().Name;
                var configSection = config.GetSection(registeredServiceName);
                registeredService.Configure(configSection);
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
