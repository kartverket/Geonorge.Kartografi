using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Geonorge.Kartografi.Models;
using Geonorge.Kartografi.Services;
using System.IO;
using Xunit;

namespace Geonorge.Kartografi.Tests.Services
{
    public class SldParserTest
    {
        private static List<SldRule> ParseFile(string fileName)
        {
            XDocument doc = XDocument.Parse(File.ReadAllText("xml\\" + fileName));
            List<SldLayer> layers = new SldParser().Parse(doc);
            return layers[0].Rules;
        }

        private static List<SldLayer> ParseSld10(string rules)
        {
            string sld = @"<StyledLayerDescriptor version=""1.0.0"" xmlns=""http://www.opengis.net/sld""
                xmlns:ogc=""http://www.opengis.net/ogc"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
              <NamedLayer><Name>Testlag</Name><UserStyle>" + rules + @"</UserStyle></NamedLayer>
            </StyledLayerDescriptor>";
            return new SldParser().Parse(XDocument.Parse(sld), "https://example.com/files/test.sld");
        }

        [Fact]
        public void ShouldParseSldRules()
        {
            List<SldRule> slds = ParseFile("AkvakulturGodkjenteLokaliteter.sld");
            Assert.Equal(8, slds.Count);
            Assert.Equal("avKlasse = 'Matfisk_laksefisk'", slds[1].Filter);
        }

        [Fact]
        public void ShouldParseSirkel()
        {
            var graphic = ParseFile("punkt_sirkel.sld")[0].Symbolizers[0].Graphic;
            Assert.Equal("circle", graphic.WellKnownName);
            Assert.Equal("#00ff0c", graphic.Fill.Color);
            Assert.Equal("#000000", graphic.Stroke.Color);
            Assert.Equal(4, graphic.Stroke.Width);
        }

        [Fact]
        public void ShouldParseSquare()
        {
            var symbolizer = ParseFile("punkt_firkant.sld")[0].Symbolizers[0];
            Assert.Equal("point", symbolizer.Type);
            Assert.Equal("square", symbolizer.Graphic.WellKnownName);
            Assert.Equal("#00ff0c", symbolizer.Graphic.Fill.Color);
            Assert.Equal("#000000", symbolizer.Graphic.Stroke.Color);
        }

        [Fact]
        public void ShouldParseTriangle()
        {
            Assert.Equal("triangle", ParseFile("punkt_triangel.sld")[0].Symbolizers[0].Graphic.WellKnownName);
        }

        [Fact]
        public void ShouldParseKryss2()
        {
            Assert.Equal("cross", ParseFile("punkt_kryss2.sld")[0].Symbolizers[0].Graphic.WellKnownName);
        }

        [Fact]
        public void ShouldParseKryss()
        {
            Assert.Equal("cross_fill", ParseFile("punkt_kryss.sld")[0].Symbolizers[0].Graphic.WellKnownName);
        }

        [Fact]
        public void ShouldParseX()
        {
            var graphic = ParseFile("punkt_x.sld")[0].Symbolizers[0].Graphic;
            Assert.Equal("cross2", graphic.WellKnownName);
            Assert.Equal(7, graphic.Size);
        }

        [Fact]
        public void ShouldParsePolygonSolid()
        {
            var symbolizer = ParseFile("polygon_solid.sld")[0].Symbolizers[0];
            Assert.Equal("polygon", symbolizer.Type);
            Assert.Null(symbolizer.Fill.Graphic);
            Assert.Equal("#c20003", symbolizer.Fill.Color);
            Assert.Equal("#000001", symbolizer.Stroke.Color);
            Assert.Equal(1, symbolizer.Stroke.Width);
        }

        [Theory]
        [InlineData("polygon_vertline.sld", "line")]
        [InlineData("polygon_horline.sld", "horline")]
        [InlineData("polygon_kryss.sld", "cross")]
        [InlineData("polygon_slash.sld", "slash")]
        [InlineData("polygon_backslash.sld", "backslash")]
        [InlineData("polygon_x.sld", "x")]
        public void ShouldParsePolygonPatternFill(string fileName, string wellKnownName)
        {
            var symbolizer = ParseFile(fileName)[0].Symbolizers[0];
            Assert.Equal("polygon", symbolizer.Type);
            Assert.Equal(wellKnownName, symbolizer.Fill.Graphic.WellKnownName);
            Assert.Equal("#c20003", symbolizer.Fill.Graphic.Stroke.Color);
        }

