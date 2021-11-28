using Microsoft.Extensions.Configuration;

namespace CarbonAware;

public interface IConfigurable
{
    public void Configure(IConfigurationSection config);
}
