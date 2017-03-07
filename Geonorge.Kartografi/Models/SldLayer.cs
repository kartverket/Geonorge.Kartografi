using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class SldLayer
    {
        public string Name { get; set; }
        public List<SldRule> Rules { get; set; }
    }
}