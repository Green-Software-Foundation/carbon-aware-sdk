using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CarbonAware.Config
{
    public interface IConfigManager
    {
        IConfigurationSection GetConfigurationSection(string registeredServiceName);
        List<ServiceRegistration> GetServiceConfiguration();
    }
}