using Microsoft.Extensions.Configuration;

namespace CarbonAware.Config;

public class ConfigManager : IConfigManager
{
    private const string CONFIG_SECTION_SERVICE_REGISTRATIONS = "service-registrations";
    private const string CONFIG_SERVICES_ARRAY = "services";

    private string _configPath;
    private IConfigurationRoot _config;
    public ConfigManager(string configPath)
    {
        _configPath = configPath;
        _config = LoadConfigFile(_configPath);
    }

    public List<ServiceRegistration> GetServiceConfiguration()
    {
        var section = _config.GetSection(CONFIG_SECTION_SERVICE_REGISTRATIONS);
        var services = section.GetSection(CONFIG_SERVICES_ARRAY).Get<List<ServiceRegistration>>();
        ValidateServiceConfiguration(services);
        return services;
    }

    private void ValidateServiceConfiguration(List<ServiceRegistration> services)
    {
        if (services == null) throw new ArgumentException($"Configuration file '{_configPath}' is invalid.  Could not find services.");
        foreach (var service in services)
        {
            ValidateService(service);
        }
    }

    public static void ValidateService(ServiceRegistration service)
    {
        if (service.service is null || service.implementation is null)
        {
            throw new ArgumentException($"Service configuration is invalid.  Service: '{service.service}', Implementation: '{service.implementation}'");
        }
    }

    public IConfigurationSection GetConfigurationSection(string sectionName)
    {
        return _config.GetSection(sectionName);
    }


    private IConfigurationRoot LoadConfigFile(string configPath)
    {
        try
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + configPath);

            var config = builder.Build();
            return config;
        }
        catch (InvalidDataException ide)
        {
            throw new ArgumentException($"Json configuration file '{ configPath }' is not valid.", ide);
        }
    }
}
