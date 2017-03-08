using Geonorge.Kartografi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Geonorge.Kartografi.Services
{
    public class SldParser
    {
        private static XNamespace SLD = "http://www.opengis.net/sld";
        private static XNamespace SE = "http://www.opengis.net/se";
        private static XNamespace XLINK = "http://www.w3.org/1999/xlink";

        public List<SldLayer> Get(string url)
        {
            XDocument sldDoc = new XDocument();
            try
            {
                sldDoc = XDocument.Load(url);
            }
            catch{}

            return Parse(sldDoc);

        }

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

                var nameLayer = namedLayer.Element(SE + "Name")?.Value;
                sldLayer.Name = nameLayer;

                IEnumerable<XElement> rules = namedLayer?
                        .Element(SLD + "UserStyle")?
                        .Element(SE + "FeatureTypeStyle")?
                        .Elements(SE + "Rule")
                        .ToList();

                if (rules == null)
                    return null;

                List<SldRule> sldRules = new List<SldRule>();

                foreach (var rule in rules)
                {

                    var name = rule.Element(SE + "Name")?.Value;
                    string symboliser = "";
                    string fill = "";
                    string stroke = "";
                    string strokeWidth = "";
                    string strokeDasharray = "0";
                    string wellKnownName = "";
                    string externalGraphicHref = "";
                    int size = 0;
                    List<SldRule> moreSymbols = new List<SldRule>();

                    externalGraphicHref = rule.Element(SLD + "PointSymbolizer")?.Element(SLD + "Graphic")
                        ?.Element(SLD + "ExternalGraphic")?.Element(SLD + "OnlineResource")?.Attribute(XLINK + "href")?.Value;

                    var point = rule.Element(SE + "PointSymbolizer");

                    var line = rule.Element(SE + "LineSymbolizer");

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
                        var polygon = rule.Element(SE + "PolygonSymbolizer");
                        if (polygon != null)
                            symboliser = "polygon";
                    }

                    if (symboliser == "point")
                    {

                        var pointRules = rule.Elements(SE + "PointSymbolizer")?.ToList();
                        if (pointRules != null)
                        {
                            foreach(var pointRule in pointRules)
                            {
                                if (pointRule.HasElements)
                                {
                                    fill = "";
                                    stroke = "";
                                    strokeWidth = "";
                                    wellKnownName = "";
                                    size = 0;

                                    wellKnownName = pointRule?.Element(SE + "Graphic")?
                                    .Element(SE + "Mark")?.Element(SE + "WellKnownName")?.Value;
                                    wellKnownName = RemoveStrings(wellKnownName);

                                    fill = pointRule?.Element(SE + "Graphic")?
                                    .Element(SE + "Mark")?.Element(SE + "Fill")?.Elements(SE + "SvgParameter")
                                    .First(x => x.Attribute("name").Value == "fill")?.Value;

                                    stroke = pointRule?.Element(SE + "Graphic")?
                                    .Element(SE + "Mark")?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")?
                                    .First(x => x.Attribute("name")?.Value == "stroke")?.Value;

                                    var strokeWidthObject = pointRule?.Element(SE + "Graphic")?
                                    .Element(SE + "Mark")?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")?
                                    .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                                    if (strokeWidthObject != null)
                                        strokeWidth = strokeWidthObject.Value;

                                    var sizeObject = pointRule?.Element(SE + "Graphic")?
                                    .Element(SE + "Size");

                                    if(sizeObject != null)
                                        int.TryParse(sizeObject.Value, out size);

                                    moreSymbols.Add(new SldRule
                                    {
                                        Symbolizer = symboliser,
                                        Name = name,
                                        WellKnownName = wellKnownName,
                                        Fill = fill,
                                        Stroke = stroke,
                                        StrokeWidth = strokeWidth,
                                        StrokeDasharray = strokeDasharray,
                                        ExternalGraphicHref = externalGraphicHref,
                                        Size = size
                                    });
                                }
                            }

                            if(moreSymbols.Count > 0)
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
                            }
                        }


                    }
                    else if (symboliser == "line")
                    {
                        stroke = rule.Element(SE + "LineSymbolizer")
                            ?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")
                            .FirstOrDefault(x => x.Attribute("name").Value == "stroke")?.Value;

                        var strokeDasharrayObject = rule.Element(SE + "LineSymbolizer")
                            ?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")
                            .FirstOrDefault(x => x.Attribute("name").Value == "stroke-dasharray");

                        if (strokeDasharrayObject != null)
                            strokeDasharray = strokeDasharrayObject.Value;

                        var strokeWidthObject = rule.Element(SE + "LineSymbolizer")?
                            .Element(SE + "Stroke")?.Elements(SE + "SvgParameter")
                            .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                    }
                    else if (symboliser == "polygon")
                    {
                        var wellKnownNameObject = rule.Element(SE + "PolygonSymbolizer").Element(SE + "Fill")?
                            .Element(SE + "GraphicFill")?.Element(SE + "Graphic")?.Element(SE + "Mark")?.Element(SE + "WellKnownName");

                        if (wellKnownNameObject != null)
                        {
                            wellKnownName = wellKnownNameObject.Value;
                            wellKnownName = RemoveStrings(wellKnownName);
                        }

                        if (string.IsNullOrEmpty(wellKnownName))
                        {
                            stroke = rule.Element(SE + "PolygonSymbolizer")
                                ?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")
                                .FirstOrDefault(x => x.Attribute("name").Value == "stroke")?.Value;
                        }
                        else
                        {
                            stroke = rule.Element(SE + "PolygonSymbolizer").Element(SE + "Fill")?
                                .Element(SE + "GraphicFill")?.Element(SE + "Graphic")?.Element(SE + "Mark")
                                ?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")
                                .FirstOrDefault(x => x.Attribute("name").Value == "stroke")?.Value;
                        }

                        var strokeWidthObject = rule.Element(SE + "PolygonSymbolizer")?
                        .Element(SE + "Stroke")?.Elements(SE + "SvgParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                        if (wellKnownName == "")
                        {
                            fill = rule.Element(SE + "PolygonSymbolizer")?
                            .Element(SE + "Fill")?.Elements(SE + "SvgParameter")
                            .First(x => x.Attribute("name").Value == "fill")?.Value;

                        }

                    }

                    sldRules.Add(new SldRule
                    {
                        Symbolizer = symboliser,
                        Name = name,
                        WellKnownName = wellKnownName,
                        Fill = fill,
                        Stroke = stroke,
                        StrokeWidth = strokeWidth,
                        StrokeDasharray = strokeDasharray,
                        ExternalGraphicHref = externalGraphicHref,
                        Size = size,
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
            if(!string.IsNullOrEmpty(str))
                str = str.Replace(@"shape://", "");

            return str;
        }

    }
}