        [Fact]
        public void ShouldParseExternalGraphic()
        {
            var graphic = ParseFile("punkt_svg.sld")[0].Symbolizers[0].Graphic;
            Assert.Equal(@"http://blackicemedia.com/presentations/2013-02-hires/img/awesome_tiger.svg", graphic.ExternalGraphicHref);
        }

        [Fact]
        public void ShouldParseLineHeltrukket()
        {
            var symbolizer = ParseFile("linje_heltrukket.sld")[0].Symbolizers[0];
            Assert.Equal("line", symbolizer.Type);
            Assert.Equal("#d6cd68", symbolizer.Stroke.Color);
            Assert.Null(symbolizer.Stroke.Dasharray);
        }

        [Fact]
        public void ShouldParseLineHelpunkt()
        {
            var stroke = ParseFile("linje_helpunkt.sld")[0].Symbolizers[0].Stroke;
            Assert.Equal("#d64c5c", stroke.Color);
            Assert.Equal("4 2 1 2", stroke.Dasharray);
        }

        [Fact]
        public void ShouldParseSld10WithCssParameters()
        {
            var layers = ParseSld10(@"
                <FeatureTypeStyle><Rule>
                  <Name>r1</Name><Title>Vei</Title>
                  <PolygonSymbolizer>
                    <Fill><CssParameter name=""fill""><ogc:Literal>#aabbcc</ogc:Literal></CssParameter>
                          <CssParameter name=""fill-opacity"">0.5</CssParameter></Fill>
                  </PolygonSymbolizer>
                  <LineSymbolizer><Stroke><CssParameter name=""stroke"">#112233</CssParameter>
                          <CssParameter name=""stroke-width"">2.5</CssParameter></Stroke></LineSymbolizer>
                </Rule></FeatureTypeStyle>");

            Assert.Equal("Testlag", layers[0].Name);
            var rule = layers[0].Rules[0];
            Assert.Equal("Vei", rule.Label);
            Assert.Equal(2, rule.Symbolizers.Count);
            Assert.Equal("#aabbcc", rule.Symbolizers[0].Fill.Color);
            Assert.Equal(0.5, rule.Symbolizers[0].Fill.Opacity);
            Assert.Equal("#112233", rule.Symbolizers[1].Stroke.Color);
            Assert.Equal(2.5, rule.Symbolizers[1].Stroke.Width);
        }

        [Fact]
        public void ShouldMergeRulesFromSeveralFeatureTypeStyles()
        {
            var layers = ParseSld10(@"
                <FeatureTypeStyle><Rule><Name>Motorvei</Name>
                  <LineSymbolizer><Stroke><CssParameter name=""stroke"">#000000</CssParameter><CssParameter name=""stroke-width"">6</CssParameter></Stroke></LineSymbolizer>
                </Rule></FeatureTypeStyle>
                <FeatureTypeStyle><Rule><Name>Motorvei</Name>
                  <LineSymbolizer><Stroke><CssParameter name=""stroke"">#ff0000</CssParameter><CssParameter name=""stroke-width"">4</CssParameter></Stroke></LineSymbolizer>
                </Rule></FeatureTypeStyle>");

            var rule = Assert.Single(layers[0].Rules);
            Assert.Equal(new[] { "#000000", "#ff0000" }, rule.Symbolizers.Select(s => s.Stroke.Color));
        }

        [Fact]
        public void ShouldNotFailOnMissingParameters()
        {
            var layers = ParseSld10(@"
                <FeatureTypeStyle><Rule>
                  <PolygonSymbolizer><Fill/></PolygonSymbolizer>
                  <PointSymbolizer><Graphic><Mark><WellKnownName>shape://Star</WellKnownName><Fill/></Mark><Size>abc</Size></Graphic></PointSymbolizer>
                  <TextSymbolizer><Label><ogc:PropertyName>navn</ogc:PropertyName></Label></TextSymbolizer>
                </Rule></FeatureTypeStyle>");

            var symbolizers = layers[0].Rules[0].Symbolizers;
            Assert.Equal("#808080", symbolizers[0].Fill.Color);
            Assert.Equal("star", symbolizers[1].Graphic.WellKnownName);
            Assert.Null(symbolizers[1].Graphic.Size);
            Assert.Equal("text", symbolizers[2].Type);
        }

        [Fact]
        public void ShouldResolveRelativeExternalGraphicAndRejectOtherSchemes()
        {
            var layers = ParseSld10(@"
                <FeatureTypeStyle>
                  <Rule><PointSymbolizer><Graphic><ExternalGraphic><OnlineResource xlink:href=""symboler/kirke.svg""/><Format>image/svg+xml</Format></ExternalGraphic></Graphic></PointSymbolizer></Rule>
                  <Rule><PointSymbolizer><Graphic><ExternalGraphic><OnlineResource xlink:href=""javascript:alert(1)""/></ExternalGraphic></Graphic></PointSymbolizer></Rule>
                </FeatureTypeStyle>");

            Assert.Equal("https://example.com/files/symboler/kirke.svg", layers[0].Rules[0].Symbolizers[0].Graphic.ExternalGraphicHref);
            Assert.Null(layers[0].Rules[1].Symbolizers[0].Graphic.ExternalGraphicHref);
        }

        [Fact]
        public void ShouldFormatFilters()
        {
            var layers = ParseSld10(@"
                <FeatureTypeStyle>
                  <Rule><ogc:Filter><ogc:And>
                      <ogc:PropertyIsGreaterThanOrEqualTo><ogc:PropertyName>areal</ogc:PropertyName><ogc:Literal>10</ogc:Literal></ogc:PropertyIsGreaterThanOrEqualTo>
                      <ogc:Or>
                        <ogc:PropertyIsLike wildCard=""*"" singleChar=""."" escape=""!""><ogc:PropertyName>navn</ogc:PropertyName><ogc:Literal>Ny*</ogc:Literal></ogc:PropertyIsLike>
                        <ogc:PropertyIsNull><ogc:PropertyName>navn</ogc:PropertyName></ogc:PropertyIsNull>
                      </ogc:Or>
                  </ogc:And></ogc:Filter></Rule>
                  <Rule><ElseFilter/></Rule>
                </FeatureTypeStyle>");

            Assert.Equal("areal ≥ 10 AND (navn LIKE 'Ny*' OR navn IS NULL)", layers[0].Rules[0].Filter);
            Assert.True(layers[0].Rules[1].IsElseFilter);
        }

        [Fact]
        public void ShouldRenderAllSymbolizersAsSvg()
        {
            var layers = ParseSld10(@"
                <FeatureTypeStyle><Rule>
                  <PolygonSymbolizer>
                    <Fill><GraphicFill><Graphic><Mark><WellKnownName>shape://slash</WellKnownName>
                      <Stroke><CssParameter name=""stroke"">#0000ff</CssParameter></Stroke></Mark></Graphic></GraphicFill></Fill>
                    <Stroke><CssParameter name=""stroke"">#ff0000</CssParameter><CssParameter name=""stroke-dasharray"">2 1</CssParameter></Stroke>
                  </PolygonSymbolizer>
                </Rule></FeatureTypeStyle>");

            string svg = new SldLegendRenderer().Render(layers[0].Rules[0]);
            var element = XElement.Parse(svg);
            XNamespace ns = "http://www.w3.org/2000/svg";

            Assert.Equal(ns + "svg", element.Name);
            var pattern = element.Descendants(ns + "pattern").Single();
            var rect = element.Descendants(ns + "rect").Single();
            Assert.Equal("url(#" + (string)pattern.Attribute("id") + ")", (string)rect.Attribute("fill"));
            Assert.Equal("#ff0000", (string)rect.Attribute("stroke"));
            Assert.Equal("8 4", (string)rect.Attribute("stroke-dasharray"));
        }

        [Fact]
        public void ShouldReturnEmptyListForInvalidUrl()
        {
            Assert.Empty(new SldParser().Get("not-a-valid-file.sld"));
        }
    }
}
