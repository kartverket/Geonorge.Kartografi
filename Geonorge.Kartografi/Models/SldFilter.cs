using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Geonorge.Kartografi.Models
{
    public class SldFilter
    {
        public string PropertyName { get; set; }
        public string Literal { get; set; }
        public XElement FilterObject { get; set; }
    }
}