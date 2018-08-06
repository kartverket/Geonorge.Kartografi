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
            {"qml", "qml"},
            {"lyr", "lyr"}
        };

        public static readonly Dictionary<string, string> Compatibility = new Dictionary<string, string>()
        {
            {"WMS", "WMS"},
            {"QGIS", "QGIS"},
            {"ESRI", "ESRI"}
        };

        public static readonly Dictionary<string, string> Status = new Dictionary<string, string>()
        {
            {"Submitted", "Sendt inn"},
            {"Accepted", "Godkjent"},
            {"Superseded", "Erstattet"},
            {"Retired", "Utgått"},
        };

        public static readonly Dictionary<string, string> StatusEnglish = new Dictionary<string, string>()
        {
            {"Submitted", "Submitted"},
            {"Accepted", "Accepted"},
            {"Superseded", "Superseded"},
            {"Retired", "Retired"},
        };

    }

}
