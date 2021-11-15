using CarbonAwareCLI.Options;
using System;
using System.Collections.Generic;

namespace CarbonAwareCLI
{
    public class CarbonAwareCLIState
    {
        public TimeOptionStates TimeOption { get; set; }
        public OutputOptionStates OutputOption { get; set; }
        public List<string> Locations { get; set; } = new List<string>();
        public DateTime Time { get; set; }
        public bool Lowest { get; set; }
        public LocationOptionStates LocationOption { get; set; }
        public bool Parsed { get; set;  } = false;
    }
}
