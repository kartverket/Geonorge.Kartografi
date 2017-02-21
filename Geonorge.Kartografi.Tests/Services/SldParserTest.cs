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

        [Fact]
        public void ShouldParseSquare()
        {
            xmlFile = File.ReadAllText("xml\\punkt_firkant.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("point", slds[0].Symbolizer);
            Assert.Equal("square", slds[0].WellKnownName);
            Assert.Equal("#00ff0c", slds[0].Fill);
            Assert.Equal("#000000", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParseTriangle()
        {
            xmlFile = File.ReadAllText("xml\\punkt_triangel.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("point", slds[0].Symbolizer);
            Assert.Equal("triangle", slds[0].WellKnownName);
            Assert.Equal("#00ff0c", slds[0].Fill);
            Assert.Equal("#000000", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParseKryss2()
        {
            xmlFile = File.ReadAllText("xml\\punkt_kryss2.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("point", slds[0].Symbolizer);
            Assert.Equal("cross", slds[0].WellKnownName);
            Assert.Equal("#00ff0c", slds[0].Fill);
            Assert.Equal("#000000", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParseKryss()
        {
            xmlFile = File.ReadAllText("xml\\punkt_kryss.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("point", slds[0].Symbolizer);
            Assert.Equal("cross_fill", slds[0].WellKnownName);
            Assert.Equal("#00ff0c", slds[0].Fill);
            Assert.Equal("#000000", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParseX()
        {
            xmlFile = File.ReadAllText("xml\\punkt_x.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("point", slds[0].Symbolizer);
            Assert.Equal("cross2", slds[0].WellKnownName);
            Assert.Equal("#00ff0c", slds[0].Fill);
            Assert.Equal("#000000", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParsePolygonSolid()
        {
            xmlFile = File.ReadAllText("xml\\polygon_solid.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("polygon", slds[0].Symbolizer);
            Assert.Equal("", slds[0].WellKnownName);
            Assert.Equal("#c20003", slds[0].Fill);
            Assert.Equal("#000001", slds[0].Stroke);
            Assert.Equal("1", slds[0].StrokeWidth);
        }

        [Fact]
        public void ShouldParsePolygonVertline()
        {
            xmlFile = File.ReadAllText("xml\\polygon_vertline.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("polygon", slds[0].Symbolizer);
            Assert.Equal("line", slds[0].WellKnownName);
            Assert.Equal("#000001", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParsePolygonHorline()
        {
            xmlFile = File.ReadAllText("xml\\polygon_horline.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("polygon", slds[0].Symbolizer);
            Assert.Equal("horline", slds[0].WellKnownName);
            Assert.Equal("#000001", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParsePolygonCross()
        {
            xmlFile = File.ReadAllText("xml\\polygon_kryss.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("polygon", slds[0].Symbolizer);
            Assert.Equal("cross", slds[0].WellKnownName);
            Assert.Equal("#000001", slds[0].Stroke);
        }

        [Fact]
        public void ShouldParsePolygonSlash()
        {
            xmlFile = File.ReadAllText("xml\\polygon_slash.sld");
            XDocument doc = XDocument.Parse(xmlFile);
            List<SldRule> slds = new SldParser().Parse(doc);
            Assert.NotNull(slds);
            Assert.Equal("polygon", slds[0].Symbolizer);
            Assert.Equal("slash", slds[0].WellKnownName);
            Assert.Equal("#000001", slds[0].Stroke);
        }

    }
}
