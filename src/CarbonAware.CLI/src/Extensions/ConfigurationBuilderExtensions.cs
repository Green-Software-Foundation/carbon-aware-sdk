using Microsoft.Extensions.Configuration;

namespace CarbonAware.CLI.Extensions;

public static class ConfigurationBuilderExtensions
{
    public const string DevelopmentEnvironment = "Development";
    public const string ProductionEnvironment = "Production";
    public static IConfigurationBuilder UseCarbonAwareDefaults(this IConfigurationBuilder builder)
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ProductionEnvironment;

        builder.AddJsonFile("appsettings.json", optional: true);
        builder.AddJsonFile($"appsettings.{env}.json", optional: true);
        builder.AddJsonFile($"carbon-aware.config", optional: true);
        if(env.Equals(DevelopmentEnvironment, StringComparison.OrdinalIgnoreCase))
        {
            builder.AddUserSecrets<Program>(optional: true);
        }
        builder.AddEnvironmentVariables();

        return builder;
    }
}