using Geonorge.Kartografi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Geonorge.Kartografi.Services
{
    public class SldParser10
    {
        private static XNamespace SLD = "http://www.opengis.net/sld";
        private static XNamespace OGC = "http://www.opengis.net/ogc";
        private static XNamespace XLINK = "http://www.w3.org/1999/xlink";

        public List<SldLayer> Parse(XDocument sldDoc)
        {
            if (sldDoc.Root == null)
                return null;

            List<SldLayer> layers = new List<SldLayer>();

            XElement root = sldDoc?.Element(SLD + "StyledLayerDescriptor");

            if (root == null)
                return null;

            IEnumerable<XElement> namedLayers = root?.Elements(SLD + "NamedLayer")
                        .ToList();

            if (namedLayers == null)
                return null;

            foreach (var namedLayer in namedLayers)
            {
                SldLayer sldLayer = new SldLayer();

                var nameLayer = namedLayer.Element(SLD + "Name")?.Value;
                sldLayer.Name = nameLayer;

                IEnumerable<XElement> rules = namedLayer?
                        .Element(SLD + "UserStyle")?
                        .Element(SLD + "FeatureTypeStyle")?
                        .Elements(SLD + "Rule")
                        .ToList();

                if (rules == null)
                    return null;

                List<SldRule> sldRules = new List<SldRule>();

                foreach (var rule in rules)
                {

                    var name = rule.Element(SLD + "Name")?.Value;
                    string symboliser = "";
                    string fill = "";
                    string stroke = "";
                    string strokeWidth = "";
                    string strokeDasharray = "0";
                    string wellKnownName = "";
                    string externalGraphicHref = "";
                    int size = 0;
                    List<SldRule> moreSymbols = new List<SldRule>();
                    string identifier = "";

                    externalGraphicHref = rule.Element(SLD + "PointSymbolizer")?.Element(SLD + "Graphic")
                        ?.Element(SLD + "ExternalGraphic")?.Element(SLD + "OnlineResource")?.Attribute(XLINK + "href")?.Value;


                    var point = rule.Element(SLD + "PointSymbolizer");

                    var line = rule.Element(SLD + "LineSymbolizer");

                    if (point != null)
                    {
                        symboliser = "point";
                    }
                    else if (line != null)
                    {
                        symboliser = "line";
                    }
                    else
                    {
                        var polygon = rule.Element(SLD + "PolygonSymbolizer");
                        if (polygon != null)
                            symboliser = "polygon";
                    }

                    if (symboliser == "point")
                    {

                        var pointRules = rule.Elements(SLD + "PointSymbolizer")?.ToList();
                        if (pointRules != null)
                        {
                            int counter = 0;
                            string parent = "0";
                            foreach (var pointRule in pointRules)
                            {
                                if (pointRule.HasElements)
                                {
                                    fill = "";
                                    stroke = "";
                                    strokeWidth = "";
                                    wellKnownName = "";
                                    size = 0;

                                    wellKnownName = pointRule?.Element(SLD + "Graphic")?
                                    .Element(SLD + "Mark")?.Element(SLD + "WellKnownName")?.Value;
                                    wellKnownName = RemoveStrings(wellKnownName);

                                    fill = pointRule?.Element(SLD + "Graphic")?
                                    .Element(SLD + "Mark")?.Element(SLD + "Fill")?.Elements(SLD + "CssParameter")
                                    .First(x => x.Attribute("name").Value == "fill")?.Value;

                                    stroke = pointRule?.Element(SLD + "Graphic")?
                                    .Element(SLD + "Mark")?.Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")?
                                    .FirstOrDefault(x => x.Attribute("name")?.Value == "stroke")?.Value;

                                    var strokeWidthObject = pointRule?.Element(SLD + "Graphic")?
                                    .Element(SLD + "Mark")?.Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")?
                                    .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                                    if (strokeWidthObject != null)
                                        strokeWidth = strokeWidthObject.Value;

                                    var sizeObject = pointRule?.Element(SLD + "Graphic")?
                                    .Element(SLD + "Size");

                                    if (sizeObject != null)
                                        int.TryParse(sizeObject.Value, out size);

                                    moreSymbols.Add(new SldRule
                                    {
                                        Identifier = counter.ToString(),
                                        Symbolizer = symboliser,
                                        Name = name,
                                        WellKnownName = wellKnownName,
                                        Fill = fill,
                                        Stroke = stroke,
                                        StrokeWidth = strokeWidth,
                                        StrokeDasharray = strokeDasharray,
                                        ExternalGraphicHref = externalGraphicHref,
                                        Size = size,
                                        Parent = parent
                                    });
                                    parent = counter.ToString();
                                    counter++;
                                }
                            }

                            if (moreSymbols.Count > 0)
                            {

                                var mainSymbol = moreSymbols.OrderByDescending(s => s.Size).First();
                                moreSymbols.Remove(mainSymbol);

                                symboliser = mainSymbol.Symbolizer;
                                name = mainSymbol.Name;
                                stroke = mainSymbol.Stroke;
                                wellKnownName = mainSymbol.WellKnownName;
                                fill = mainSymbol.Fill;
                                stroke = mainSymbol.Stroke;
                                strokeWidth = mainSymbol.StrokeWidth;
                                size = mainSymbol.Size;
                                identifier = mainSymbol.Identifier;

                            }
                        }


                    }
                    else if (symboliser == "line")
                    {
                        stroke = rule.Element(SLD + "LineSymbolizer")
                            ?.Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")
                            .FirstOrDefault(x => x.Attribute("name").Value == "stroke")?.Value;

                        var strokeDasharrayObject = rule.Element(SLD + "LineSymbolizer")
                            ?.Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")
                            .FirstOrDefault(x => x.Attribute("name").Value == "stroke-dasharray");

                        if (strokeDasharrayObject != null)
                            strokeDasharray = strokeDasharrayObject.Value;

                        var strokeWidthObject = rule.Element(SLD + "LineSymbolizer")?
                            .Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")
                            .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                    }
                    else if (symboliser == "polygon")
                    {
                        var wellKnownNameObject = rule.Element(SLD + "PolygonSymbolizer").Element(SLD + "Fill")?
                            .Element(SLD + "GraphicFill")?.Element(SLD + "Graphic")?.Element(SLD + "Mark")?.Element(SLD + "WellKnownName");

                        if (wellKnownNameObject != null)
                        {
                            wellKnownName = wellKnownNameObject.Value;
                            wellKnownName = RemoveStrings(wellKnownName);
                        }

                        if (string.IsNullOrEmpty(wellKnownName))
                        {
                            stroke = rule.Element(SLD + "PolygonSymbolizer")
                                ?.Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")
                                .FirstOrDefault(x => x.Attribute("name").Value == "stroke")?.Value;
                        }
                        else
                        {
                            stroke = rule.Element(SLD + "PolygonSymbolizer").Element(SLD + "Fill")?
                                .Element(SLD + "GraphicFill")?.Element(SLD + "Graphic")?.Element(SLD + "Mark")
                                ?.Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")
                                .FirstOrDefault(x => x.Attribute("name").Value == "stroke")?.Value;
                        }

                        var strokeWidthObject = rule.Element(SLD + "PolygonSymbolizer")?
                        .Element(SLD + "Stroke")?.Elements(SLD + "CssParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                        if (wellKnownName == "")
                        {
                            fill = rule.Element(SLD + "PolygonSymbolizer")?
                            .Element(SLD + "Fill")?.Elements(SLD + "CssParameter")
                            .First(x => x.Attribute("name").Value == "fill")?.Value;

                        }

                    }

                    sldRules.Add(new SldRule
                    {
                        Identifier = identifier,
                        Symbolizer = symboliser,
                        Name = name,
                        WellKnownName = wellKnownName,
                        Fill = fill,
                        Stroke = stroke,
                        StrokeWidth = strokeWidth,
                        StrokeDasharray = strokeDasharray,
                        ExternalGraphicHref = externalGraphicHref,
                        Size = size,
                        Parent = "0",
                        MoreSymbols = moreSymbols
                    });

                }
                sldLayer.Rules = sldRules;
                layers.Add(sldLayer);
            }


            return layers;
        }

        string RemoveStrings(string str)
        {
            if (!string.IsNullOrEmpty(str))
                str = str.Replace(@"shape://", "");

            return str;
        }

    }
}