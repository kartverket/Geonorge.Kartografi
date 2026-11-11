namespace Geonorge.Kartografi.Models
{
    public class SldGraphic
    {
        /// <summary>Mark name in lower case without "shape://" prefix, e.g. circle, square, horline</summary>
        public string WellKnownName { get; set; }
        public SldFill Fill { get; set; }
        public SldStroke Stroke { get; set; }
        public string ExternalGraphicHref { get; set; }
        public string ExternalGraphicFormat { get; set; }
        /// <summary>Size in pixels, null when not given in the SLD</summary>
        public double? Size { get; set; }
        public double Rotation { get; set; }
        public double Opacity { get; set; } = 1;
    }
}
