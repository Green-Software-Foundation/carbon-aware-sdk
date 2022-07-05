namespace CarbonAware.Interfaces;

public interface ICarbonAwarePlugin : ICarbonAwareBase, IConfigurable
{
    string Name { get; }
    string Description { get; }
    string Author { get; }
    string Version { get; }
    object URL { get; }

}
