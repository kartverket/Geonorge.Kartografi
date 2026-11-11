using Geonorge.Kartografi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Geonorge.Kartografi.Services
{
    /// <summary>
    /// Renders a legend symbol for an SLD rule as inline SVG in a 100x100 viewBox.
    /// All symbolizers in the rule are painted in order, like a map renderer would.
    /// </summary>
    public class SldLegendRenderer
    {
        private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

        // SLD sizes are in pixels, the viewBox is roughly a 25 px legend icon for lines and polygons
        private const double LineScale = 4;
        private const double PatternScale = 2.5;
        private const double DefaultMarkSize = 6;

        private static readonly HashSet<string> HatchMarks = new HashSet<string>
        {
            "line", "vertline", "horline", "slash", "backslash", "cross", "plus", "x", "times", "cross2"
        };

        // Pattern ids must be unique on the page
        private readonly string _idPrefix = "sld-" + Guid.NewGuid().ToString("N").Substring(0, 8) + "-";
        private int _idCounter;

        public string Render(SldRule rule, int width = 100, int height = 100)
        {
            var svg = new XElement(Svg + "svg",
                new XAttribute("width", width),
                new XAttribute("height", height),
                new XAttribute("viewBox", "0 0 100 100"),
                new XAttribute("role", "img"),
                new XAttribute("aria-label", rule.Label ?? ""));

            var defs = new XElement(Svg + "defs");
            double pointScale = PointScale(rule);

            foreach (var symbolizer in rule.Symbolizers)
            {
                switch (symbolizer.Type)
                {
                    case "polygon":
                        svg.Add(Polygon(symbolizer, defs));
                        break;
                    case "line":
                        svg.Add(Line(symbolizer, defs));
                        break;
                    case "point":
                        svg.Add(Point(symbolizer.Graphic, 50, 50, pointScale, defs));
                        break;
                    case "text":
                        svg.Add(Text(symbolizer));
                        break;
                    case "raster":
                        svg.Add(Raster(symbolizer));
                        break;
                }
            }

            if (defs.HasElements)
                svg.AddFirst(defs);

            return svg.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>Points share one scale so stacked marks keep their relative size</summary>
        private static double PointScale(SldRule rule)
        {
            var sizes = rule.Symbolizers
                .Where(s => s.Type == "point" && s.Graphic != null && (s.Graphic.Size.HasValue || s.Graphic.ExternalGraphicHref == null))
                .Select(s => s.Graphic.Size ?? DefaultMarkSize)
                .Where(s => s > 0)
                .ToList();

            double maxSize = sizes.Count > 0 ? sizes.Max() : DefaultMarkSize;
            double available = rule.Symbolizers.Any(s => s.Type != "point") ? 50 : 80;

            return available / maxSize;
        }

        private XElement Polygon(SldSymbolizer symbolizer, XElement defs)
        {
            var group = new XElement(Svg + "g");
            var fill = symbolizer.Fill;

            // A color behind a pattern fill
            if (fill != null && fill.Graphic != null && fill.Color != null)
                group.Add(Rect(new XAttribute("fill", fill.Color), Opacity("fill-opacity", fill.Opacity)));

            group.Add(Rect(
                FillAttributes(fill, defs),
                StrokeAttributes(symbolizer.Stroke, LineScale, 1, 20)));

            return group;
        }

        private static XElement Rect(params object[] content)
        {
            return new XElement(Svg + "rect",
                new XAttribute("x", 10), new XAttribute("y", 10),
                new XAttribute("width", 80), new XAttribute("height", 80),
                content);
        }

        private XElement Line(SldSymbolizer symbolizer, XElement defs)
        {
            var stroke = symbolizer.Stroke;
            double y = 50 - Clamp(symbolizer.PerpendicularOffset * LineScale, -40, 40);
            var group = new XElement(Svg + "g");

            if (stroke.Color != null)
            {
                group.Add(new XElement(Svg + "path",
                    new XAttribute("d", "M5 " + F(y) + " L95 " + F(y)),
                    new XAttribute("fill", "none"),
                    StrokeAttributes(stroke, LineScale, 1, 60)));
            }

            if (stroke.Graphic != null)
            {
                double size = Clamp((stroke.Graphic.Size ?? DefaultMarkSize) * LineScale, 6, 30);
                double scale = size / (stroke.Graphic.Size ?? DefaultMarkSize);
                for (double x = 20; x <= 80; x += 30)
                    group.Add(Point(stroke.Graphic, x, y, scale, defs));
            }

            return group;
        }

        private XElement Point(SldGraphic graphic, double cx, double cy, double scale, XElement defs)
        {
            var group = new XElement(Svg + "g", Opacity("opacity", graphic.Opacity));
            if (graphic.Rotation != 0)
                group.Add(new XAttribute("transform", "rotate(" + F(graphic.Rotation) + " " + F(cx) + " " + F(cy) + ")"));

            if (graphic.ExternalGraphicHref != null)
            {
                double size = graphic.Size.HasValue ? graphic.Size.Value * scale : 80;
                group.Add(Image(graphic.ExternalGraphicHref, cx - size / 2, cy - size / 2, size));
            }
            else
            {
                double size = (graphic.Size ?? DefaultMarkSize) * scale;
                group.Add(Mark(graphic, cx, cy, size, scale, defs));
            }

            return group;
        }

        private static XElement Image(string href, double x, double y, double size)
        {
            return new XElement(Svg + "image",
                new XAttribute("href", href),
                new XAttribute("x", F(x)), new XAttribute("y", F(y)),
                new XAttribute("width", F(size)), new XAttribute("height", F(size)),
                new XAttribute("preserveAspectRatio", "xMidYMid meet"));
        }

        private XElement Mark(SldGraphic graphic, double cx, double cy, double size, double strokeScale, XElement defs)
        {
            bool lineOnly;
            string path = MarkPath(graphic.WellKnownName, cx, cy, size, out lineOnly);

            return new XElement(Svg + "path",
                new XAttribute("d", path),
                lineOnly ? new XAttribute("fill", "none") : (object)FillAttributes(graphic.Fill, defs),
                StrokeAttributes(graphic.Stroke, strokeScale, 1, Math.Max(2, size * 0.1)));
        }

        private XElement Text(SldSymbolizer symbolizer)
        {
            var text = new XElement(Svg + "text", "Abc",
                new XAttribute("x", 50), new XAttribute("y", 50),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("dominant-baseline", "central"),
                new XAttribute("font-size", 40),
                new XAttribute("font-family", symbolizer.FontFamily ?? "sans-serif"),
                FillAttributes(symbolizer.Fill, null));

            if (symbolizer.FontStyle != null)
                text.Add(new XAttribute("font-style", symbolizer.FontStyle));
            if (symbolizer.FontWeight != null)
                text.Add(new XAttribute("font-weight", symbolizer.FontWeight));

            if (symbolizer.HaloFill?.Color != null)
            {
                text.Add(new XAttribute("stroke", symbolizer.HaloFill.Color),
                    Opacity("stroke-opacity", symbolizer.HaloFill.Opacity),
                    new XAttribute("stroke-width", F(Clamp(symbolizer.HaloRadius * 2 * LineScale, 2, 12))),
                    new XAttribute("stroke-linejoin", "round"),
                    new XAttribute("paint-order", "stroke"));
            }

            return text;
        }

        private static XElement Raster(SldSymbolizer symbolizer)
        {
            var colors = symbolizer.ColorMap.Count > 0 ? symbolizer.ColorMap : new List<string> { "#000000", "#ffffff" };
            var group = new XElement(Svg + "g");
            double width = 80.0 / colors.Count;

            for (int i = 0; i < colors.Count; i++)
            {
                group.Add(new XElement(Svg + "rect",
                    new XAttribute("x", F(10 + i * width)), new XAttribute("y", 10),
                    new XAttribute("width", F(width)), new XAttribute("height", 80),
                    new XAttribute("fill", colors[i])));
            }

            return group;
        }

        private IEnumerable<XAttribute> FillAttributes(SldFill fill, XElement defs)
        {
            if (fill == null || (fill.Color == null && fill.Graphic == null))
            {
                yield return new XAttribute("fill", "none");
                yield break;
            }

            if (fill.Graphic != null && defs != null)
            {
                yield return new XAttribute("fill", "url(#" + Pattern(fill.Graphic, defs) + ")");
                yield break;
            }

            yield return new XAttribute("fill", fill.Color ?? "none");
            var opacity = Opacity("fill-opacity", fill.Opacity);
            if (opacity != null)
                yield return opacity;
        }

        private static IEnumerable<XAttribute> StrokeAttributes(SldStroke stroke, double scale, double minWidth, double maxWidth)
        {
            if (stroke == null || stroke.Color == null)
            {
                yield return new XAttribute("stroke", "none");
                yield break;
            }

            yield return new XAttribute("stroke", stroke.Color);
            yield return new XAttribute("stroke-width", F(Clamp(stroke.Width * scale, minWidth, maxWidth)));

            var opacity = Opacity("stroke-opacity", stroke.Opacity);
            if (opacity != null)
                yield return opacity;

            if (stroke.Dasharray != null)
            {
                var dashes = stroke.Dasharray.Split(' ')
                    .Select(d => F(double.Parse(d, CultureInfo.InvariantCulture) * scale));
                yield return new XAttribute("stroke-dasharray", string.Join(" ", dashes));
            }

            if (stroke.Linecap != null)
                yield return new XAttribute("stroke-linecap", stroke.Linecap);
            if (stroke.Linejoin != null)
                yield return new XAttribute("stroke-linejoin", stroke.Linejoin == "mitre" ? "miter" : stroke.Linejoin);
        }

        private static XAttribute Opacity(string name, double opacity)
        {
            return opacity < 1 ? new XAttribute(name, F(Clamp(opacity, 0, 1))) : null;
        }

        /// <summary>Adds a tile pattern for a GraphicFill and returns its id</summary>
        private string Pattern(SldGraphic graphic, XElement defs)
        {
            string id = _idPrefix + (++_idCounter);
            bool external = graphic.ExternalGraphicHref != null;
            double tile = external
                ? Clamp((graphic.Size ?? 16) * PatternScale, 10, 50)
                : Clamp((graphic.Size ?? DefaultMarkSize) * PatternScale, 6, 40);

            var pattern = new XElement(Svg + "pattern",
                new XAttribute("id", id),
                new XAttribute("patternUnits", "userSpaceOnUse"),
                new XAttribute("width", F(tile)), new XAttribute("height", F(tile)));
            var content = new XElement(Svg + "g", Opacity("opacity", graphic.Opacity));
            pattern.Add(content);

            // Added before the content so nested patterns are not referenced before they are defined
            defs.Add(pattern);

            if (external)
            {
                content.Add(Image(graphic.ExternalGraphicHref, 0, 0, tile));
            }
            else if (HatchMarks.Contains(graphic.WellKnownName))
            {
                // Lines that continue across tiles
                content.Add(new XElement(Svg + "path",
                    new XAttribute("d", HatchPath(graphic.WellKnownName, tile)),
                    new XAttribute("fill", "none"),
                    new XAttribute("stroke-linecap", "square"),
                    StrokeAttributes(graphic.Stroke, PatternScale, 0.75, tile / 2)));
            }
            else
            {
                double scale = tile / (graphic.Size ?? DefaultMarkSize);
                content.Add(Mark(graphic, tile / 2, tile / 2, tile, scale, defs));
            }

            return id;
        }

        private static string HatchPath(string name, double t)
        {
            string vertical = "M" + F(t / 2) + " 0 L" + F(t / 2) + " " + F(t);
            string horizontal = "M0 " + F(t / 2) + " L" + F(t) + " " + F(t / 2);
            string slash = "M0 " + F(t) + " L" + F(t) + " 0 " +
                           "M" + F(-t / 2) + " " + F(t / 2) + " L" + F(t / 2) + " " + F(-t / 2) + " " +
                           "M" + F(t / 2) + " " + F(t * 1.5) + " L" + F(t * 1.5) + " " + F(t / 2);
            string backslash = "M0 0 L" + F(t) + " " + F(t) + " " +
                               "M" + F(t / 2) + " " + F(-t / 2) + " L" + F(t * 1.5) + " " + F(t / 2) + " " +
                               "M" + F(-t / 2) + " " + F(t / 2) + " L" + F(t / 2) + " " + F(t * 1.5);

            switch (name)
            {
                case "horline": return horizontal;
                case "slash": return slash;
                case "backslash": return backslash;
                case "cross":
                case "plus": return vertical + " " + horizontal;
                case "x":
                case "times":
                case "cross2": return slash + " " + backslash;
                default: return vertical;
            }
        }

        /// <summary>Path for a well known mark, centered on (cx, cy) and fitting in a square of the given size</summary>
        private static string MarkPath(string name, double cx, double cy, double size, out bool lineOnly)
        {
            lineOnly = false;
            double r = size / 2;
            Func<double[][], bool, string> path = (points, closed) =>
            {
                var sb = new StringBuilder();
                for (int i = 0; i < points.Length; i++)
                    sb.Append(i == 0 ? "M" : " L").Append(F(cx + points[i][0] * size)).Append(' ').Append(F(cy + points[i][1] * size));
                if (closed)
                    sb.Append(" Z");
                return sb.ToString();
            };

            switch (name)
            {
                case "circle":
                    return "M" + F(cx - r) + " " + F(cy) + " A" + F(r) + " " + F(r) + " 0 1 0 " + F(cx + r) + " " + F(cy) +
                           " A" + F(r) + " " + F(r) + " 0 1 0 " + F(cx - r) + " " + F(cy) + " Z";
                case "dot":
                    double dot = r / 3;
                    return "M" + F(cx - dot) + " " + F(cy) + " A" + F(dot) + " " + F(dot) + " 0 1 0 " + F(cx + dot) + " " + F(cy) +
                           " A" + F(dot) + " " + F(dot) + " 0 1 0 " + F(cx - dot) + " " + F(cy) + " Z";
                case "triangle":
                    return path(new[] { P(0, -0.5), P(0.5, 0.5), P(-0.5, 0.5) }, true);
                case "diamond":
                    return path(new[] { P(0, -0.5), P(0.5, 0), P(0, 0.5), P(-0.5, 0) }, true);
                case "star":
                    return path(Star(), true);
                case "pentagon":
                    return path(RegularPolygon(5), true);
                case "hexagon":
                    return path(RegularPolygon(6), true);
                case "octagon":
                    return path(RegularPolygon(8), true);
                case "cross":
                case "cross_fill":
                    return path(Plus(0), true);
                case "x":
                    return path(Plus(45), true);
                case "carrow":
                    return path(new[] { P(-0.5, -0.5), P(0.5, 0), P(-0.5, 0.5) }, true);
                case "oarrow":
                    lineOnly = true;
                    return path(new[] { P(-0.5, -0.5), P(0.5, 0), P(-0.5, 0.5) }, false);
                case "line":
                case "vertline":
                    lineOnly = true;
                    return path(new[] { P(0, -0.5), P(0, 0.5) }, false);
                case "horline":
                    lineOnly = true;
                    return path(new[] { P(-0.5, 0), P(0.5, 0) }, false);
                case "slash":
                    lineOnly = true;
                    return path(new[] { P(-0.5, 0.5), P(0.5, -0.5) }, false);
                case "backslash":
                    lineOnly = true;
                    return path(new[] { P(-0.5, -0.5), P(0.5, 0.5) }, false);
                case "plus":
                    lineOnly = true;
                    return path(new[] { P(0, -0.5), P(0, 0.5) }, false) + " " + path(new[] { P(-0.5, 0), P(0.5, 0) }, false);
                case "times":
                case "cross2":
                    lineOnly = true;
                    return path(new[] { P(-0.5, -0.5), P(0.5, 0.5) }, false) + " " + path(new[] { P(-0.5, 0.5), P(0.5, -0.5) }, false);
                default:
                    // SE: square is the default mark, also used for marks we can not draw (e.g. ttf:// fonts)
                    return path(new[] { P(-0.5, -0.5), P(0.5, -0.5), P(0.5, 0.5), P(-0.5, 0.5) }, true);
            }
        }

        private static double[] P(double x, double y)
        {
            return new[] { x, y };
        }

        private static double[][] Star()
        {
            return Enumerable.Range(0, 10)
                .Select(i => Polar(i % 2 == 0 ? 0.5 : 0.2, -90 + i * 36))
                .ToArray();
        }

        private static double[][] RegularPolygon(int sides)
        {
            return Enumerable.Range(0, sides)
                .Select(i => Polar(0.5, -90 + i * 360.0 / sides))
                .ToArray();
        }

        private static double[][] Plus(double rotation)
        {
            double w = 0.125;
            double l = rotation == 0 ? 0.5 : 0.6;
            var points = new[]
            {
                P(-w, -l), P(w, -l), P(w, -w), P(l, -w), P(l, w), P(w, w),
                P(w, l), P(-w, l), P(-w, w), P(-l, w), P(-l, -w), P(-w, -w)
            };
            double radians = rotation * Math.PI / 180;
            return points
                .Select(p => P(p[0] * Math.Cos(radians) - p[1] * Math.Sin(radians), p[0] * Math.Sin(radians) + p[1] * Math.Cos(radians)))
                .ToArray();
        }

        private static double[] Polar(double radius, double degrees)
        {
            double radians = degrees * Math.PI / 180;
            return P(radius * Math.Cos(radians), radius * Math.Sin(radians));
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static string F(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
