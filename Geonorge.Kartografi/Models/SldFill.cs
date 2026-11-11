namespace Geonorge.Kartografi.Models
{
    public class SldFill
    {
        public string Color { get; set; }
        public double Opacity { get; set; } = 1;
        /// <summary>Pattern fill (GraphicFill)</summary>
        public SldGraphic Graphic { get; set; }
    }
}
