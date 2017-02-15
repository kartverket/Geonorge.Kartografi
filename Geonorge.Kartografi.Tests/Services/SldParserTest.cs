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
    }
}
