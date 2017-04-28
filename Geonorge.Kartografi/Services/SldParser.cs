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
        public List<SldLayer> Get(string url)
        {
            XDocument sldDoc = new XDocument();
            List<SldLayer> sldLayers = new List<SldLayer>();
            try
            {
                sldDoc = XDocument.Load(url);
                string version = sldDoc.Root.Attribute("version").Value;
                version = version.Substring(0, 3);

                //if (version == "1.0")
                //    sldLayers = new SldParser10().Parse(sldDoc); //sld 1.0 not fully supported
                //else
                    sldLayers = new SldParser11().Parse(sldDoc);

            }
            catch{}

            return sldLayers;

        }

    }
}