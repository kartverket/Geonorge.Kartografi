using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class SldRule
    {
        public string Symbolizer { get; set; }
        public string Name { get; set; }
        public string WellKnownName { get; set; }
        public string Fill { get; set; }
        public string Stroke { get; set; }
        public string StrokeWidth { get; set; }
        public string StrokeDasharray { get; set; }
        public int Size { get; set; }
        public string ExternalGraphicHref { get; set; }
        public List<SldRule> MoreSymbols { get; set; }
        public string Identifier { get; set; }
        public string Parent { get; set; }
        public List<SldFilter> SldFilters { get; set; }
    }
}