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

            string version = root.Attribute("version").Value;

            if (version.StartsWith("1.0"))
            {
                IEnumerable<XElement> rules =
                from r in root.Element(SLD + "NamedLayer")
                    .Element(SLD + "UserStyle")
                    .Element(SLD + "FeatureTypeStyle")
                    .Elements(SLD + "Rule")
                select r;

                foreach (var rule in rules)
                {
                    var name = rule.Element(SLD + "Name").Value;

                    sldRules.Add(new SldRule { Name = name });
                }
            }
            else
            { 
                IEnumerable<XElement> rules =
                        from r in root.Element(SLD + "NamedLayer")
                            .Element(SLD + "UserStyle")
                            .Element(SE + "FeatureTypeStyle")
                            .Elements(SE + "Rule")
                        select r;

                foreach (var rule in rules)
                {
                    var name = rule.Element(SE + "Name").Value;

                    sldRules.Add(new SldRule { Name = name });
                }
            }
            return sldRules;
        }

    }
}