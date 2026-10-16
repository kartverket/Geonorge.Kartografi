namespace Geonorge.Kartografi.Models
{
    public class SldStroke
    {
        public string Color { get; set; }
        public double Width { get; set; } = 1;
        public double Opacity { get; set; } = 1;
        public string Dasharray { get; set; }
        public string Linecap { get; set; }
        public string Linejoin { get; set; }
        /// <summary>Repeated symbol along the line (GraphicStroke)</summary>
        public SldGraphic Graphic { get; set; }
    }
}
