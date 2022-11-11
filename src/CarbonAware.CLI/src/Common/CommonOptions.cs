using System.CommandLine;

namespace CarbonAware.CLI.Common
{
    internal class CommonOptions
    {
        public static readonly Option<string[]> RequiredLocationOption = new Option<string[]>(
            new string[] { "--location", "-l" }, 
            CommonLocalizableStrings.LocationDescription)
            {
                IsRequired = true,
                Arity = ArgumentArity.OneOrMore
            };
    }
}
