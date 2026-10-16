using System.Collections.Generic;

namespace Geonorge.Kartografi.Models
{
    public class SldSymbolizer
    {
        /// <summary>point, line, polygon, text or raster</summary>
        public string Type { get; set; }
        public SldFill Fill { get; set; }
        public SldStroke Stroke { get; set; }
        public SldGraphic Graphic { get; set; }
        public double PerpendicularOffset { get; set; }

        public string FontFamily { get; set; }
        public string FontStyle { get; set; }
        public string FontWeight { get; set; }
        public SldFill HaloFill { get; set; }
        public double HaloRadius { get; set; }

        public List<string> ColorMap { get; set; } = new List<string>();
    }
}
