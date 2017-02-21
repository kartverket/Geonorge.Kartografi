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

        public List<SldRule> Get(string url)
        {
            XDocument sldDoc = new XDocument();
            try
            {
                sldDoc = XDocument.Load(url);
            }
            catch{}

            return Parse(sldDoc);

        }

        public List<SldRule> Parse(XDocument sldDoc)
        {
            if (sldDoc.Root == null)
                return null;

            List<SldRule> sldRules = new List<SldRule>();

            XElement root = sldDoc.Element(SLD + "StyledLayerDescriptor");

            IEnumerable<XElement> rules =
                    from r in root.Element(SLD + "NamedLayer")
                        .Element(SLD + "UserStyle")
                        .Element(SE + "FeatureTypeStyle")
                        .Elements(SE + "Rule")
                    select r;

            foreach (var rule in rules)
            {

                var name = rule.Element(SE + "Name").Value;
                string symboliser = "";
                string fill = "";
                string stroke = "";
                string strokeWidth = "";
                string wellKnownName = "";

                var point = rule.Element(SE + "PointSymbolizer");
                if (point != null)
                {
                    symboliser = "point";
                }
                else
                {
                    var polygon = rule.Element(SE + "PolygonSymbolizer");
                    if (polygon != null)
                        symboliser = "polygon";
                }

  


                if (symboliser == "point")
                {
                    wellKnownName = rule.Element(SE + "PointSymbolizer").Element(SE + "Graphic")
                        .Element(SE + "Mark").Element(SE + "WellKnownName").Value;

                    if(wellKnownName == "circle" || wellKnownName == "cross" || wellKnownName == "cross_fill")
                    {
                        fill = rule.Element(SE + "PointSymbolizer").Element(SE + "Graphic")
                        .Element(SE + "Mark").Element(SE + "Fill").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "fill").Value;

                        stroke = rule.Element(SE + "PointSymbolizer").Element(SE + "Graphic")
                        .Element(SE + "Mark").Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "stroke").Value;

                        var strokeWidthObject = rule.Element(SE + "PointSymbolizer").Element(SE + "Graphic")
                        .Element(SE + "Mark").Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;
                    }
                    else if (wellKnownName == "square" || wellKnownName == "triangle" || wellKnownName == "cross2")
                    {
                        fill = rule.Element(SE + "PointSymbolizer").Element(SE + "Graphic")
                        .Element(SE + "Mark").Element(SE + "Fill").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "fill").Value;

                        stroke = rule.Element(SE + "PointSymbolizer").Element(SE + "Graphic")
                        .Element(SE + "Mark").Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "stroke").Value;
                    }

                }
                else if (symboliser == "polygon")
                {
                    var wellKnownNameObject = rule.Element(SE + "PolygonSymbolizer").Element(SE + "Fill")?
                        .Element(SE + "GraphicFill")?.Element(SE + "Graphic")?.Element(SE + "Mark")?.Element(SE + "WellKnownName");

                    if (wellKnownNameObject != null)
                        wellKnownName = wellKnownNameObject.Value;


                    if (wellKnownName == "")
                    {
                        fill = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Fill").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "fill").Value;

                        stroke = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "stroke").Value;

                        var strokeWidthObject = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;
                    }
                    else if (wellKnownName == "line")
                    {
                        stroke = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "stroke").Value;

                        var strokeWidthObject = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                    }

                    else if (wellKnownName == "horline")
                    {
                        stroke = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "stroke").Value;

                        var strokeWidthObject = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                    }

                    else if (wellKnownName == "cross")
                    {
                        stroke = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "stroke").Value;

                        var strokeWidthObject = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                    }

                    else if (wellKnownName == "slash")
                    {
                        stroke = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .First(x => x.Attribute("name").Value == "stroke").Value;

                        var strokeWidthObject = rule.Element(SE + "PolygonSymbolizer")
                        .Element(SE + "Stroke").Elements(SE + "SvgParameter")
                        .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                        if (strokeWidthObject != null)
                            strokeWidth = strokeWidthObject.Value;

                    }


                }

                sldRules.Add(new SldRule
                {
                    Symbolizer = symboliser,
                    Name = name,
                    WellKnownName = wellKnownName,
                    Fill = fill,
                    Stroke = stroke,
                    StrokeWidth = strokeWidth
                });

            }

            return sldRules;
        }

    }
}