namespace CarbonAwareCLI;

class Program
{
    static void Main(string[] args)
    {
        var cli = new CarbonAwareCLI(args);
        if (cli.Parsed)
        {
            var emissions = cli.GetEmissions();
            cli.OutputEmissionsData(emissions);
        }
    }
}
