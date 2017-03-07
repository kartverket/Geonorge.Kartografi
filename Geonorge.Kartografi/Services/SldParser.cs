﻿using Geonorge.Kartografi.Models;
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

            XElement root = sldDoc?.Element(SLD + "StyledLayerDescriptor");

            if (root == null)
                return null;

            IEnumerable<XElement> namedLayers = root?.Elements(SLD + "NamedLayer")
                        .ToList();

            if (namedLayers == null)
                return null;

            foreach (var namedLayer in namedLayers)
            {

                IEnumerable<XElement> rules = namedLayer?
                        .Element(SLD + "UserStyle")?
                        .Element(SE + "FeatureTypeStyle")?
                        .Elements(SE + "Rule")
                        .ToList();

                if (rules == null)
                    return null;

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
                        wellKnownName = rule.Element(SE + "PointSymbolizer")?.Element(SE + "Graphic")?
                            .Element(SE + "Mark")?.Element(SE + "WellKnownName")?.Value;
                        wellKnownName = RemoveStrings(wellKnownName);

                        if (wellKnownName == "circle" || wellKnownName == "cross" || wellKnownName == "cross_fill")
                        {
                            fill = rule.Element(SE + "PointSymbolizer")?.Element(SE + "Graphic")?
                            .Element(SE + "Mark")?.Element(SE + "Fill")?.Elements(SE + "SvgParameter")
                            .First(x => x.Attribute("name").Value == "fill")?.Value;

                            stroke = rule.Element(SE + "PointSymbolizer")?.Element(SE + "Graphic")?
                            .Element(SE + "Mark")?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")?
                            .First(x => x.Attribute("name")?.Value == "stroke")?.Value;

                            var strokeWidthObject = rule?.Element(SE + "PointSymbolizer")?.Element(SE + "Graphic")
                            .Element(SE + "Mark")?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")?
                            .FirstOrDefault(x => x.Attribute("name").Value == "stroke-width");

                            if (strokeWidthObject != null)
                                strokeWidth = strokeWidthObject.Value;
                        }
                        else if (wellKnownName == "square" || wellKnownName == "triangle" || wellKnownName == "cross2")
                        {
                            fill = rule.Element(SE + "PointSymbolizer")?.Element(SE + "Graphic")?
                            .Element(SE + "Mark")?.Element(SE + "Fill")?.Elements(SE + "SvgParameter")
                            .First(x => x.Attribute("name").Value == "fill")?.Value;

                            stroke = rule.Element(SE + "PointSymbolizer")?.Element(SE + "Graphic")?
                            .Element(SE + "Mark")?.Element(SE + "Stroke")?.Elements(SE + "SvgParameter")
                            .First(x => x.Attribute("name")?.Value == "stroke")?.Value;
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
                        ExternalGraphicHref = externalGraphicHref
                    });

                }
            }

           return sldRules;
        }

        string RemoveStrings(string str)
        {
            if(!string.IsNullOrEmpty(str))
                str = str.Replace(@"shape://", "");

            return str;
        }

    }
}