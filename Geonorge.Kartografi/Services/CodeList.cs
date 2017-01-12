using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Services
{
    public static class CodeList
    {
        public static readonly Dictionary<string, string> Formats = new Dictionary<string, string>()
        {
            {"sld", "sld"},
            {"lyr", "lyr"}
        };
    }

}
