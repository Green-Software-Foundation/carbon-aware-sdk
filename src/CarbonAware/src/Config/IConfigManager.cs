using Microsoft.Extensions.Configuration;

namespace CarbonAware.Config;

public interface IConfigManager
{
    IConfigurationSection GetConfigurationSection(string registeredServiceName);
    List<ServiceRegistration> GetServiceConfiguration();
}
