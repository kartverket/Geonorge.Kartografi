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

        public static readonly Dictionary<string, string> Compatibility = new Dictionary<string, string>()
        {
            {"wms", "wms"},
            {"qgis", "qgis"}
        };

        public static readonly Dictionary<string, string> Status = new Dictionary<string, string>()
        {
            {"Draft", "Utkast"},
            {"NotAccepted", "Ikke godkjent"},
            {"Retired", "Utgått"},
            {"Sosi-valid", "SOSI godkjent"},
            {"Submitted", "Sendt inn"},
            {"Superseded", "Erstattet"},
            {"Valid", "Gyldig"},
        };

    }

}
