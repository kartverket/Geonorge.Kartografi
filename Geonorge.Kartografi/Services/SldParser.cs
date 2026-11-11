using Geonorge.Kartografi.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Geonorge.Kartografi.Services
{
    /// <summary>
    /// Parses SLD 1.0 and SLD 1.1/SE 1.1 documents. Elements are matched on local name only,
    /// so the sld:, se: and ogc: namespaces (and CssParameter/SvgParameter) are handled alike.
    /// </summary>
    public class SldParser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SldParser));

        public List<SldLayer> Get(string url)
        {
            try
            {
                return Parse(XDocument.Load(url), url);
            }
            catch (Exception ex)
            {
                Log.Warn("Could not parse SLD " + url, ex);
                return new List<SldLayer>();
            }
        }

        /// <param name="baseUrl">Used to resolve relative ExternalGraphic references</param>
        public List<SldLayer> Parse(XDocument sldDoc, string baseUrl = null)
        {
            var layers = new List<SldLayer>();
            XElement root = sldDoc?.Root;
            if (root == null)
                return layers;

            switch (root.Name.LocalName)
            {
                case "StyledLayerDescriptor":
                    foreach (var layer in root.Elements().Where(e => e.Name.LocalName == "NamedLayer" || e.Name.LocalName == "UserLayer"))
                        AddLayer(layers, Text(layer, "Name"), Children(layer, "UserStyle"), baseUrl);
                    break;
                case "UserStyle":
                    AddLayer(layers, Text(root, "Name"), new[] { root }, baseUrl);
                    break;
                case "FeatureTypeStyle":
                case "CoverageStyle":
                    AddLayer(layers, Text(root, "Name"), new[] { new XElement("UserStyle", root) }, baseUrl);
                    break;
            }

            return layers;
        }

        private void AddLayer(List<SldLayer> layers, string name, IEnumerable<XElement> userStyles, string baseUrl)
        {
            var rules = new List<SldRule>();
            var rulesByKey = new Dictionary<string, SldRule>();

            var featureTypeStyles = userStyles.SelectMany(s => s.Elements()
                .Where(e => e.Name.LocalName == "FeatureTypeStyle" || e.Name.LocalName == "CoverageStyle"));

            foreach (var featureTypeStyle in featureTypeStyles)
            {
                var added = new List<KeyValuePair<string, SldRule>>();

                foreach (var ruleElement in Children(featureTypeStyle, "Rule"))
                {
                    SldRule rule = ParseRule(ruleElement, baseUrl);

                    // Several FeatureTypeStyles are often used to paint the same rule in layers (e.g. road casing + fill),
                    // merge them with the rule from an earlier FeatureTypeStyle so the legend shows one combined symbol.
                    string key = string.Join("|", rule.Name, rule.Title, rule.Filter, rule.IsElseFilter,
                        rule.MinScaleDenominator, rule.MaxScaleDenominator);
                    SldRule existing;
                    if (rulesByKey.TryGetValue(key, out existing))
                    {
                        existing.Symbolizers.AddRange(rule.Symbolizers);
                    }
                    else
                    {
                        added.Add(new KeyValuePair<string, SldRule>(key, rule));
                        rules.Add(rule);
                    }
                }

                foreach (var rule in added)
                {
                    if (!rulesByKey.ContainsKey(rule.Key))
                        rulesByKey[rule.Key] = rule.Value;
                }
            }

            if (rules.Count > 0)
                layers.Add(new SldLayer { Name = name, Rules = rules });
        }

        private SldRule ParseRule(XElement rule, string baseUrl)
        {
            var description = Child(rule, "Description");
            var filter = Child(rule, "Filter");

            var sldRule = new SldRule
            {
                Name = Text(rule, "Name"),
                Title = Text(description, "Title") ?? Text(rule, "Title"),
                Abstract = Text(description, "Abstract") ?? Text(rule, "Abstract"),
                Filter = filter != null ? SldFilterFormatter.Format(filter) : null,
                IsElseFilter = Child(rule, "ElseFilter") != null,
                MinScaleDenominator = Number(Child(rule, "MinScaleDenominator")),
                MaxScaleDenominator = Number(Child(rule, "MaxScaleDenominator")),
            };

            foreach (var symbolizer in rule.Elements())
            {
                var parsed = ParseSymbolizer(symbolizer, baseUrl);
                if (parsed != null)
                    sldRule.Symbolizers.Add(parsed);
            }

            return sldRule;
        }

        private SldSymbolizer ParseSymbolizer(XElement element, string baseUrl)
        {
            switch (element.Name.LocalName)
            {
                case "PointSymbolizer":
                    return new SldSymbolizer
                    {
                        Type = "point",
                        Graphic = ParseGraphic(Child(element, "Graphic"), baseUrl) ?? new SldGraphic { WellKnownName = "square" }
                    };
                case "LineSymbolizer":
                    return new SldSymbolizer
                    {
                        Type = "line",
                        // SE: a LineSymbolizer without Stroke is drawn as a solid black line
                        Stroke = ParseStroke(Child(element, "Stroke"), baseUrl) ?? new SldStroke { Color = "#000000" },
                        PerpendicularOffset = Number(Child(element, "PerpendicularOffset")) ?? 0
                    };
                case "PolygonSymbolizer":
                    var fillElement = Child(element, "Fill");
                    return new SldSymbolizer
                    {
                        Type = "polygon",
                        // SE: a PolygonSymbolizer without Fill is filled with solid gray
                        Fill = fillElement == null ? new SldFill { Color = "#808080" } : ParseFill(fillElement, baseUrl),
                        Stroke = ParseStroke(Child(element, "Stroke"), baseUrl)
                    };
                case "TextSymbolizer":
                    var font = Child(element, "Font");
                    var halo = Child(element, "Halo");
                    return new SldSymbolizer
                    {
                        Type = "text",
                        FontFamily = Parameter(font, "font-family"),
                        FontStyle = Parameter(font, "font-style"),
                        FontWeight = Parameter(font, "font-weight"),
                        Fill = Child(element, "Fill") != null ? ParseFill(Child(element, "Fill"), baseUrl) : new SldFill { Color = "#000000" },
                        HaloFill = halo == null ? null : (Child(halo, "Fill") != null ? ParseFill(Child(halo, "Fill"), baseUrl) : new SldFill { Color = "#ffffff" }),
                        HaloRadius = halo == null ? 0 : Number(Child(halo, "Radius")) ?? 1
                    };
                case "RasterSymbolizer":
                    var colorMap = Child(element, "ColorMap");
                    var colors = colorMap?.Descendants()
                        .Where(e => e.Name.LocalName == "ColorMapEntry")
                        .Select(e => (string)e.Attribute("color"))
                        .ToList() ?? new List<string>();
                    // SE 1.1 uses Categorize/Interpolate with Value elements
                    if (colors.Count == 0 && colorMap != null)
                        colors = colorMap.Descendants().Where(e => e.Name.LocalName == "Value" && !e.HasElements)
                            .Select(e => e.Value.Trim()).Where(v => v.StartsWith("#")).ToList();
                    return new SldSymbolizer { Type = "raster", ColorMap = colors.Where(c => !string.IsNullOrEmpty(c)).ToList() };
                default:
                    return null;
            }
        }

        private SldFill ParseFill(XElement fill, string baseUrl)
        {
            if (fill == null)
                return null;

            var graphic = ParseGraphic(Child(Child(fill, "GraphicFill"), "Graphic"), baseUrl);
            var color = Parameter(fill, "fill");

            return new SldFill
            {
                Color = color ?? (graphic == null ? "#808080" : null),
                Opacity = Number(Parameter(fill, "fill-opacity")) ?? 1,
                Graphic = graphic
            };
        }

        private SldStroke ParseStroke(XElement stroke, string baseUrl)
        {
            if (stroke == null)
                return null;

            var graphic = ParseGraphic(Child(Child(stroke, "GraphicStroke"), "Graphic"), baseUrl);
            var color = Parameter(stroke, "stroke");

            return new SldStroke
            {
                Color = color ?? (graphic == null ? "#000000" : null),
                Width = Number(Parameter(stroke, "stroke-width")) ?? 1,
                Opacity = Number(Parameter(stroke, "stroke-opacity")) ?? 1,
                Dasharray = NormalizeDasharray(Parameter(stroke, "stroke-dasharray")),
                Linecap = Parameter(stroke, "stroke-linecap"),
                Linejoin = Parameter(stroke, "stroke-linejoin"),
                Graphic = graphic
            };
        }

        private SldGraphic ParseGraphic(XElement graphic, string baseUrl)
        {
            if (graphic == null)
                return null;

            var result = new SldGraphic
            {
                Size = Number(Child(graphic, "Size")),
                Rotation = Number(Child(graphic, "Rotation")) ?? 0,
                Opacity = Number(Child(graphic, "Opacity")) ?? 1
            };

            // A Graphic may list several Marks/ExternalGraphics as alternatives, the first supported one is used
            foreach (var item in graphic.Elements())
            {
                if (item.Name.LocalName == "ExternalGraphic")
                {
                    var href = Child(item, "OnlineResource")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "href")?.Value;
                    href = ResolveHref(href, baseUrl);
                    if (href == null)
                        continue;
                    result.ExternalGraphicHref = href;
                    result.ExternalGraphicFormat = Text(item, "Format");
                    return result;
                }

                if (item.Name.LocalName == "Mark")
                {
                    result.WellKnownName = NormalizeWellKnownName(Text(item, "WellKnownName"));
                    var fill = Child(item, "Fill");
                    var stroke = Child(item, "Stroke");
                    // SE: Mark defaults are gray fill and black stroke when the elements are left out
                    result.Fill = fill == null ? new SldFill { Color = "#808080" } : ParseFill(fill, baseUrl);
                    result.Stroke = stroke == null ? new SldStroke { Color = "#000000" } : ParseStroke(stroke, baseUrl);
                    return result;
                }
            }

            // SE: a Graphic without Mark or ExternalGraphic is a gray square
            result.WellKnownName = "square";
            result.Fill = new SldFill { Color = "#808080" };
            result.Stroke = new SldStroke { Color = "#000000" };
            return result;
        }

        private static string ResolveHref(string href, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(href))
                return null;

            href = href.Trim();
            Uri uri;
            if (!Uri.TryCreate(href, UriKind.Absolute, out uri))
            {
                Uri baseUri;
                if (baseUrl == null || !Uri.TryCreate(baseUrl, UriKind.Absolute, out baseUri) || !Uri.TryCreate(baseUri, href, out uri))
                    return null;
            }

            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                return uri.AbsoluteUri;

            if (uri.Scheme == "data" && href.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                return href;

            return null;
        }

        private static string NormalizeWellKnownName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "square";

            name = name.Trim().ToLowerInvariant();
            if (name.StartsWith("shape://"))
                name = name.Substring("shape://".Length);

            return name;
        }

        private static string NormalizeDasharray(string dasharray)
        {
            if (string.IsNullOrWhiteSpace(dasharray))
                return null;

            var values = dasharray.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0 || values.Any(v => Number(v) == null) || values.All(v => Number(v) == 0))
                return null;

            return string.Join(" ", values);
        }

        // CssParameter (SLD 1.0) or SvgParameter (SE 1.1)
        private static string Parameter(XElement container, string name)
        {
            var parameter = container?.Elements()
                .FirstOrDefault(e => (e.Name.LocalName == "CssParameter" || e.Name.LocalName == "SvgParameter")
                    && (string)e.Attribute("name") == name);

            return ExpressionText(parameter);
        }

        /// <summary>Value of a parameter or expression, null if it depends on feature attributes</summary>
        private static string ExpressionText(XElement element)
        {
            if (element == null)
                return null;

            if (!element.HasElements)
                return NullIfEmpty(element.Value);

            var parts = new List<string>();
            foreach (var node in element.Nodes())
            {
                var text = node as XText;
                var child = node as XElement;
                if (text != null)
                    parts.Add(text.Value);
                else if (child != null && child.Name.LocalName == "Literal")
                    parts.Add(child.Value);
                else
                    return null;
            }

            return NullIfEmpty(string.Concat(parts));
        }

        private static double? Number(XElement element)
        {
            return Number(ExpressionText(element));
        }

        private static double? Number(string value)
        {
            double number;
            if (value != null && double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                return number;

            return null;
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static IEnumerable<XElement> Children(XElement element, string localName)
        {
            return element?.Elements().Where(e => e.Name.LocalName == localName) ?? Enumerable.Empty<XElement>();
        }

        private static XElement Child(XElement element, string localName)
        {
            return Children(element, localName).FirstOrDefault();
        }

        private static string Text(XElement element, string localName)
        {
            return NullIfEmpty(Child(element, localName)?.Value);
        }
    }
}
