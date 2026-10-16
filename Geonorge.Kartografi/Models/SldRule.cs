using System.Collections.Generic;

namespace Geonorge.Kartografi.Models
{
    public class SldRule
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }

        /// <summary>Human readable version of the rule filter, null when the rule has no filter</summary>
        public string Filter { get; set; }
        public bool IsElseFilter { get; set; }
        public double? MinScaleDenominator { get; set; }
        public double? MaxScaleDenominator { get; set; }

        /// <summary>Symbolizers in painting order, may come from several FeatureTypeStyles</summary>
        public List<SldSymbolizer> Symbolizers { get; set; } = new List<SldSymbolizer>();

        public string Label => !string.IsNullOrWhiteSpace(Title) ? Title : Name;
    }
}
