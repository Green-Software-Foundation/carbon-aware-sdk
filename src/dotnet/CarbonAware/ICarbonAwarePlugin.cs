using System;
using System.Collections.Generic;

namespace CarbonAware
{
    public interface ICarbonAwarePlugin : ICarbonAwareBase
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
        string Version { get; }
        object URL { get; }
    }
}
