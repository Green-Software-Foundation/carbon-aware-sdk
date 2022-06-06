using Microsoft.Extensions.Configuration;

namespace CarbonAware.Interfaces;

public interface IConfigurable
{
    public void Configure(IConfigurationSection config);
}
