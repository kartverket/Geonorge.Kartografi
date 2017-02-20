using System.Collections.Generic;
using System.Xml.Linq;
using Geonorge.Kartografi.Models;
using Geonorge.Kartografi.Services;
using Moq;
using System.IO;
using Xunit;

namespace Geonorge.Kartografi.Tests.Services
{
    public class SldParserTest
    {
        private string xmlFile;

        [Fact]
        public void ShouldParseSldRules()
        {
            xmlFile = File.ReadAllText("xml\\AkvakulturGodkjenteLokaliteter.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal(8, slds.Count);
        }

        [Fact]
        public void ShouldParseSirkel()
        {
            xmlFile = File.ReadAllText("xml\\punkt_sirkel.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("point", slds[0].Symbolizer);
            Assert.Equal("circle", slds[0].WellKnownName);
            Assert.Equal("#00ff0c", slds[0].Fill);
            Assert.Equal("#000000", slds[0].Stroke);
            Assert.Equal("4", slds[0].StrokeWidth);
        }
    }
}
