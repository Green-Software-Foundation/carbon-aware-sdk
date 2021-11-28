using CarbonAware;
using CarbonAware.Config;
using CarbonAware.Data;
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
        

        private CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
        private CarbonAwareCore _carbonAwareCore;
        private ServiceManager _serviceManager;

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
                _serviceManager = new ServiceManager(_state.ConfigPath);

                var plugin = _serviceManager.ServiceProvider.GetService<ICarbonAwarePlugin>();

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
